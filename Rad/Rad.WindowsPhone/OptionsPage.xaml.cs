using Rad.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Rad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OptionsPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private static string ChosenCityName;
        private static string ChosenRadarType;
        private static string ChosenPrecipitationType;
        private static XMLParserClass ProvincialCityXML = new XMLParserClass("ProvinceCities.xml");
        private static XMLParserClass CityCodeXML = new XMLParserClass("CityCodes.xml");
        
        public OptionsPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            switch (GenericCodeClass.FileDownloadPeriod)
            {
                case 1:
                    DurationRadioButton1.IsChecked = true;
                    break;
                case 3:
                    DurationRadioButton2.IsChecked = true;
                    break;
            }

            switch (GenericCodeClass.LoopInterval.Milliseconds)
            {
                case 0:
                    if (GenericCodeClass.LoopInterval.Seconds == 1)
                        LoopTimerRadioButton3.IsChecked = true;
                    break;
                case 500:
                    LoopTimerRadioButton2.IsChecked = true;
                    break;
                case 100:
                    LoopTimerRadioButton1.IsChecked = true;
                    break;
                case 1000:
                    LoopTimerRadioButton3.IsChecked = true;
                    break;
            }

            switch (GenericCodeClass.RadarTypeString)
            {
                case "PRECIPET":
                    ProductRadioButton1.IsChecked = true;
                    break;
                case "CAPPI":
                    ProductRadioButton2.IsChecked = true;
                    break;
            }

            switch (GenericCodeClass.PrecipitationTypeString)
            {
                case "RAIN":
                    PrecipitationRadioButton1.IsChecked = true;
                    break;
                case "SNOW":
                    PrecipitationRadioButton2.IsChecked = true;
                    break;
            }

            CountryRadioButton1.IsChecked = GenericCodeClass.CanadaSelected;
            CountryRadioButton2.IsChecked = !GenericCodeClass.CanadaSelected;

            if (GenericCodeClass.CanadaSelected)
            {
                ProvincialCityXML.SetSourceFile("ProvinceCities.xml");
                CityCodeXML.SetSourceFile("CityCodes.xml");
            }
            else
            {
                ProvincialCityXML.SetSourceFile("USStateCities.xml");
                CityCodeXML.SetSourceFile("USCityCodes.xml");
            }

            PopulateProvinceBox(true);
            //ProvinceComboBox.SelectedItem = GenericCodeClass.HomeProvinceName;
            PopulateStationBox(ProvinceComboBox.SelectedIndex, ProvinceComboBox.Items[ProvinceComboBox.SelectedIndex].ToString(), true);
            SetOptions();
            //StationComboBox.SelectedItem = GenericCodeClass.HomeStationName;

            CityCheckBox.IsChecked = GenericCodeClass.CityOverlayFlag;
            RoadNoCheckBox.IsChecked = GenericCodeClass.RoadNoOverlayFlag;
            RoadCheckBox.IsChecked = GenericCodeClass.RoadOverlayFlag;
            RadarCircleCheckBox.IsChecked = GenericCodeClass.RadarCircleOverlayFlag;

            CountryRadioButton1.Checked += CountryRadioButton_CheckedHandler;
            CountryRadioButton2.Checked += CountryRadioButton_CheckedHandler;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + GenericCodeClass.RadarTypeString + "/GIF/" + GenericCodeClass.HomeStationCodeString + "/";
            ChosenCityName = StationComboBox.Items[StationComboBox.SelectedIndex].ToString();

            if (ProductRadioButton1.IsChecked == true)
                ChosenRadarType = "PRECIPET";
            else if (ProductRadioButton2.IsChecked == true)
                ChosenRadarType = "CAPPI";

            if (PrecipitationRadioButton1.IsChecked == true)
                ChosenPrecipitationType = "RAIN";
            else if (PrecipitationRadioButton2.IsChecked == true)
                ChosenPrecipitationType = "SNOW";

            GenericCodeClass.HomeStationChanged = !ChosenCityName.Equals(GenericCodeClass.HomeStationName)
                                                    || !ChosenRadarType.Equals(GenericCodeClass.RadarTypeString)
                                                    || !ChosenPrecipitationType.Equals(GenericCodeClass.PrecipitationTypeString);

            if (GenericCodeClass.HomeStationChanged)
            {
                if (StationComboBox != null)
                {
                    GenericCodeClass.HomeStationCodeString = CityCodeXML.GetCityCode(StationComboBox.Items[StationComboBox.SelectedIndex].ToString()); //Change this to ChosenCityCode?
                    GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + GenericCodeClass.HomeStationCodeString + "/"; //Change this to ChosenCityCode?                    
                }

                GenericCodeClass.HomeStationName = StationComboBox.Items[StationComboBox.SelectedIndex].ToString();
                GenericCodeClass.HomeProvinceName = ProvinceComboBox.Items[ProvinceComboBox.SelectedIndex].ToString();
                GenericCodeClass.RadarTypeString = ChosenRadarType;
                GenericCodeClass.PrecipitationTypeString = ChosenPrecipitationType;                
            }

            //Better to check for existing download intervals before setting new times?
            if (DurationRadioButton1.IsChecked == true)
                GenericCodeClass.FileDownloadPeriod = 1;
            else if (DurationRadioButton2.IsChecked == true)
                GenericCodeClass.FileDownloadPeriod = 3;

            if (LoopTimerRadioButton1.IsChecked == true)
                GenericCodeClass.LoopInterval = new TimeSpan(0, 0, 0, 0, 100);
            else if (LoopTimerRadioButton2.IsChecked == true)
                GenericCodeClass.LoopInterval = new TimeSpan(0, 0, 0, 0, 500);
            else
                GenericCodeClass.LoopInterval = new TimeSpan(0, 0, 0, 0, 1000);

            GenericCodeClass.CityOverlayFlag = (bool)CityCheckBox.IsChecked;
