using System;
using System.Threading.Tasks;

namespace RijksLockScreen.WP.Services
{
  public static class LockHelper
  {
    public async static Task<bool> SetLock(string filePathOfTheImage, bool isAppResource)
    {
      try
      {
        var isProvider = Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication;
        if (!isProvider)
        {
          // If you're not the provider, this call will prompt the user for permission.
          // Calling RequestAccessAsync from a background agent is not allowed.
          var op = await Windows.Phone.System.UserProfile.LockScreenManager.RequestAccessAsync();

          // Only do further work if the access was granted.
          isProvider = op == Windows.Phone.System.UserProfile.LockScreenRequestResult.Granted;
        }

        if (isProvider)
        {
          // At this stage, the app is the active lock screen background provider.

          // The following code example shows the new URI schema.
          // ms-appdata points to the root of the local app data folder.
          // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package.
          var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
          var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);

          // Set the lock screen background image.
          Windows.Phone.System.UserProfile.LockScreen.SetImageUri(uri);

          // Get the URI of the lock screen background image.
          //var currentImage = Windows.Phone.System.UserProfile.LockScreen.GetImageUri();
          //System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());

          return true;
        }
        else
        {
          //MessageBox.Show("You said no, so I can't update your background.");
        }
      }
      catch (System.Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.ToString());
      }

      return false;
    }

  }
}
