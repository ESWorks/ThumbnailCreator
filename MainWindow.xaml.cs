using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media.Effects;
using System.Windows.Media;

namespace WPF_Thumbnails
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _dispatch = new DispatcherTimer();
        private Dictionary<string,BoxArtFrame> _frames = new Dictionary<string, BoxArtFrame>();
        private BitmapSource _default;
        public MainWindow()
        {
            InitializeComponent();
            _dispatch.Interval = TimeSpan.FromMilliseconds(250);
            _dispatch.Tick += (s, a) => {
                if (File.Exists(file_source.Text))
                {
                    try
                    {
                        source_start.Text = @"00:00:00.00";
                        source_end.Text = VideoLen();
                    }
                    catch
                    {
                    }
                }
                _dispatch.Stop();
            };
        }

        private string VideoLen()
        {

            Effect = new BlurEffect() { Radius = 15 };


            ProcessStartInfo lengthProcess = new ProcessStartInfo
            {
                UseShellExecute = false,
                ErrorDialog = false,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = ".\\ffmpeg.exe",
                Arguments = "-i \"" + file_source.Text + "\""
            };



            using (var proc = Process.Start(lengthProcess))
            {
                if (proc != null)
                {
                    var errorReader = proc.StandardError;
                    proc.WaitForExit();

                    Effect = null;

                    var result = errorReader.ReadToEnd();
                    return result.Substring(result.IndexOf("Duration: ", StringComparison.Ordinal) + ("Duration: ").Length, ("00:00:00.00").Length);
                }
            }

            return "Error";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter =
                    "MPEG Files|*.mpeg;*.mpg;*.MPG;*.MPEG|MKV Files|*.mkv;*.MKV|MOV Files|*.mov;*.MOV|MP4 Files|*.mp4;*.MP4|WMV Files|*.wmv;*.WMV|AVI Files|*.avi;*.AVI|MXF Files|*.mxf;*.MXF|*.* Any|*.*"
            };

            if (ofd.ShowDialog() == true) file_source.Text = ofd.FileName;
            _dispatch.Stop();
            try
            {
                string end = VideoLen();
                string[] endArray = end.Split(':', '.');
                TimeSpan span = new TimeSpan(0, int.Parse(endArray[0]), int.Parse(endArray[1]), int.Parse(endArray[2]), int.Parse(endArray[3]));
                slider_start.Value = 0;
                slider_start.Maximum = span.Ticks;
                slider_end.Maximum = span.Ticks;
                slider_end.Value = span.Ticks;
                source_end.Text = end;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                source_start.Text = @"00:00:00.00";
                slider_start.Value = 0;
                slider_start.Maximum = 0;
                slider_end.Value = 0;
                slider_end.Maximum = 0;
                source_end.Text = "Failed!";
            }
        }

        private void File_source_TextChanged(object sender, TextChangedEventArgs e)
        {
            _dispatch.Stop();
            _dispatch.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Effect = new BlurEffect() { Radius = 15 };
                BackgroundWorker worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                tb_prg.Value = 0;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                worker.RunWorkerAsync(new[] { file_source.Text, source_start.Text, source_end.Text });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        public static string GetThumbnail(string video, string thumbnail, string time)
        {
            //-deinterlace -an -ss 1 -t 00:00:01 -r 1 -y -vcodec mjpeg -f mjpeg $thumbnail 2>&1
            // ffmpeg -ss 01:23:45 -i input -vframes 1 -q:v 2 output.jpg
            // "ffmpeg -itsoffset -1 -t -ss " + time + " -i " + '"' + video + '"' + " -vcodec mjpeg -vframes 1 -an -f rawvideo " + '"' + thumbnail + '"'
            var cmd = "ffmpeg -ss " + time + " -i \"" + video + "\" -vf yadif -vframes 1 -q:v 2 "+ thumbnail;
            File.AppendAllText("log.txt", cmd + "\r\n");
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C " + cmd
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit(5000);
            }

            return new FileInfo(thumbnail).FullName;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                find_file.IsEnabled = false;
                gather.IsEnabled = false;
                custom.IsEnabled = false;

            });
            try
            {

                for (int i = 0; i < 9; i++)
                {
                    File.Delete("output_" + i + ".jpg");
                }

                TimeSpan start = TimeSpan.Parse(((object[])e.Argument)[1] as string ?? throw new InvalidOperationException());
                TimeSpan end = TimeSpan.Parse(((object[])e.Argument)[2] as string ?? throw new InvalidOperationException());

                TimeSpan difference = new TimeSpan(end.Ticks - start.Ticks);

                long interval = difference.Ticks / 9;

                for (int i = 0; i < 9; i++)
                {
                    long addition = interval * i;
                    long newtime = start.Ticks + addition;
                    TimeSpan timeSpan = new TimeSpan(newtime);

                    if (timeSpan.Ticks > end.Ticks)
                    {
                        timeSpan = end;
                    }

                    

                    
                    Dispatcher.Invoke(() => {
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri(GetThumbnail(((object[])e.Argument)[0] as string, "output_" + i + ".jpg", $@"{timeSpan.Hours.ToString("D2")}:{timeSpan.Minutes.ToString("D2")}:{timeSpan.Seconds.ToString("D2")}"));
                        bmi.CacheOption = BitmapCacheOption.OnLoad;
                        bmi.EndInit();
                        ((Image)FindName("thumbnail_" + i)).Source = ((Image)FindName("thumbnail_" + i)).Source = bmi.Clone();
                    });

                    (sender as BackgroundWorker)?.ReportProgress(Convert.ToInt32(((double)i / 9) * 100));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

                for (int i = 0; i < 9; i++)
                {
                    File.Delete("output_" + i + ".jpg");
                }

                //panel_start.Enabled = true;
                //panel_end.Enabled = true;
                //button_gather.Enabled = true;
                Dispatcher.Invoke(() =>
                {
                    find_file.IsEnabled = true;
                    gather.IsEnabled = true;
                    custom.IsEnabled = true;

                });
                e.Result = ((object[])e.Argument)[0];
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine("Progress on thumbnails (" + e.ProgressPercentage + "%)");

            tb_prg.Dispatcher.Invoke(DispatcherPriority.Send, new Action<MainWindow>
            (
                (s) =>
                {
                    s.tb_prg.Value = e.ProgressPercentage;
                }
            ),this);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Effect = null;
            tb_prg.Value = 100;
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "PNG Files|*.png;*.PNG|JPEG Files|*.jpeg;*.jpg;*.JPEG;*.JPG|BMP Files|*.bmp;*.BMP|*.* Any|*.*";
                if (ofd.ShowDialog() == true)
                {
                    BitmapImage bmi = new BitmapImage();
                    bmi.BeginInit();
                    bmi.UriSource = new Uri(ofd.FileName);
                    bmi.CacheOption = BitmapCacheOption.OnLoad;
                    bmi.EndInit();
                    Export export = new Export(bmi);
                    export.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Thumbnail_0_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Image)sender).Effect = new BlurEffect { Radius = 50 };
            
        }

        private void Thumbnail_0_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Effect = null;
        }

        private void Thumbnail_0_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (_default.IsEqual((BitmapSource)((Image)sender).Source)) { e.Handled = true; return; }
            Export export = new Export(((Image)sender).Source);
            export.Show();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 9; i++)
            {
                _default = Properties.Resources.base_thumbnail.Convert();
                var control = ((Image)FindName($@"thumbnail_{i}"));
                if (control != null) control.Source = _default;
            }

            StyleMode.SelectedIndex = Properties.Settings.Default.Style;

            if (File.Exists("frames.xml"))
            {
                _frames =(Dictionary<string,BoxArtFrame>) _frames.GetType().GetXmlDictionary(DictionaryExtension.ReadXmlToString("frames.xml"));
            }
            else
            {
                _frames.Add("16x9", new BoxArtFrame(1422,800,"16x9"));
                _frames.Add("4x3", new BoxArtFrame(1066, 800, "4x3"));
                _frames.Add("3x4", new BoxArtFrame(600, 800, "3x4"));
                _frames.Add("2x3", new BoxArtFrame(533, 800, "2x3"));
                _frames.GetXmlString().SaveXmlToFile("frames.xml");
            }

            foreach (KeyValuePair<string, BoxArtFrame> frame in _frames)
            {
                frame_list.Items.Add(new ListViewItem() { Tag = frame.Key, Content = frame.Value.ToString() });
            }

        }

        private void StyleMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Style = StyleMode.SelectedIndex;
            Properties.Settings.Default.Save();
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("Skin_" + WPF_Thumbnails.Properties.Settings.Default.Style + ".xaml",
                UriKind.Relative);
            Application.Current.Resources["PrimaryColour"] = dict["PrimaryColour"];
            Application.Current.Resources["SecondaryColour"] = dict["SecondaryColour"];
            Application.Current.Resources["DrkPrimaryColour"] = dict["DrkPrimaryColour"];
            Application.Current.Resources["DrkSecondaryColour"] = dict["DrkSecondaryColour"];
        }

        private void Slider_start_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan ts = new TimeSpan((long)slider_start.Value);
            source_start.Text = $@"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D2}";
        }

        private void Slider_end_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            TimeSpan ts = new TimeSpan((long)slider_end.Value);
            source_end.Text = $@"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D2}";
        }

        private void Add_frame_Click(object sender, RoutedEventArgs e)
        {
            if (frame_height.Value == null || frame_width.Value == null || frame_name.Text == "") return;
            _frames[frame_name.Text] =
                new BoxArtFrame((int) frame_width.Value, (int) frame_height.Value, frame_name.Text);
            frame_list.Items.Add(new ListViewItem(){Tag = frame_name.Text, Content = _frames[frame_name.Text].ToString() });
            _frames.GetXmlString().SaveXmlToFile("frames.xml");
        }

        private void Edit_frame_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem key = ((ListViewItem) frame_list.SelectedItem);
            frame_name.Text = key.Tag+"";
            frame_height.Value = _frames[frame_name.Text].Height;
            frame_width.Value = _frames[frame_name.Text].Width;
            _frames.Remove(frame_name.Text);
            frame_list.Items.Remove(key);
            frame_list.UnselectAll();
            _frames.GetXmlString().SaveXmlToFile("frames.xml");
        }

        private void Remove_frame_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem key = ((ListViewItem)frame_list.SelectedItem);
            _frames.Remove(key.Tag+"");
            frame_list.Items.Remove(key);
            frame_list.UnselectAll();
            _frames.GetXmlString().SaveXmlToFile("frames.xml");
        }
    }
}
