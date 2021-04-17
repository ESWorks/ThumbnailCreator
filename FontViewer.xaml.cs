using System.Windows;
using System.Windows.Media;

namespace WPF_Thumbnails
{
    /// <summary>
    /// Interaction logic for FontViewer.xaml
    /// </summary>
    public partial class FontViewer : Window
    {
        private FontViewer()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static Typeface GetTypeface()
        {
            FontViewer fv = new FontViewer();
            if(fv.ShowDialog() == true)
            {
                return new Typeface(fv.preview.FontFamily, fv.preview.FontStyle, fv.preview.FontWeight, fv.preview.FontStretch);
            }
            return default;
        }
    }
}
