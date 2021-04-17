using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Windows.Application;

namespace WPF_Thumbnails
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : Window
    {
        private readonly BitmapSource _source;
        public Typeface Typeface;
        private readonly DispatcherTimer _dispatcher = new DispatcherTimer();
        private Dictionary<string, BoxArtFrame> _frames = new Dictionary<string, BoxArtFrame>();

        public Export(ImageSource source)
        {
            InitializeComponent();
            StyleMode.SelectedIndex = Properties.Settings.Default.Style;
            _source = (BitmapSource) source;
            _frames = (Dictionary<string, BoxArtFrame>) typeof(Dictionary<string, BoxArtFrame>).GetXmlDictionary(DictionaryExtension.ReadXmlToString("frames.xml"));
            
            if (File.Exists("default_banner.png"))
            {
                var bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = new Uri(new FileInfo("default_banner.png").FullName);
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();
                banner_file.Source = bmi;
            }

            _dispatcher.Interval = TimeSpan.FromMilliseconds(500);

            _dispatcher.Tick += (s, a) =>
            {
                foreach (var boxArtFrame in ExportTabFrames.Items)
                {
                    var item = boxArtFrame as TabItem;

                    var editor = item?.Content as EditorWindow;
                    editor?.RenderFrames();
                }
                _dispatcher.Stop();
            };

            Typeface = new Typeface(new FontFamily("Museo Sans 500"), FontStyles.Normal, FontWeights.Regular,
                FontStretches.Normal);

            title_typeface.Text =
                $"@Typeface: ({Typeface.FontFamily},{Typeface.Style},{Typeface.Weight},{Typeface.Stretch})";


            foreach (var frame in _frames)
            {
                var editor = new EditorWindow();
                var tabFrame = new TabItem
                {
                    Header = frame.Key,
                    Name = "TabItem_" + frame.Key.Replace("x", "_").Replace(" ", "_"),
                    Style = FindResource("CrimsonTabStyle") as Style,
                    Content = editor
                };

                ExportTabFrames.Items.Add(tabFrame);
                editor.SetImage(_source, frame.Value, this);
            }
        }

        private void Chkb_bkg_title_Click(object sender, RoutedEventArgs e)
        {

            if (bkg_title_color != null) bkg_title_color.IsEnabled = chkb_bkg_title.IsChecked ?? false;
            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Bkg_title_color_Click(object sender, RoutedEventArgs e)
        {
            var color = new ColorDialog();
            if (color.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                bkg_title_color_box.Fill =
                    new SolidColorBrush(Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B));

                _dispatcher.Stop();
                _dispatcher.Start();
            }
        }

        private void Render_images_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action<Export>((s) =>
                {
                    var effect = new BlurEffect {Radius = 50};
                    s.Effect = effect;
                    foreach (var boxArtFrame in ExportTabFrames.Items)
                    {
                        var item = boxArtFrame as TabItem;
                        
                        var editor = item?.Content as EditorWindow;
                        editor?.RenderFrames();
                    }

                    s.Effect = null;
                }),
                this
            );
        }

        private void Fore_title_color_Click(object sender, RoutedEventArgs e)
        {
            var color = new ColorDialog();
            if (color.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fore_title_color_box.Fill =
                    new SolidColorBrush(Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B));
            }


            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Font_typeface_Click(object sender, RoutedEventArgs e)
        {

            var tp = FontViewer.GetTypeface();
            if (tp == default) return;

            Typeface = tp;
            title_typeface.Text =
                $"@Typeface: ({Typeface.FontFamily},{Typeface.Style},{Typeface.Weight},{Typeface.Stretch})";


            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Text_title_TextChanged(object sender, TextChangedEventArgs e)
        {
            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Title_shift_h_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Title_shift_v_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Font_size_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Logo_image_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = new Uri(ofd.FileName);
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();
                logo_file.Source = bmi;
            }

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Banner_image_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = new Uri(ofd.FileName);
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();
                banner_file.Source = bmi;
            }

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Logo_shift_w_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Logo_shift_h_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Logo_scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Export_images_Click(object sender, RoutedEventArgs e)
        {
            var folder = new FolderBrowserDialog();
            if (folder.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var ret = PictureType.Show();
            if (ret.Length <= 0) return;
            foreach (var boxArtFrame in ExportTabFrames.Items)
            {
                var item = boxArtFrame as TabItem;
                var editor = item?.Content as EditorWindow;
                var keyImage = (BitmapSource)editor?.KeyImage.Source;
                var titleImage = (BitmapSource)editor?.TitleImage.Source;
                if (keyImage == null || titleImage == null) continue;
                switch (ret[1])
                {
                    case ".png":
                        keyImage.SaveToPng(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ','_')}_Key{ret[1]}"));
                        titleImage.SaveToPng(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ', '_')}_Title{ret[1]}"));
                        break;
                    case ".jpg":
                        keyImage.SaveToJpeg(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ', '_')}_Key{ret[1]}"));
                        titleImage.SaveToJpeg(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ', '_')}{ret[1]}"));
                        break;
                    case ".jpeg":
                        keyImage.SaveToJpeg(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ', '_')}_Key{ret[1]}"));
                        titleImage.SaveToJpeg(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ', '_')}{ret[1]}"));
                        break;
                    default:
                        keyImage.SaveToBmp(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ', '_')}_Key{ret[1]}"));
                        titleImage.SaveToBmp(Path.Combine(folder.SelectedPath, $".\\{ret[0]}_{item.Header.ToString().Replace(' ', '_')}_Title{ret[1]}"));
                        break;
                }
            }
            
        }

        

        private void StyleMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Style = StyleMode.SelectedIndex;
            Properties.Settings.Default.Save();
            var dict = new ResourceDictionary();
            dict.Source = new Uri("Skin_" + WPF_Thumbnails.Properties.Settings.Default.Style + ".xaml",
                UriKind.Relative);
            Current.Resources["PrimaryColour"] = dict["PrimaryColour"];
            Current.Resources["SecondaryColour"] = dict["SecondaryColour"];
            Current.Resources["DrkPrimaryColour"] = dict["DrkPrimaryColour"];
            Current.Resources["DrkSecondaryColour"] = dict["DrkSecondaryColour"];
        }


        private void Chkb_otln_Click(object sender, RoutedEventArgs e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Chkb_drpshdw_Click(object sender, RoutedEventArgs e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }

        private void Chkb_centering_Click(object sender, RoutedEventArgs e)
        {

            _dispatcher.Stop();
            _dispatcher.Start();
        }
    }
}