//            GenericCodeClass.TownOverlayFlag = (bool)TownCheckBox.IsChecked;
            GenericCodeClass.RoadOverlayFlag = (bool)RoadCheckBox.IsChecked;
            GenericCodeClass.RoadNoOverlayFlag = (bool)RoadNoCheckBox.IsChecked;
            GenericCodeClass.RadarCircleOverlayFlag = (bool)RadarCircleCheckBox.IsChecked;
            
            GenericCodeClass.SaveAppData(true);
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        
        private void ProvinceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProvinceComboBox != null)
            {
                PopulateStationBox(ProvinceComboBox.SelectedIndex, ProvinceComboBox.Items[ProvinceComboBox.SelectedIndex].ToString(),false);
                GenericCodeClass.HomeProvinceName = ProvinceComboBox.Items[ProvinceComboBox.SelectedIndex].ToString();
                SetOptions();
            }
        }

        private void SetOptions()
        {
            if (ProvinceComboBox != null)
            {
                bool isCompositeSelected = ProvinceComboBox.SelectedItem.Equals("Regional Composites");

                ProductRadioButton1.IsEnabled = true;
                ProductRadioButton2.IsEnabled = !isCompositeSelected;
                ProductRadioButton2.IsChecked = !isCompositeSelected && GenericCodeClass.RadarTypeString.Equals("CAPPI");
                ProductRadioButton1.IsChecked = isCompositeSelected || GenericCodeClass.RadarTypeString.Equals("PRECIPET");

                PrecipitationRadioButton1.IsEnabled = GenericCodeClass.CanadaSelected;
                PrecipitationRadioButton2.IsEnabled = GenericCodeClass.CanadaSelected;

                CityCheckBox.IsEnabled = !isCompositeSelected || (isCompositeSelected && GenericCodeClass.CanadaSelected);
                RadarCircleCheckBox.IsEnabled = !isCompositeSelected;
                RoadCheckBox.IsEnabled = !isCompositeSelected;
                RoadNoCheckBox.IsEnabled = !isCompositeSelected;

                RadarCircleCheckBox.IsChecked = !isCompositeSelected && GenericCodeClass.RadarCircleOverlayFlag;
                RoadCheckBox.IsChecked = !isCompositeSelected && GenericCodeClass.RoadOverlayFlag;
                RoadNoCheckBox.IsChecked = !isCompositeSelected && GenericCodeClass.RoadNoOverlayFlag;

                if (GenericCodeClass.CanadaSelected)
                {
                    ProductRadioButton1.Content = "PRECIPET";
                    ProductRadioButton2.Content = "CAPPI";
                    RoadNoCheckBox.Content = "Road Numbers";
                    RadarCircleCheckBox.Content = "Radar Circles";

                }
                else
                {
                    ProductRadioButton1.Content = "COMPOSITE";
                    ProductRadioButton2.Content = "BASE";
                    RoadNoCheckBox.Content = "Counties";
                    RadarCircleCheckBox.Content = "Warnings";
                }
            }
        }

        private void PopulateStationBox(int ProvinceBoxIndex, string ProvinceName, bool UseHomeStationValue)
        {

            if (StationComboBox != null)
            {
                List<string> CityNames = new List<string>();

                if (ProvinceName.Contains('&'))
                    ProvinceName = ProvinceName.Substring(0, 12);

                ProvincialCityXML.ReadCitiesInProvince(ProvinceName, CityNames);
                if (StationComboBox != null)
                {
                    StationComboBox.Items.Clear();
                    foreach (string City in CityNames)
                    {
                        StationComboBox.Items.Add(City);
                    }
                }

                if (UseHomeStationValue)
                    StationComboBox.SelectedItem = GenericCodeClass.HomeStationName;
                else
                    StationComboBox.SelectedIndex = 0;
            }
        }

        private void CountryRadioButton_CheckedHandler(object sender, RoutedEventArgs e)
        {
            if (sender == CountryRadioButton1)
            {
                if (GenericCodeClass.CanadaSelected)
                    return;
                ProvincialCityXML.SetSourceFile("ProvinceCities.xml");
                CityCodeXML.SetSourceFile("CityCodes.xml");
                GenericCodeClass.CanadaSelected = true;
            }
            else if (sender == CountryRadioButton2)
            {
                if (!GenericCodeClass.CanadaSelected)
                    return;
                ProvincialCityXML.SetSourceFile("USStateCities.xml");
                CityCodeXML.SetSourceFile("USCityCodes.xml");
                GenericCodeClass.CanadaSelected = false;
            }


            PopulateProvinceBox(false);

        }

        private void PopulateProvinceBox(bool UseHomeStationVlaue)
        {
            List<string> ProvinceList = ProvincialCityXML.ReadProvinceList();

            if (ProvinceComboBox != null)
            {
                ProvinceComboBox.SelectionChanged -= ProvinceComboBox_SelectionChanged;
                ProvinceComboBox.Items.Clear();

                foreach (string str in ProvinceList)
                    ProvinceComboBox.Items.Add(str);

                ProvinceComboBox.SelectionChanged += ProvinceComboBox_SelectionChanged;
                if (UseHomeStationVlaue)
                    ProvinceComboBox.SelectedItem = GenericCodeClass.HomeProvinceName;
                else
                    ProvinceComboBox.SelectedIndex = 0;

                //PopulateStationBox(ProvinceComboBox.SelectedIndex, ProvinceComboBox.Items[ProvinceComboBox.SelectedIndex].ToString());
            }

        }
    }
}
