using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace Rad
{
    public sealed partial class HelpPage : SettingsFlyout
    {
        public HelpPage()
        {
            this.InitializeComponent();
        }

        private async void ECLink_onClick(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.ec.gc.ca/meteo-weather/default.asp?lang=En&n=2B931828-1"));
        }

        private async void NWSLink_onClick(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://weather.noaa.gov/radar/radinfo/radinfo.html"));
        }
    }
}
