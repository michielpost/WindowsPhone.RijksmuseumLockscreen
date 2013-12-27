using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace RijksLockScreen.WP
{
  public partial class AboutPage : PhoneApplicationPage
  {
    public AboutPage()
    {
      InitializeComponent();
    }

    private void EmailButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      EmailComposeTask emailComposeTask = new EmailComposeTask();

      emailComposeTask.Subject = "Rijksmuseum Lockscreen";
      emailComposeTask.To = "michiel@michielpost.nl";
      emailComposeTask.Show();

    }
  }
}