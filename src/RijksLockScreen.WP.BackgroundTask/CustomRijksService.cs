﻿using Microsoft.Phone.Info;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RijksLockScreen.WP.BackgroundTask
{

  public static class WebRequestExtensions
  {
    public static WebResponse GetResponse(this WebRequest request)
    {
      AutoResetEvent autoResetEvent = new AutoResetEvent(false);

      IAsyncResult asyncResult = request.BeginGetResponse(r => autoResetEvent.Set(), null);

      // Wait until the call is finished
      autoResetEvent.WaitOne();

      autoResetEvent = null;


      return request.EndGetResponse(asyncResult);
    }

    public static Stream GetRequestStream(this WebRequest request)
    {
      AutoResetEvent autoResetEvent = new AutoResetEvent(false);

      IAsyncResult asyncResult = request.BeginGetRequestStream(r => autoResetEvent.Set(), null);

      // Wait until the call is finished
      autoResetEvent.WaitOne();

      autoResetEvent = null;

      return request.EndGetRequestStream(asyncResult);
    }
  }



  public static class CustomRijksService
  {

    public static string _apiKey = "EWkwIpWi";


    public async static Task<Uri> GetWeblUriAsync()
    {
      Debug.WriteLine("Start GetWeblUriAsync {0}", DeviceStatus.ApplicationCurrentMemoryUsage);

      //var currentObject = await _client.GetCollectionDetails(objectOfTheDay);
      var url = await GetObjectOfTheDayUrl();
      //var url = currentObject.ArtObject.WebImage.Url;

      url = url.Replace("=s0", "=s480-c");

      return new Uri(url);
    }

    private static string GetWebResponse(string url)
    {
      HttpWebRequest request =
    (HttpWebRequest)HttpWebRequest.Create(url);
      var response = request.GetResponse();

      string stringResult = string.Empty;
      using (var reader = new StreamReader(response.GetResponseStream()))
      {
        stringResult = reader.ReadToEnd();
      }

      request = null;
      response = null;

      return stringResult;
    }


    public async static Task GetLocalImagePath(Uri uri, string key)
    {
      HttpWebRequest request =
   (HttpWebRequest)HttpWebRequest.Create(uri);
      var response = request.GetResponse().GetResponseStream();

      byte[] bytes;
      // if you want to read binary response
      using (var reader = new BinaryReader(response))
      {
        bytes = reader.ReadBytes((int)response.Length);
      }

      request = null;
      response = null;


      //Save data to cache
      var file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(key, Windows.Storage.CreationCollisionOption.ReplaceExisting);
      using (Stream stream = await file.OpenStreamForWriteAsync())
      {
        await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false); ;
      }

      bytes = null;
      file = null;
            
    }


    /// <summary>
    /// https://www.rijksmuseum.nl/api/nl/collection/sk-c-5?key=fakekey&format=json
    /// </summary>
    /// <param name="objectNumber"></param>
    /// <returns></returns>
    public async static Task<string> GetObjectOfTheDayUrl()
    {
      //Do HTTP Request
      string stringResult = GetWebResponse(string.Format("https://www.rijksmuseum.nl/api/nl/collection?key={0}&format=json&type=schilderij&p={1}&ps=1&imgonly=True&toppieces=True", _apiKey, DateTime.Now.DayOfYear));

      //Parse JSON
      JObject jresponse = JObject.Parse(stringResult);
      stringResult = null;
      var result = jresponse["artObjects"][0]["webImage"]["url"].ToString();
      jresponse = null; //Free memory

      return result;
    }

    
  }
}
