﻿using Newtonsoft.Json.Linq;
using Q42.RijksmuseumApi;
using Q42.RijksmuseumApi.Interfaces;
using Q42.RijksmuseumApi.Models;
using Q42.WinRT.Data;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RijksLockScreen.WP.Services
{
  public interface IRijksService
  {
    Task<ArtObjectDetails> GetArtObjectAsync();
    //Task<Uri> GetWeblUriAsync();
    Task<Uri> GetLocalImageUri(Uri webUri);
    Task<string> GetLocalImagePath(Uri webUri);
  }

  public class RijksService : IRijksService
  {
    public IRijksClient _client;
    private string _language;

    public const string _apiKey = "EWkwIpWi";

    public RijksService(string language = "en")
    {
      _client = new RijksClient(_apiKey, language);
      _language = language;
    }

    public async Task<ArtObjectDetails> GetArtObjectAsync()
    {
      var objectOfTheDay = await this.GetObjectOfTheDay();
      var currentObject = await _client.GetCollectionDetails(objectOfTheDay);
      var currentArtObject = currentObject.ArtObject;

      currentArtObject.WebImage.Url = currentArtObject.WebImage.Url.Replace("=s0", "=s768-c");

      return currentArtObject;
    }

    private async Task<string> GetObjectOfTheDay()
    {
      int dayOfTheYear = DateTime.Now.DayOfYear;

      string type = "schilderij";
      if (_language == "en")
        type = "painting";

      var result = await _client.GetCollection(new CollectionSearchRequest() { ImageOnly = true, TopPiecesOnly = true, Type = type }, page: dayOfTheYear, pageSize: 1);

      var obj =  result.ArtObjects.FirstOrDefault();

      if (obj != null)
        return obj.ObjectNumber;
      else
        return "SK-C-5";
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
