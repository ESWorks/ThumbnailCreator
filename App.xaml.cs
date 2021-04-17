using System;
using System.Windows;

namespace WPF_Thumbnails
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //using System.Windows
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("Skin_"+WPF_Thumbnails.Properties.Settings.Default.Style + ".xaml", UriKind.Relative);
            Current.Resources["PrimaryColour"] = dict["PrimaryColour"];
            Current.Resources["SecondaryColour"] = dict["SecondaryColour"];
            Current.Resources["DrkPrimaryColour"] = dict["DrkPrimaryColour"];
            Current.Resources["DrkSecondaryColour"] = dict["DrkSecondaryColour"];
        }
    }
}
