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
using System.Text;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Rad
{
    public sealed partial class OverlaidImages : UserControl
    {
        public OverlaidImages()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Scrollview_changing(object sender, ScrollViewerViewChangingEventArgs e)
        {
            
        }

        public BitmapImage ImgSource
        {
            set { BaseImage.Source = value;}
            get { return (BitmapImage) BaseImage.Source; }
        }

        public BitmapImage CitiesOverlaySource
        {
            set { CityOverlay.Source = value; }
            get { return (BitmapImage)CityOverlay.Source; }
        }

        public BitmapImage RoadsOverlaySource
        {
            set { RoadsOverlay.Source = value; }
            get { return (BitmapImage)RoadsOverlay.Source; }
        }

        public BitmapImage TownsOverlaySource
        {
            set { TownsOverlay.Source = value; }
            get { return (BitmapImage) TownsOverlay.Source; }
        }

        public BitmapImage RoadNosOverlaySource
        {
            set { RoadNoOverlay.Source = value; }
            get { return (BitmapImage) RoadNoOverlay.Source; }
        }

        public Visibility CitiesOverlayVisibility
        {
            set { CityOverlay.Visibility = value; }
            get { return CityOverlay.Visibility; }
        }

        public Visibility RoadsOverlayVisibility
        {
            set { RoadsOverlay.Visibility = value; }
            get { return RoadsOverlay.Visibility; }
        }

        public Visibility TownsOverlayVisibility
        {
            set { TownsOverlay.Visibility = value; }
            get { return TownsOverlay.Visibility; }
        }

        public Visibility RoadNosOverlayVisibility
        {
            set { RoadNoOverlay.Visibility = value; }
            get { return RoadNoOverlay.Visibility; }
        }
    }
}
