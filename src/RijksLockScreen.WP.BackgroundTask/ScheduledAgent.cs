using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Windows.Phone.System.UserProfile;
using System;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Info;

namespace RijksLockScreen.WP.BackgroundTask
{
  public class ScheduledAgent : ScheduledTaskAgent
  {
    /// <remarks>
    /// ScheduledAgent constructor, initializes the UnhandledException handler
    /// </remarks>
    static ScheduledAgent()
    {
      // Subscribe to the managed exception handler
      Deployment.Current.Dispatcher.BeginInvoke(delegate
      {
        Application.Current.UnhandledException += UnhandledException;
      });
    }

    /// Code to execute on Unhandled Exceptions
    private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
    {
      if (Debugger.IsAttached)
      {
        // An unhandled exception has occurred; break into the debugger
        Debugger.Break();
      }
    }

    void FreeMemory()
    {

      GC.Collect();

      GC.WaitForPendingFinalizers();

      GC.Collect();

    }

    /// <summary>
    /// Agent that runs a scheduled task
    /// </summary>
    /// <param name="task">
    /// The invoked task
    /// </param>
    /// <remarks>
    /// This method is called when a periodic or resource intensive task is invoked
    /// </remarks>
    protected override async void OnInvoke(ScheduledTask task)
    {

      Debug.WriteLine("Start: {0}", DeviceStatus.ApplicationPeakMemoryUsage);

      //Only run when there is internet connection
      if (DeviceNetworkInformation.IsNetworkAvailable
                   && (DeviceNetworkInformation.IsWiFiEnabled || DeviceNetworkInformation.IsCellularDataEnabled))
      {

        if (LockScreenManager.IsProvidedByCurrentApplication)
        {
          string fileName = "lastUpdate";
          Q42.WinRT.Storage.StorageHelper<DateTime?> sh = new Q42.WinRT.Storage.StorageHelper<DateTime?>(Q42.WinRT.Storage.StorageType.Local);
          var lastUpdate = await sh.LoadAsync(fileName);

          if (!lastUpdate.HasValue
            || lastUpdate.Value.Date != DateTime.Now.Date)
          {

            // Get the URI of the lock screen background image.
            // NOTE: GetImageUri throws is the app is not the current application 
            var currentImage = LockScreen.GetImageUri();

            Debug.WriteLine("Start GetWeblUriAsync: {0}", DeviceStatus.ApplicationPeakMemoryUsage);
            var url = await CustomRijksService.GetWeblUriAsync();
            Debug.WriteLine("Finish GetWeblUriAsync: {0}", DeviceStatus.ApplicationPeakMemoryUsage);


            FreeMemory();

            string key = "A.jpg";
            if (currentImage.AbsolutePath.Contains("A.jpg"))
              key = "B.jpg";

            //Check if it's not already the current image
            //if (!currentImage.AbsolutePath.Contains(url.ToCacheKey()))
           // {
              //currentImage = null;
              Debug.WriteLine("Start GetLocalImagePath: {0}", DeviceStatus.ApplicationPeakMemoryUsage);
              await CustomRijksService.GetLocalImagePath(url, key);
              Debug.WriteLine("Finish GetLocalImagePath: {0}", DeviceStatus.ApplicationPeakMemoryUsage);

              url = null;

              FreeMemory();


              // Set the lock screen background image.
              Windows.Phone.System.UserProfile.LockScreen.SetImageUri(new Uri("ms-appdata:///Local/" + key, UriKind.Absolute));


              await sh.SaveAsync(DateTime.Now, fileName);


            //}

          }

        }
        //else
        //{
        //  // Do cleanup, since we are no longer the active lock screen provider.  
        //  // This could be: delete images, stop the agent, etc...    
        //  var periodicTask = ScheduledActionService.Find(task.Name) as PeriodicTask;
        //  if (periodicTask != null)
        //  {
        //    try
        //    {
        //      ScheduledActionService.Remove(task.Name);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //  }
        //}


        // If debugging is enabled, launch the agent again in one minute.
        // debug, so run in every 30 secs
#if(DEBUG)  
      //ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(15)); 
      //System.Diagnostics.Debug.WriteLine("Periodic task is started again: " + task.Name);
#endif

      }

      Debug.WriteLine("PEAK: {0}", DeviceStatus.ApplicationPeakMemoryUsage);

      // Call NotifyComplete to let the system know the agent is done working.
      NotifyComplete();

    }
  }
}