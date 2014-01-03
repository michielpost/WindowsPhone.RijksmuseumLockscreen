using Newtonsoft.Json.Linq;
using Q42.RijksmuseumApi;
using Q42.RijksmuseumApi.Interfaces;
using Q42.RijksmuseumApi.Models;
using Q42.WinRT.Data;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RijksLockScreen.WP.Services
{
  public interface IRijksService
  {
    Task<ArtObjectDetails> GetArtObjectAsync();
    Task<Uri> GetWeblUriAsync();
    Task<Uri> GetLocalImageUri(Uri webUri);
    Task<string> GetLocalImagePath(Uri webUri);
  }

  public class RijksService : IRijksService
  {
    public IRijksClient _client;

    public const string _apiKey = "EWkwIpWi";

    public RijksService(string language = "en")
    {
      _client = new RijksClient(_apiKey, language);
    }

    public async Task<ArtObjectDetails> GetArtObjectAsync()
    {
      var objectOfTheDay = await _client.GetObjectOfTheDay();
      var currentObject = await _client.GetCollectionDetails(objectOfTheDay);
      var currentArtObject = currentObject.ArtObject;

      currentArtObject.WebImage.Url = currentArtObject.WebImage.Url.Replace("=s0", "=s768-c");

      return currentArtObject;
    }

    public async Task<Uri> GetWeblUriAsync()
    {
      //var request = new Q42.RijksmuseumApi.Models.CollectionSearchRequest()
      //  {
      //    ImageOnly = true,
      //    TopPiecesOnly = true
      //  };

      ////Get current object id
      //var collection = await _client.GetCollection(request);
      //var currentObject = collection.ArtObjects.First();

      var objectOfTheDay = await _client.GetObjectOfTheDay();
      //var currentObject = await _client.GetCollectionDetails(objectOfTheDay);
      var url = await this.GetCollectionDetails(objectOfTheDay);
      objectOfTheDay = null;
      //var url = currentObject.ArtObject.WebImage.Url;

      url = url.Replace("=s0", "=s480-c");

      return new Uri(url);
    }

    public async Task<Uri> GetLocalImageUri(Uri webUri)
    {
      //First delete all old images
      try
      {
        await WebDataCache.ClearAll();

      }
      catch(Exception e) 
      {
      }

      //Download and save image
      var localUri = await WebDataCache.GetLocalUriAsync(webUri);

      return localUri;

    }

    public async Task<string> GetLocalImagePath(Uri webUri)
    {
      //First delete all old images
      //try
      //{
      //  await WebDataCache.ClearAll();

      //}
      //catch(Exception e) 
      //{
      //}

      //Download and save image
      var localUri = await WebDataCache.GetLocalUriAsync(webUri);

      return localUri.AbsolutePath;

    }

    /// <summary>
    /// https://www.rijksmuseum.nl/api/nl/collection/sk-c-5?key=fakekey&format=json
    /// </summary>
    /// <param name="objectNumber"></param>
    /// <returns></returns>
    public async Task<string> GetCollectionDetails(string objectNumber)
    {
      if (string.IsNullOrEmpty(objectNumber))
        throw new ArgumentNullException("objectNumber");

      //Create URL
      Uri uri = new Uri(string.Format("https://www.rijksmuseum.nl/api/nl/collection/{0}?key={1}&format=json", objectNumber, _apiKey));

      //Do HTTP Request
      HttpClient client = new HttpClient();
      string stringResult = await client.GetStringAsync(uri).ConfigureAwait(false);
      client = null; //Free memory
      uri = null; //Free memory

      //Parse JSON
      JObject jresponse = JObject.Parse(stringResult);
      var result = jresponse["artObject"]["webImage"]["url"].ToString();
      jresponse = null; //Free memory

      return result;
    }
  }
}
