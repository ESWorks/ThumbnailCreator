using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WPF_Thumbnails
{
    /// <summary>
    /// Interaction logic for FineAdjustmentControls.xaml
    /// </summary>
    public partial class FineAdjustmentControls : UserControl
    {
        //Prevent multiple warnings about size correction
        bool _warned;

        //Max Height of Image
        private double _imageHeight;
        //Max Width of Image
        private double _imageWidth;

        //Base Max of 100% scaling on shift width
        private double _shiftWidth;

        //Base Max of 100% scaling on shift height
        private double _shiftHeight;

        // destination sizes
        private double _cropWidth;
        private double _cropHeight;
        private int _cropScaleMin;

        //Image Shift Properties
        public double WidthAdjustment => ShiftWidth.Value;
        public double HeightAdjustment => ShiftHeight.Value;

        //Image Scale Properties
        public double ImageScale => ImageScaleSlider.Value / 100;

        //Override Typeface
        public Typeface Typeface { get; private set; }

        //Timer to prevent to many image action callbacks
        readonly DispatcherTimer _dispatch = new DispatcherTimer();

        //Callback <Void> function for timer
        public Action Callback { get; internal set; }

        public FineAdjustmentControls()
        {
            InitializeComponent();
            _dispatch.Interval = TimeSpan.FromMilliseconds(250);
            _dispatch.Tick += (s, a) => {
                Callback?.Invoke();
                _dispatch.Stop();
            };
        }

        /// <summary>
        /// Set base max and mins of controls
        /// </summary>
        /// <param name="source"></param>
        internal void SetImage(ImageSource source)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<FineAdjustmentControls>((s) =>
            {
                s._imageHeight = source.Height;
                s._imageWidth = source.Width;

                s._shiftWidth = source.Width - 5;
                s._shiftHeight = source.Height - 5;

                s.ShiftWidth.Maximum = _shiftWidth;
                s.ShiftWidth.Minimum = _shiftWidth * -1;

                s.ShiftHeight.Maximum = _shiftHeight;
                s.ShiftHeight.Minimum = _shiftHeight * -1;

                s.ImageScaleSlider.Value = 100;

                ImageCrop.Items.Remove(SC);
            }), this);
           
        }
        internal void SetImage(ImageSource source, int cropWidth, int cropHeight)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<FineAdjustmentControls>((s) =>
            {
                

                s._imageHeight = source.Height;
                s._imageWidth = source.Width;

                s._shiftWidth = source.Width - 5;
                s._shiftHeight = source.Height - 5;

                s.ShiftWidth.Maximum = _shiftWidth;
                s.ShiftWidth.Minimum = _shiftWidth * -1;

                s.ShiftHeight.Maximum = _shiftHeight;
                s.ShiftHeight.Minimum = _shiftHeight * -1;

                s.ImageScaleSlider.Value = 100;

                _cropHeight = cropHeight;
                _cropWidth = cropWidth;

                double scaledImageW = _imageWidth * ImageScale, scaledImageH = _imageHeight * ImageScale;
                double cropSliderX = scaledImageW, cropSliderY = scaledImageH;
                if (scaledImageW < _cropWidth)
                {
                    var rate = _cropWidth / scaledImageW;
                    cropSliderX = (int)scaledImageW * rate;
                    cropSliderY = (int)scaledImageH * rate;
                }
                else if (scaledImageH < _cropHeight)
                {
                    var rate = _cropHeight / scaledImageH;
                    cropSliderX = (int)scaledImageW * rate;
                    cropSliderY = (int)scaledImageH * rate;
                }

                double ratio = 1;
                double nextRatio = ratio;
                while ((int)(_imageHeight * nextRatio) > _cropHeight && (int)(_imageWidth * nextRatio) > _cropWidth)
                {
                    ratio = Math.Round(ratio - 0.01, 2);
                    nextRatio = Math.Round(ratio - 0.01, 2);
                }

                _cropScaleMin = (int)(ratio * 100);
                // set crop max
                s.CropY.Maximum = cropSliderY;
                s.CropX.Maximum = cropSliderX;

            }), this);

        }
        private void Shift_h_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized) return;
            if (e.NewValue != 0)
            {
                int adjH = (int)(_imageWidth - e.NewValue);
                if (adjH > _imageWidth || adjH < 0)
                {
                    if (!_warned) { MessageBox.Show("Shift is invalid as the cropped image would violate the bounds of the original. Image adjustments may be made to compensate."); _warned = true; }
                }
            }

            ShiftHLabel.Content = "Background Shift (H = " + (int)WidthAdjustment + ")";
            _dispatch.Stop();
            _dispatch.Start();
        }
        private void Shift_v_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized) return;
            if (e.NewValue != 0)
            {
                int adjV = (int)(_imageHeight - e.NewValue);
                if (adjV > _imageHeight || adjV < 0)
                {
                    if (!_warned) { MessageBox.Show("Shift is invalid as the cropped image would violate the bounds of the original. Image adjustments may be made to compensate."); _warned = true; }
                }
            }

            ShiftVLabel.Content = "Background Shift (V = " + (int)HeightAdjustment + ")";

            _dispatch.Stop();
            _dispatch.Start();
        }

        private void Scaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized) return;
            ScaleLabel.Content = "Background Scaling (" + (int)ImageScaleSlider.Value + "%)";

            //Create Current max height and width for shift based on ScaleValue
            int newMh = (int)(_shiftWidth * ImageScale);
            int newMnh = -1 * newMh;
            int newMw = (int)(_shiftHeight * ImageScale);
            int newMnw = -1 * newMw;

            //If current value is greater than the new max set to max, if current value is less than new min set to min, else retain current value. 
            ShiftWidth.Value = ShiftWidth.Value > newMw ? newMw : ShiftWidth.Value < newMnw ? newMnw : ShiftWidth.Value;
            ShiftHeight.Value = ShiftHeight.Value > newMh ? newMh : ShiftHeight.Value < newMnh ? newMnh : ShiftHeight.Value;

            //Set Maximums
            ShiftWidth.Maximum = newMw;
            ShiftHeight.Maximum = newMh;

            //Set Minimums
            ShiftWidth.Minimum = newMnw;
            ShiftHeight.Minimum = newMnh;

            if (ImageCrop.Items.Count > 2)
            {
                double scaledImageW = _imageWidth * ImageScale, scaledImageH = _imageHeight * ImageScale;
                double cropSliderX = scaledImageW, cropSliderY = scaledImageH;
                if (scaledImageW < _cropWidth)
                {
                    var rate = _cropWidth / scaledImageW;
                    cropSliderX = (int)scaledImageW * rate;
                    cropSliderY = (int)scaledImageH * rate;
                }
                else if (scaledImageH < _cropHeight)
                {
                    var rate = _cropHeight / scaledImageH;
                    cropSliderX = (int)scaledImageW * rate;
                    cropSliderY = (int)scaledImageH * rate;
                }

                // set crop max
                CropY.Maximum = cropSliderY;
                CropX.Maximum = cropSliderX;
                CropX.Value = CropX.Value > cropSliderX ? cropSliderX : CropX.Value;
                CropY.Value = CropY.Value > cropSliderY ? cropSliderY : CropY.Value;
            }

            //interupt current dispatch callback timer
            _dispatch.Stop();
            //start new dispatch callback timer
            _dispatch.Start();
        }
        private void Fore_title_color_Click(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            CropX.Value = 0;
            CropY.Value = 0;
            ShiftWidth.Value = 0;
            ShiftHeight.Value = 0;
            ImageScaleSlider.Value = 100;

            double scaledImageW = _imageWidth * ImageScale, scaledImageH = _imageHeight * ImageScale;
            double cropSliderX = scaledImageW, cropSliderY = scaledImageH;
            if (scaledImageW < _cropWidth)
            {
                var rate = _cropWidth / scaledImageW;
                cropSliderX = (int)scaledImageW * rate;
                cropSliderY = (int)scaledImageH * rate;
            }
            else if (scaledImageH < _cropHeight)
            {
                var rate = _cropHeight / scaledImageH;
                cropSliderX = (int)scaledImageW * rate;
                cropSliderY = (int)scaledImageH * rate;
            }

            // set crop max
            CropY.Maximum = cropSliderY;
            CropX.Maximum = cropSliderX;
        }

        private void ImageCrop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!IsInitialized) return;
            if (!((ComboBoxItem) ImageCrop.SelectedItem).Content.Equals(@"Scaled Crop"))
            {
                var uaf = ((ComboBoxItem) ImageCrop.SelectedItem).Content.Equals(@"Uniformed and Filled");
                ShiftWidth.Visibility = uaf ? Visibility.Visible : Visibility.Hidden;
                ShiftHeight.Visibility = uaf ? Visibility.Visible : Visibility.Hidden;
                ImageScaleSlider.Visibility = uaf ? Visibility.Visible : Visibility.Hidden;
                CropX.Visibility = Visibility.Hidden;
                CropY.Visibility = Visibility.Hidden;
                ShiftWidth.IsEnabled = uaf;
                ShiftHeight.IsEnabled = uaf;
                ImageScaleSlider.IsEnabled = uaf;
                CropX.IsEnabled = false;
                CropY.IsEnabled = false;
                ImageScaleSlider.Value = ImageScaleSlider.Value < 100 ? 100 : ImageScaleSlider.Value;
                ImageScaleSlider.Minimum = 100;
            }
            else
            {
                CropX.Visibility = Visibility.Visible;
                CropY.Visibility = Visibility.Visible;
                ShiftWidth.Visibility = Visibility.Hidden;
                ShiftHeight.Visibility = Visibility.Hidden;
                ImageScaleSlider.Visibility = Visibility.Visible;
                ShiftWidth.IsEnabled = false;
                ShiftHeight.IsEnabled = false;
                ImageScaleSlider.IsEnabled = true;
                CropX.IsEnabled = true;
                CropY.IsEnabled = true;
                ImageScaleSlider.Minimum = _cropScaleMin;


            }
            //interupt current dispatch callback timer
            _dispatch.Stop();
            //start new dispatch callback timer
            _dispatch.Start();
        }

        private void Crop_x_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CropXLabel.Content = "Position (X = " + (int)CropX.Value + ")";
            //interupt current dispatch callback timer
            _dispatch.Stop();
            //start new dispatch callback timer
            _dispatch.Start();

        }

        private void Crop_y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CropYLabel.Content = "Position (Y = " + (int)CropY.Value + ")";
            //interupt current dispatch callback timer
            _dispatch.Stop();
            //start new dispatch callback timer
            _dispatch.Start();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Typeface = new Typeface(new FontFamily("Museo Sans 500"), FontStyles.Normal, FontWeights.Regular,
                FontStretches.Normal);
            ov_title_typeface.Text =
                $"@Typeface: ({Typeface.FontFamily},{Typeface.Style},{Typeface.Weight},{Typeface.Stretch})";
        }

        private void TglTextOverride_Click(object sender, RoutedEventArgs e)
        {
            //interupt current dispatch callback timer
            _dispatch.Stop();
            //start new dispatch callback timer
            _dispatch.Start();
        }

        private void Ov_font_size_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //interupt current dispatch callback timer
            _dispatch.Stop();
            //start new dispatch callback timer
            _dispatch.Start();
        }

        private void Ov_font_typeface_Click(object sender, RoutedEventArgs e)
        {
            Typeface tp = FontViewer.GetTypeface();
            if (tp == default) return;
            ov_title_typeface.Text =
                $"@Typeface: ({tp.FontFamily},{tp.Style},{tp.Weight},{tp.Stretch})";
            Typeface = tp;
            _dispatch.Stop();
            _dispatch.Start();
        }
    }
}
