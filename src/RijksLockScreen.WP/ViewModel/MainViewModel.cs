using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Q42.RijksmuseumApi.Models;
using Q42.WinRT.Portable.Data;
using RijksLockScreen.WP.Resources;
using RijksLockScreen.WP.Services;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RijksLockScreen.WP.ViewModel
{
  /// <summary>
  /// This class contains properties that the main View can data bind to.
  /// <para>
  /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
  /// </para>
  /// <para>
  /// You can also use Blend to data bind with the tool's support.
  /// </para>
  /// <para>
  /// See http://www.galasoft.ch/mvvm
  /// </para>
  /// </summary>
  public class MainViewModel : ViewModelBase
  {
    private Uri _imageUri;

    public Uri ImageUri
    {
      get { return _imageUri; }
      set
      {
        _imageUri = value;
        RaisePropertyChanged(() => ImageUri);
        RaisePropertyChanged(() => ImageSource);

      }
    }

    public BitmapImage  ImageSource
    {
      get
      {
        if (ImageUri == null)
          return null;

        BitmapImage bimg = new BitmapImage();

        using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
        {
          using (IsolatedStorageFileStream stream = iso.OpenFile(ImageUri.PathAndQuery, FileMode.Open, FileAccess.Read))
          {
            bimg.SetSource(stream);
          }
        }

        return bimg;
      }
      
    }

    private ArtObjectDetails _artObject;

    public ArtObjectDetails ArtObject
    {
      get { return _artObject; }
      set
      {
        _artObject = value;
        RaisePropertyChanged(() => ArtObject);
      }
    }



    public DataLoader DataLoader { get; set; }

    public IRijksService RijksService { get; set; }

    public RelayCommand SetLockScreenCommand { get; set; }


    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    public MainViewModel()
    {
      RijksService = SimpleIoc.Default.GetInstance<IRijksService>();

      DataLoader = new DataLoader();

      SetLockScreenCommand = new RelayCommand(SetLockScreen);

      ////if (IsInDesignMode)
      ////{
      ////    // Code runs in Blend --> create design time data.
      ////}
      ////else
      ////{
      ////    // Code runs "for real"
      ////}

      Initialize();
    }

   

    private async Task Initialize()
    {
      var url = await DataLoader.LoadAsync(async () => {

        ArtObject = await RijksService.GetArtObjectAsync();

        var result = await RijksService.GetLocalImageUri(new Uri(ArtObject.WebImage.Url));

        return result;
        
      });

      ImageUri = url;

    }

    private async void SetLockScreen()
    {
      bool success = await LockHelper.SetLock(ImageUri.AbsolutePath, false);

      if(!success)
        MessageBox.Show(AppResources.UserOptionNo);
      else
        MessageBox.Show(AppResources.UserOptionYes);

    }

  }
}