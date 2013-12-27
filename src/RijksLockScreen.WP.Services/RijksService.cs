using Q42.RijksmuseumApi;
using Q42.RijksmuseumApi.Interfaces;
using Q42.WinRT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RijksLockScreen.WP.Services
{
  public interface IRijksService
  {
    Task<Uri> DownloadImageAndGetLocalUriAsync();
    Task<Uri> GetWeblUriAsync();
    Task<Uri> GetLocalImageUri(Uri webUri);
  }

  public class RijksService : IRijksService
  {
    public IRijksClient _client;

    public RijksService()
    {
      _client = new RijksClient("fpGQTuED");
    }

    public async Task<Uri> DownloadImageAndGetLocalUriAsync()
    {
      //Get current object id
      var url = await GetWeblUriAsync();

      var local = await GetLocalImageUri(url);

      return local;
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
      var currentObject = await _client.GetCollectionDetails(objectOfTheDay);
      var url = currentObject.ArtObject.WebImage.Url;

      url = url.Replace("=s0", "=s768-c");

      return new Uri(url);
    }

    public async Task<Uri> GetLocalImageUri(Uri webUri)
    {
      //First delete all old images
      try
      {
        await WebDataCache.ClearAll();

      }
      catch { }

      //Download and save image
      var localUri = await WebDataCache.GetLocalUriAsync(webUri);

      return localUri;

    }
  }
}
