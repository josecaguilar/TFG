using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using FacialRecognitionDoor.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FacialRecognitionDoor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public class Temperature
    {
        public WebcamHelper webcam;
        private StorageFile currentIdPhotoFile;

        public Temperature()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(15000); //15 seconds as Timestamp
            timer.Tick += Timer_Tick;
            timer.Start();
            
            // To line everything up for ease of reading back (on byte boundary) we 
            // will pad the command start bit with 7 leading "0" bits

            // Write 0000 000S GDDD xxxx xxxx xxxx
            // Read  ???? ???? ???? ?N98 7654 3210
            // S = start bit
            // G = Single / Differential
            // D = Chanel data 
            // ? = undefined, ignore
            // N = 0 "Null bit"
            // 9-0 = 10 data bits

            // 0000 01 = 7 pad bits, start bit
            // 1000 0000 = single ended, channel bit 2, channel bit 1, channel bit 0, 4 clocking bits
            // 0000 0000 = 8 clocking bits
            readBuffer = new byte[3] { 0x00, 0x00, 0x00 };
            writeBuffer = new byte[3] { 0x01, 0x80, 0x00 };
           
            InitSPI();
        }

        private async void InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;// 10000000;
                settings.Mode = SpiMode.Mode0; //Mode3;
                var controller = await SpiController.GetDefaultAsync();
                SpiDisplay = controller.GetDevice(settings);
            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }
        private async void Timer_Tick(object sender, object e)
        {
            if (livetemperature() >= 23)
            {
                TestPostMessage(livetemperature().ToString());
                var date = DateTime.Now;
                
                //Take photo and save into Intrusos
                if (webcam == null || !webcam.IsInitialized())
                {
                    // Initialize Webcam Helper
                    webcam = new WebcamHelper();
                    await webcam.InitializeCameraAsync();
                }
                currentIdPhotoFile = await webcam.CapturePhoto();
                // Create or open the folder in which the Whitelist is stored
                StorageFolder whitelistFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Intrusos", CreationCollisionOption.OpenIfExists);
                // Create a folder to store this specific user's photos
                StorageFolder currentFolder = await whitelistFolder.CreateFolderAsync(date.Day.ToString()+"-"+date.Month.ToString()+"-"+date.Year.ToString(), CreationCollisionOption.OpenIfExists);
                // Move the already captured photo the user's folder
                await currentIdPhotoFile.MoveAsync(currentFolder);
            }
            await Task.Run(async () => { await AzureIoTHub.SendTemperatureAsync(livetemperature()); });
        }
        public int livetemperature()
        {
            SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
            res = convertToInt(readBuffer);
            int mVolt = res * (2000 / 1023);
            res = mVolt / 10;
            return res;
        }

        public void TestPostMessage(string message)
        {
            string urlWithAccessToken = GeneralConstants.SlackURI;

            SlackClient client = new SlackClient(urlWithAccessToken);

            client.PostMessage(username: "IronDoor",
                       text: "Temperatura IronDoor 1: " + message + "ºC, la puerta ha sido comprometida, tomando fotos del intruso",
                       channel: "#status");
        }

        public int convertToInt(byte[] data)
        {
            int result = 0;
            /*mcp3008 10 bit output*/
            result = data[1] & 0x03;
            result <<= 8;
            result += data[2];
                    
            return result;
        }

        /*RaspBerry Pi2  Parameters*/
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */


        byte[] readBuffer = null;                           /* this is defined to hold the output data*/
        byte[] writeBuffer = null;                          /* we will hold the command to send to the chipbuild this in the constructor for the chip we are using */


        private SpiDevice SpiDisplay;

        // create a timer
        private DispatcherTimer timer;
        int res;
    }
}
