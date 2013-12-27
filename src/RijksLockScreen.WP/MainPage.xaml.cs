using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RijksLockScreen.WP.Resources;
using Microsoft.Phone.Scheduler;

namespace RijksLockScreen.WP
{
  public partial class MainPage : PhoneApplicationPage
  {
    PeriodicTask periodicTask; 
    string periodicTaskName = "PeriodicAgent";

    // Constructor
    public MainPage()
    {
      InitializeComponent();

      StartPeriodicAgent();

      // Sample code to localize the ApplicationBar
      //BuildLocalizedApplicationBar();
    }

    private void StartPeriodicAgent()
    {
      // is old task running, remove it   
      periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
      if (periodicTask != null)
      {
        try
        {
          ScheduledActionService.Remove(periodicTaskName);
        }
        catch (Exception)
        {
        }
      }
      // create a new task  
      periodicTask = new PeriodicTask(periodicTaskName);
      // load description from localized strings 
      periodicTask.Description = "This is Lockscreen image provider app.";
      // set expiration days    
      periodicTask.ExpirationTime = DateTime.Now.AddDays(14);
      try
      {
        // add this to scheduled action service 
        ScheduledActionService.Add(periodicTask);
        // debug, so run in every 30 secs
#if(DEBUG)   
        ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(15));     
        System.Diagnostics.Debug.WriteLine("Periodic task is started: " + periodicTaskName);
#endif
      }
      catch (InvalidOperationException exception)
      {
        if (exception.Message.Contains("BNS Error: The action is disabled"))
        {
          // load error text from localized strings   
          MessageBox.Show("Background agents for this application have been disabled by the user.");
        }
        if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
        {
          // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.   
        }
      }
      catch (SchedulerServiceException)
      {
        // No user action required.  
      }
    }


    // Sample code for building a localized ApplicationBar
    //private void BuildLocalizedApplicationBar()
    //{
    //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
    //    ApplicationBar = new ApplicationBar();

    //    // Create a new button and set the text value to the localized string from AppResources.
    //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
    //    appBarButton.Text = AppResources.AppBarButtonText;
    //    ApplicationBar.Buttons.Add(appBarButton);

    //    // Create a new menu item with the localized string from AppResources.
    //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
    //    ApplicationBar.MenuItems.Add(appBarMenuItem);
    //}


    /// <summary>
    /// Go to lockscreen settings
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void btnGoToLockSettings_Click(object sender, RoutedEventArgs e)
    {
      // Launch URI for the lock screen settings screen.
      var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
    }
  }
}