//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BackgroundRemovalBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.BackgroundRemoval;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private double dot;

        private double ddot;

        private double dddot;
        // 追加
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        // 追加
        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap foregroundBitmap;

        // 追加
        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        // 追加
        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensorChooser sensorChooser;

        /// <summary>
        /// Our core library which does background 
        /// </summary>
        private BackgroundRemovedColorStream backgroundRemovedColorStream;

        /// <summary>
        /// Intermediate storage for the skeleton data received from the sensor
        /// </summary>
        private Skeleton[] skeletons;

        /// <summary>
        /// the skeleton that is currently tracked by the app
        /// </summary>
        private int currentlyTrackedSkeletonId;

        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed;

        // 追加
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            // 追加-----------------------------------------------------------------------
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            //Image.Source = this.imageSource;

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            // Turn on the skeleton stream to receive skeleton frames
            this.sensor.SkeletonStream.Enable();
            // Add an event handler to be called whenever there is new color frame data
            this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
            //------------------------------------------------------------------------------

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += this.SensorChooserOnKinectChanged;
            this.sensorChooser.Start();
        }

        /// <summary>
        /// Finalizes an instance of the MainWindow class.
        /// This destructor will run only if the Dispose method does not get called.
        /// </summary>
        ~MainWindow()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose the allocated frame buffers and reconstruction.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees all memory associated with the FusionImageFrame.
        /// </summary>
        /// <param name="disposing">Whether the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (null != this.backgroundRemovedColorStream)
                {
                    this.backgroundRemovedColorStream.Dispose();
                    this.backgroundRemovedColorStream = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
            this.sensorChooser = null;
        }

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, or lingering events from previous sensor, do nothing here.
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect || this.sensorChooser.Kinect != sender)
            {
                return;
            }

            try
            {
                using (var depthFrame = e.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    }
                }

                using (var colorFrame = e.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                    }
                }

                using (var skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (null != skeletonFrame)
                    {
                        skeletonFrame.CopySkeletonDataTo(this.skeletons);
                        this.backgroundRemovedColorStream.ProcessSkeleton(this.skeletons, skeletonFrame.Timestamp);
                    }
                }

                this.ChooseSkeleton();
            }
            catch (InvalidOperationException)
            {
                // Ignore the exception. 
            }
        }

        /// <summary>
        /// Handle the background removed color frame ready event. The frame obtained from the background removed
        /// color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    if (null == this.foregroundBitmap || this.foregroundBitmap.PixelWidth != backgroundRemovedFrame.Width
                        || this.foregroundBitmap.PixelHeight != backgroundRemovedFrame.Height)
                    {
                        this.foregroundBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.MaskedColor.Source = this.foregroundBitmap;
                    }

                    // Write the pixel data into our bitmap
                    this.foregroundBitmap.WritePixels(
                        new Int32Rect(0, 0, this.foregroundBitmap.PixelWidth, this.foregroundBitmap.PixelHeight),
                        backgroundRemovedFrame.GetRawPixelData(),
                        this.foregroundBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        // 追加
        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                this.Backdrop.Visibility = Visibility.Visible;
                this.Adrop.Visibility = Visibility.Hidden;
                this.Cdrop.Visibility = Visibility.Hidden;
                this.Ddrop.Visibility = Visibility.Hidden;
                this.Edrop.Visibility = Visibility.Hidden;
                this.Fdrop.Visibility = Visibility.Hidden;
                this.Gdrop.Visibility = Visibility.Hidden;
                this.Hdrop.Visibility = Visibility.Hidden;
                this.label1.Content = "どこに行く?";

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {

                            Vector4 vec1, vec2;
                            vec1 = new Vector4();
                            vec2 = new Vector4();

                            vec1.X = skel.Joints[JointType.ElbowLeft].Position.X -
                                skel.Joints[JointType.ShoulderLeft].Position.X;
                            vec1.Y = skel.Joints[JointType.ElbowLeft].Position.Y -
                                skel.Joints[JointType.ShoulderLeft].Position.Y;
                            vec1.Z = skel.Joints[JointType.ElbowLeft].Position.Z -
                                skel.Joints[JointType.ShoulderLeft].Position.Z;

                            vec2.X = skel.Joints[JointType.ElbowLeft].Position.X -
                                skel.Joints[JointType.HandLeft].Position.X;
                            vec2.Y = skel.Joints[JointType.ElbowLeft].Position.Y -
                                skel.Joints[JointType.HandLeft].Position.Y;
                            vec2.Z = skel.Joints[JointType.ElbowLeft].Position.Z -
                                skel.Joints[JointType.HandLeft].Position.Z;

                            float AA, BB, AB;
                            AA = vec1.X * vec1.X + vec1.Y * vec1.Y + vec1.Z * vec1.Z;
                            BB = vec2.X * vec2.X + vec2.Y * vec2.Y + vec2.Z * vec2.Z;
                            AB = vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;
                            dot = (float)(AB / (System.Math.Sqrt(AA) * System.Math.Sqrt(BB)));
                            if (dot > 0 && dot < 0.5)
                            {
                                this.Adrop.Visibility = Visibility.Visible;
                                this.Backdrop.Visibility = Visibility.Hidden;
                                this.Cdrop.Visibility = Visibility.Hidden;
                                this.Ddrop.Visibility = Visibility.Hidden;
                                this.Edrop.Visibility = Visibility.Hidden;
                                this.Fdrop.Visibility = Visibility.Hidden;
                                this.Gdrop.Visibility = Visibility.Hidden;
                                this.Hdrop.Visibility = Visibility.Hidden;
                                this.label1.Content = "日本:富士山";
                            }
                            else if (dot > 0.5)
                            {
                                this.Ddrop.Visibility = Visibility.Visible;
                                this.Backdrop.Visibility = Visibility.Hidden;
                                this.Adrop.Visibility = Visibility.Hidden;
                                this.Cdrop.Visibility = Visibility.Hidden;
                                this.Edrop.Visibility = Visibility.Hidden;
                                this.Fdrop.Visibility = Visibility.Hidden;
                                this.Gdrop.Visibility = Visibility.Hidden;
                                this.Hdrop.Visibility = Visibility.Hidden;
                                this.label1.Content = "中国:万里の長城";
                            }
                        }
                    }
                    if (skeletons.Length != 0)
                    {
                        foreach (Skeleton skel in skeletons)
                        {

                            if (skel.TrackingState == SkeletonTrackingState.Tracked)
                            {

                                Vector4 vec3, vec4;
                                vec3 = new Vector4();
                                vec4 = new Vector4();

                                vec3.X = skel.Joints[JointType.ElbowRight].Position.X -
                                    skel.Joints[JointType.ShoulderRight].Position.X;
                                vec3.Y = skel.Joints[JointType.ElbowRight].Position.Y -
                                    skel.Joints[JointType.ShoulderRight].Position.Y;
                                vec3.Z = skel.Joints[JointType.ElbowRight].Position.Z -
                                    skel.Joints[JointType.ShoulderRight].Position.Z;

                                vec4.X = skel.Joints[JointType.ElbowRight].Position.X -
                                    skel.Joints[JointType.HandRight].Position.X;
                                vec4.Y = skel.Joints[JointType.ElbowRight].Position.Y -
                                    skel.Joints[JointType.HandRight].Position.Y;
                                vec4.Z = skel.Joints[JointType.ElbowRight].Position.Z -
                                    skel.Joints[JointType.HandRight].Position.Z;

                                float AA, BB, AB;
                                AA = vec3.X * vec3.X + vec3.Y * vec3.Y + vec3.Z * vec3.Z;
                                BB = vec4.X * vec4.X + vec4.Y * vec4.Y + vec4.Z * vec4.Z;
                                AB = vec3.X * vec4.X + vec3.Y * vec4.Y + vec3.Z * vec4.Z;
                                ddot = (float)(AB / (System.Math.Sqrt(AA) * System.Math.Sqrt(BB)));

                                if (ddot < 0.5 && ddot > 0)
                                {
                                    this.Cdrop.Visibility = Visibility.Visible;
                                    this.Backdrop.Visibility = Visibility.Hidden;
                                    this.Adrop.Visibility = Visibility.Hidden;
                                    this.Ddrop.Visibility = Visibility.Hidden;
                                    this.Edrop.Visibility = Visibility.Hidden;
                                    this.Fdrop.Visibility = Visibility.Hidden;
                                    this.Gdrop.Visibility = Visibility.Hidden;
                                    this.Hdrop.Visibility = Visibility.Hidden;
                                    this.label1.Content = "アメリカ:ホワイトハウス";
                                }else if (ddot > 0.5)
                                {
                                    this.Edrop.Visibility = Visibility.Visible;
                                    this.Backdrop.Visibility = Visibility.Hidden;
                                    this.Adrop.Visibility = Visibility.Hidden;
                                    this.Cdrop.Visibility = Visibility.Hidden;
                                    this.Ddrop.Visibility = Visibility.Hidden;
                                    this.Fdrop.Visibility = Visibility.Hidden;
                                    this.Gdrop.Visibility = Visibility.Hidden;
                                    this.Hdrop.Visibility = Visibility.Hidden;
                                    this.label1.Content = "ロシア:聖ワシリイ大聖堂";
                                }
                            }

                           
                        }
                    }
                    if (skeletons.Length != 0)
                    {
                        foreach (Skeleton skel in skeletons)
                        {

                            if (skel.TrackingState == SkeletonTrackingState.Tracked)
                            {

                                Vector4 vec5, vec6;
                                vec5 = new Vector4();
                                vec6 = new Vector4();

                                vec5.X = skel.Joints[JointType.KneeRight].Position.X -
                                    skel.Joints[JointType.HipRight].Position.X;
                                vec5.Y = skel.Joints[JointType.KneeRight].Position.Y -
                                    skel.Joints[JointType.HipRight].Position.Y;
                                vec5.Z = skel.Joints[JointType.KneeRight].Position.Z -
                                    skel.Joints[JointType.HipRight].Position.Z;

                                vec6.X = skel.Joints[JointType.KneeRight].Position.X -
                                    skel.Joints[JointType.FootRight].Position.X;
                                vec6.Y = skel.Joints[JointType.KneeRight].Position.Y -
                                    skel.Joints[JointType.FootRight].Position.Y;
                                vec6.Z = skel.Joints[JointType.KneeRight].Position.Z -
                                    skel.Joints[JointType.FootRight].Position.Z;

                                float AA, BB, AB;
                                AA = vec5.X * vec5.X + vec5.Y * vec5.Y + vec5.Z * vec5.Z;
                                BB = vec6.X * vec6.X + vec6.Y * vec6.Y + vec6.Z * vec6.Z;
                                AB = vec5.X * vec6.X + vec5.Y * vec6.Y + vec5.Z * vec6.Z;
                                dddot = (float)(AB / (System.Math.Sqrt(AA) * System.Math.Sqrt(BB)));

                                if (dddot > 0)
                                {
                                    this.Fdrop.Visibility = Visibility.Visible;
                                    this.Backdrop.Visibility = Visibility.Hidden;
                                    this.Adrop.Visibility = Visibility.Hidden;
                                    this.Cdrop.Visibility = Visibility.Hidden;
                                    this.Ddrop.Visibility = Visibility.Hidden;
                                    this.Edrop.Visibility = Visibility.Hidden;
                                    this.Gdrop.Visibility = Visibility.Hidden;
                                    this.Hdrop.Visibility = Visibility.Hidden;
                                    this.label1.Content = "エジプト:ピラミッド";
                                }
                                if (dot > 0.5 && ddot > 0.5)
                                {
                                    this.Gdrop.Visibility = Visibility.Visible;
                                    this.Backdrop.Visibility = Visibility.Hidden;
                                    this.Adrop.Visibility = Visibility.Hidden;
                                    this.Cdrop.Visibility = Visibility.Hidden;
                                    this.Ddrop.Visibility = Visibility.Hidden;
                                    this.Edrop.Visibility = Visibility.Hidden;
                                    this.Fdrop.Visibility = Visibility.Hidden;
                                    this.Hdrop.Visibility = Visibility.Hidden;
                                    this.label1.Content = "フランス:エッフェル塔";
                                }
                                if (dot < 0.5 && dot > 0 && ddot < 0.5 && ddot > 0)
                                {
                                    this.Hdrop.Visibility = Visibility.Visible;
                                    this.Backdrop.Visibility = Visibility.Hidden;
                                    this.Adrop.Visibility = Visibility.Hidden;
                                    this.Cdrop.Visibility = Visibility.Hidden;
                                    this.Ddrop.Visibility = Visibility.Hidden;
                                    this.Edrop.Visibility = Visibility.Hidden;
                                    this.Fdrop.Visibility = Visibility.Hidden;
                                    this.Gdrop.Visibility = Visibility.Hidden;
                                    this.label1.Content = "Yes We Can!!";
                                }
                            }


                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Use the sticky skeleton logic to choose a player that we want to set as foreground. This means if the app
        /// is tracking a player already, we keep tracking the player until it leaves the sight of the camera, 
        /// and then pick the closest player to be tracked as foreground.
        /// </summary>
        private void ChooseSkeleton()
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeleton = 0;

            foreach (var skel in this.skeletons)
            {
                if (null == skel)
                {
                    continue;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                {
                    isTrackedSkeltonVisible = true;
                    break;
                }

                if (skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    nearestSkeleton = skel.TrackingId;
                }
            }

            if (!isTrackedSkeltonVisible && nearestSkeleton != 0)
            {
                this.backgroundRemovedColorStream.SetTrackedPlayer(nearestSkeleton);
                this.currentlyTrackedSkeletonId = nearestSkeleton;
            }
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.AllFramesReady -= this.SensorAllFramesReady;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();

                    // Create the background removal stream to process the data and remove background, and initialize it.
                    if (null != this.backgroundRemovedColorStream)
                    {
                        this.backgroundRemovedColorStream.BackgroundRemovedFrameReady -= this.BackgroundRemovedFrameReadyHandler;
                        this.backgroundRemovedColorStream.Dispose();
                        this.backgroundRemovedColorStream = null;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthFormat);
                    args.NewSensor.ColorStream.Enable(ColorFormat);
                    args.NewSensor.SkeletonStream.Enable();

                    this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(args.NewSensor);
                    this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);

                    // Allocate space to put the depth, color, and skeleton data we'll receive
                    if (null == this.skeletons)
                    {
                        this.skeletons = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    }

                    // Add an event handler to be called when the background removed color frame is ready, so that we can
                    // composite the image and output to the app
                    this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;

                    // Add an event handler to be called whenever there is new depth frame data
                    args.NewSensor.AllFramesReady += this.SensorAllFramesReady;

                    try
                    {
                        args.NewSensor.DepthStream.Range = this.checkBoxNearMode.IsChecked.GetValueOrDefault()
                                                    ? DepthRange.Near
                                                    : DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    this.statusBarText.Text = Properties.Resources.ReadyForScreenshot;
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }


        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            int colorWidth = this.foregroundBitmap.PixelWidth;
            int colorHeight = this.foregroundBitmap.PixelHeight;

            // create a render target that we'll render our controls to
            var renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // render the backdrop
                var backdropBrush = new VisualBrush(Backdrop);
                dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));

                // render the color image masked out by players
                var colorBrush = new VisualBrush(MaskedColor);
                dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            var time = DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            var myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteFailed, path);
            }
        }

        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect)
            {
                return;
            }

            // will not function on non-Kinect for Windows devices
            try
            {
                this.sensorChooser.Kinect.DepthStream.Range = this.checkBoxNearMode.IsChecked.GetValueOrDefault()
                                                    ? DepthRange.Near
                                                    : DepthRange.Default;
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}