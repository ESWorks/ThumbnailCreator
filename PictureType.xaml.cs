using System.Windows;

namespace WPF_Thumbnails
{
    /// <summary>
    /// Interaction logic for PictureType.xaml
    /// </summary>
    public partial class PictureType : Window
    {
        private PictureType()
        {
            InitializeComponent();
        }

        private string[] _retVal;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _retVal = new[] { prefix.Text, ".png" };
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            _retVal = new[] { prefix.Text, ".jpeg" };
            DialogResult = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            _retVal = new[] { prefix.Text, ".bmp" };
            DialogResult = true;
        }

        public new static string[] Show()
        {
            var pt = new PictureType();
            return pt.ShowDialog() == true ? pt._retVal : new string[0];
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {


            _retVal = new[] { prefix.Text, ".jpg" };
            DialogResult = true;
        }
    }
}
