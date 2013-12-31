using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Windows.Phone.System.UserProfile;
using RijksLockScreen.WP.Services;
using Q42.WinRT;
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
#if(DEBUG) 
      var a = DeviceStatus.ApplicationCurrentMemoryUsage;
      if (a != null) { }
#endif

      //Only run when there is internet connection
      if (DeviceNetworkInformation.IsNetworkAvailable
                   && (DeviceNetworkInformation.IsWiFiEnabled || DeviceNetworkInformation.IsCellularDataEnabled))
      {

        if (LockScreenManager.IsProvidedByCurrentApplication)
        {
          string fileName = "lastUpdate";
          Q42.WinRT.Storage.StorageHelper<DateTime?> sh = new Q42.WinRT.Storage.StorageHelper<DateTime?>(Q42.WinRT.Storage.StorageType.Local);
          var lastUpdate = await sh.LoadAsync(fileName);

#if(DEBUG)
          a = DeviceStatus.ApplicationCurrentMemoryUsage;
          if (a != null) { }
#endif

          if (!lastUpdate.HasValue
            || lastUpdate.Value.Date != DateTime.Now.Date)
          {

            // Get the URI of the lock screen background image.
            // NOTE: GetImageUri throws is the app is not the current application 
            var currentImage = LockScreen.GetImageUri();

            var rijksService = new RijksService();
            var url = await rijksService.GetWeblUriAsync();

 #if(DEBUG)  
            a = DeviceStatus.ApplicationCurrentMemoryUsage;
            if (a != null) { }
#endif

            //Check if it's not already the current image
            if (!currentImage.AbsolutePath.Contains(url.ToCacheKey()))
            {
              var localUri = rijksService.GetLocalImageUri(url);
              await LockHelper.SetLock(url.AbsolutePath, false);

              await sh.SaveAsync(DateTime.Now, fileName);

#if(DEBUG)  
              a = DeviceStatus.ApplicationCurrentMemoryUsage;
              if (a != null) { }
#endif
            }

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

      // Call NotifyComplete to let the system know the agent is done working.
      NotifyComplete();

    }
  }
}