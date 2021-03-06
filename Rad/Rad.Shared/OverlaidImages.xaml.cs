﻿using System;
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

        public BitmapImage LegendSource
        {
            set {LegendOverlay.Source = value; }
            get { return (BitmapImage)LegendOverlay.Source; }
        }

        public Visibility LegendOverlayVisibility
        {
            set {LegendOverlay.Visibility = value; }
            get { return LegendOverlay.Visibility; }
        }

        public BitmapImage WarningsSource
        {
            set {WarningsOverlay.Source = value; }
            get { return (BitmapImage)WarningsOverlay.Source; }
        }

        public Visibility WarningsOverlayVisibility
        {
            set {WarningsOverlay.Visibility = value; }
            get { return WarningsOverlay.Visibility; }
        }

        public BitmapImage CountiesSource
        {
            set {CountiesOverlay.Source = value; }
            get { return (BitmapImage)CountiesOverlay.Source; }
        }

        public BitmapImage TopoBackgroundSource
        {
            set { TopoBackground.Source = value; }
            get { return (BitmapImage)TopoBackground.Source; }
        }

        public BitmapImage ImgSource
        {
            set 
            {
                BaseImage.Source = value;
                ControlHeight = value.PixelHeight;
                if (TopoBackground != null)
                    TopoBackground.Visibility = Visibility.Visible;
                BaseImage.Visibility = Visibility.Visible;
            }
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

        public BitmapImage RoadNosOverlaySource
        {
            set { RoadNoOverlay.Source = value; }
            get { return (BitmapImage) RoadNoOverlay.Source; }
        }
		
		public double ControlHeight
        {
            set
            {
                Thickness margins = new Thickness(0,-value,0,0);

                TopoBackground.Height = value;
                BaseImage.Height = value;
                CountiesOverlay.Height = value;
                RoadsOverlay.Height = value;
                RoadNoOverlay.Height = value;
                RadarCircleOverlay.Height = value;
                CityOverlay.Height = value;
                WarningsOverlay.Height = value;
                LegendOverlay.Height = value;

                BaseImage.Margin = margins;
                CountiesOverlay.Margin = margins;
                RoadsOverlay.Margin = margins;
                RoadNoOverlay.Margin = margins;
                RadarCircleOverlay.Margin = margins;
                CityOverlay.Margin = margins;
                WarningsOverlay.Margin = margins;
                LegendOverlay.Margin = margins;
            }
            get { return BaseImage.Height; }
        }

        public BitmapImage RadarCircleOverlaySource
        {
            set { RadarCircleOverlay.Source = value; }
            get { return (BitmapImage)RadarCircleOverlay.Source; }
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

        public Visibility RoadNosOverlayVisibility
        {
            set { RoadNoOverlay.Visibility = value; }
            get { return RoadNoOverlay.Visibility; }
        }

        public Visibility RadarCircleOverlayVisibility
        {
            set { RadarCircleOverlay.Visibility = value; }
            get { return RadarCircleOverlay.Visibility; }
        }

        public Visibility CountiesOverlayVisibility
        {
            set {CountiesOverlay.Visibility = value; }
            get { return CountiesOverlay.Visibility; }
        }

        public void ClearAllImages()
        {
            TopoBackground.Source = null;
            BaseImage.Source = null;
            CountiesOverlay.Source = null;
            RoadsOverlay.Source = null;
            RoadNoOverlay.Source = null;
            RadarCircleOverlay.Source = null;
            CityOverlay.Source = null;
            WarningsOverlay.Source = null;
            LegendOverlay.Source = null;

        }

        public void MakeImagesInvisible()
        {
            TopoBackground.Visibility = Visibility.Collapsed;
            BaseImage.Visibility = Visibility.Collapsed;
            CountiesOverlay.Visibility = Visibility.Collapsed;
            RoadsOverlay.Visibility = Visibility.Collapsed;
            RoadNoOverlay.Visibility = Visibility.Collapsed;
            RadarCircleOverlay.Visibility = Visibility.Collapsed;
            CityOverlay.Visibility = Visibility.Collapsed;
            WarningsOverlay.Visibility = Visibility.Collapsed;
            LegendOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
