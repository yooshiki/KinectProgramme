//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Windows.Threading;
    using Microsoft.Win32;
    using System.Configuration;
   

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        bool bflg;                              // ボタンのフラグ
        DispatcherTimer timer;                  // タイマークラス
        int timecount;                          // タイムをカウントする
        FileSave fsave;                         // ファイルクラス
        bool[] Personflg = new bool[3600];                       // 人物が認識されたかを格納するフラグ
        bool[] flg1 = new bool[] { true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true};     // フラグ

        bool[] flg2= new bool[] { true, true, true, true, true, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true};

        bool[] flg3 = new bool[] { true, true, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true};

        bool[] flg4 = new bool[] { false, true, false, false, false, false, false, false, 
                                false, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true};

        bool[] flg5 = new bool[] { true, true, true, true, true, true, true, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true};

        bool[] flg6 = new bool[] { false, false, false, false, false, false, false, false, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, true, false, true, false, true, 
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true,
                                true, false, true, false, true, true, false, true}; 


        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            fsave = new FileSave();
            timer = new DispatcherTimer();               // タイマーをインスタンス化
            timer.Interval = new TimeSpan(0, 0, 1);      // 1秒毎処理する
            timecount = 0;                               // タイムカウントの初期化
            timer.Tick += new EventHandler(timer_Tick);  // タイマーイベントの追加
            //timer.Start();                               // タイマーの動作を開始
            bflg = false;
            Button1.Content = "記録開始";
            Title = "Team Notebook 1123043 石田悠 1123085 木村導 1023066 桑山英明";
        }

        // タイマーイベント
        void timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(timecount + "秒経過");
            
            // センサーが接続されていない時の例外処理
            //try
            //{
            fsave.SaveFile(Personflg[0], Personflg[1], Personflg[2],
                Personflg[3], Personflg[4], Personflg[5]);            
            //}
            //catch (Exception)
            //{
              //  MessageBox.Show("センサーが接続されていません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                //Close();
            //}

            //fsave.SaveFile(flg1[timecount], flg2[timecount], flg3[timecount], 
            //               flg4[timecount], flg5[timecount], flg6[timecount]);
            timecount++;
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Skeleton を使用する
                this.sensor.SkeletonStream.Enable();

                // Skeleton イベントメソッド
                this.sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        // Skeleton イベントメソッド
        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // Skeleton配列の定義と初期化
            Skeleton[] Person = new Skeleton[0];

            using (SkeletonFrame skeltonFrame = e.OpenSkeletonFrame())
            {
                if (skeltonFrame != null)
                {
                    // 骨格情報の取得できるデータ数を取得
                    Person = new Skeleton[skeltonFrame.SkeletonArrayLength];
                    // Kinect から骨格情報を取得
                    skeltonFrame.CopySkeletonDataTo(Person);
                    // 人物が認識されたかを配列に格納する
                    Personflg = new bool[skeltonFrame.SkeletonArrayLength];
                }
            }

            if (Person.Length != 0)
            {
                int Personcount = 0;

                foreach (Skeleton skeleton in Person)
                {
                    // 配列に真偽を格納する
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked ||
                        skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                        Personflg[Personcount] = true;
                    else if (skeleton.TrackingState == SkeletonTrackingState.NotTracked)
                        Personflg[Personcount] = false;
                    Personcount++;
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        // 画像保存 //

        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
           
            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }
           
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();
            
            

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));
            
            // 保存先指定　ダイアログ //
            
            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);
            
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "画像の保存先指定";
            saveFileDialog1.InitialDirectory = @"C:\";
            
            string path = saveFileDialog1.FileName = "KinectSnapshot" + time;
            
            saveFileDialog1.Filter = "*.pug";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.CreatePrompt = true;
            

            //string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            //string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");
            
            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
            }

            catch (IOException)
            {
                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }

        // 記録開始のボタンが押された時のイベントメソッド
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (bflg == false)
            {
                timer.Start();                               // タイマーの動作を開始
                bflg = true;
            }
            else
            {
                bflg = false;
                timer.Stop();
            }
        }
    }
}