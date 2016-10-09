using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using FacialRecognitionDoor.Helpers;

namespace FacialRecognitionDoor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreditsPage : Page
    {
        //private WebcamHelper webcam;
        public CreditsPage()
        {
            this.InitializeComponent();
        }
            private void BackButton_Click(object sender, RoutedEventArgs e) //async void
            {
                //await webcam.StartCameraPreview();
                // Navigate back to the MainPage
                Frame.Navigate(typeof(MainPage));
            }       
    }
}

