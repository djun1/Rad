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
    public sealed partial class OptionsPage : SettingsFlyout
    {
        public event EventHandler SettingsChanged;
        private static String ChosenCityName;
        private static String ChosenCityURL;
        private static String ChosenRadarType;
        private static String ChosenPrecipitationType;

        public OptionsPage()
        {
            this.InitializeComponent();            
        }

        private void ProvinceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProvinceComboBox != null)
            {
                PopulateStationBox(ProvinceComboBox.SelectedIndex);
                //switch (ProvinceComboBox.SelectedIndex)
                //{
                //    case 0://AB
                //        //ChosenCityURL = "http://www.ssd.noaa.gov/goes/west/wfo/byz/img/";
                //        //ChosenCityName = "Billings";
                //        //StationComboBox.Items.Clear();
                //        PopulateStationBox("AB");        
                //        break;
                //    case 1://BC
                //        //StationComboBox.Items.Clear();
                //        PopulateStationBox("BC");
                //        break;
                //    case 2://MB
                //        //StationComboBox.Items.Clear();
                //        PopulateStationBox("MB");
                //        break;
                //    case 3://NB
                //        //StationComboBox.Items.Clear();
                //        PopulateStationBox("NB");
                //        break;
                //    case 4://NL
                //        PopulateStationBox("NL");
                //        break;
                //    case 5://ON
                //        PopulateStationBox("ON");
                //        break;
                //    case 6://QC
                //        PopulateStationBox("QC");
                //        break;
                //    case 7://SK
                //        PopulateStationBox("SK");
                //        break;
                //}
                //StationComboBox.SelectedIndex = 0;
            }
        }

        private void OptionsPage_BackClick(object sender, BackClickEventArgs e)
        {
            if(ChosenCityName != null && ChosenCityName.Equals(GenericCodeClass.HomeStationName) == false)
            {
                GenericCodeClass.HomeStationName = ChosenCityName;
                GenericCodeClass.HomeStation = ChosenCityURL;   //check for null?
                GenericCodeClass.HomeStationChanged = true;
            }

            //Better to check for existing download intervals before setting new times?
            if (DurationRadioButton1.IsChecked == true)
                GenericCodeClass.FileDownloadPeriod = 3;
            else if(DurationRadioButton2.IsChecked == true)
                GenericCodeClass.FileDownloadPeriod = 6;
            

            if (LoopTimerRadioButton1.IsChecked == true)
                GenericCodeClass.LoopInterval = new TimeSpan(0, 0, 0, 0, 100);
            else if (LoopTimerRadioButton2.IsChecked == true)
                GenericCodeClass.LoopInterval = new TimeSpan(0, 0, 0, 0, 500);
            else
                GenericCodeClass.LoopInterval = new TimeSpan(0, 0, 0, 1, 0);
            

            if (SettingsChanged != null)
                SettingsChanged(this, EventArgs.Empty);
        }

        private void OptionsPage_Unloaded(object sender, RoutedEventArgs e)
        {
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
                    switch (StationComboBox.Items[StationComboBox.SelectedIndex].ToString())
                    {
                        
                        case "Edmonton":
                            GenericCodeClass.HomeStationCodeString = "WHK"; //Change this to ChosenCityCode?
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WHK/"; //Change this to ChosenCityCode?
                            break;
                        case "Cold Lake":
                            GenericCodeClass.HomeStationCodeString = "WHN";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WHN/";
                            //GenericCodeClass.HomeStation = "WHN";
                            break;
                        case "Calgary":
                            GenericCodeClass.HomeStationCodeString = "XSM";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XSM/";
                            //GenericCodeClass.HomeStationCodeString = "XSM";
                            break;
                        case "Grande Prairie":
                            GenericCodeClass.HomeStationCodeString = "WWW";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WWW/";
                            //GenericCodeClass.HomeStationCodeString = "WWW";
                            break;
                        case "Medicine Hat":
                            GenericCodeClass.HomeStationCodeString = "XBU";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XBU/";
                            //GenericCodeClass.HomeStationCodeString = "XBU";
                            break;
                        
                        case "Vancouver":
                            GenericCodeClass.HomeStationCodeString = "WUJ";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WUJ/";
                            //GenericCodeClass.HomeStationCodeString = "WUJ";
                            break;
                        case "Victoria":
                            GenericCodeClass.HomeStationCodeString = "XSI";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XSI/";
                            //GenericCodeClass.HomeStationCodeString = "XSI";
                            break;
                        case "Kelowna":
                            GenericCodeClass.HomeStationCodeString = "XSS";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XSS/";
                            //GenericCodeClass.HomeStationCodeString = "XSS";
                            break;
                        case "Prince George":
                            GenericCodeClass.HomeStationCodeString = "XPG";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XPG/";
                            //GenericCodeClass.HomeStationCodeString = "XPG";
                            break;
                        
                        case "Sudbury":
                            GenericCodeClass.HomeStationCodeString = "WHK";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WHK/";
                            //GenericCodeClass.HomeStationCodeString = "WHK";
                            break;
                        case "Sault Ste. Marie":
                            GenericCodeClass.HomeStationCodeString = "WBI";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WBI/";
                            //GenericCodeClass.HomeStationCodeString = "WBI";
                            break;
                        case "Toronto":
                            GenericCodeClass.HomeStationCodeString = "WKR";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WKR/";
                            //GenericCodeClass.HomeStationCodeString = "WKR";
                            break;
                        case "London":
                            GenericCodeClass.HomeStationCodeString = "WSO";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WSO/";
                            //GenericCodeClass.HomeStationCodeString = "WSO";
                            break;
                        case "Dryden":
                            GenericCodeClass.HomeStationCodeString = "XDR";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XDR/";
                            //GenericCodeClass.HomeStationCodeString = "XDR";
                            break;
                        case "Ottawa":
                            GenericCodeClass.HomeStationCodeString = "XFT";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XFT/";
                            //GenericCodeClass.HomeStationCodeString = "XFT";
                            break;
                        case "Timmins":
                            GenericCodeClass.HomeStationCodeString = "XTI";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XTI/";
                            //GenericCodeClass.HomeStationCodeString = "XTI";
                            break;
                        case "Thunder Bay":
                            GenericCodeClass.HomeStationCodeString = "XNI";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XNI/";
                            //GenericCodeClass.HomeStationCodeString = "XNI";
                            break;

                        case "Chicoutimi":
                            GenericCodeClass.HomeStationCodeString = "WMB";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WMB/";
                            //GenericCodeClass.HomeStationCodeString = "WMB";
                            break;
                        case "Montreal":
                            GenericCodeClass.HomeStationCodeString = "WMN";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WMN/";
                            //GenericCodeClass.HomeStationCodeString = "WMN";
                            break;
                        case "Quebec City":
                            GenericCodeClass.HomeStationCodeString = "WVY";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WVY/";
                            //GenericCodeClass.HomeStationCodeString = "WVY";
                            break;

                        case "Rimouski":
                            GenericCodeClass.HomeStationCodeString = "XAM";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XAM/";
                            //GenericCodeClass.HomeStationCodeString = "XAM";
                            break;
                        case "Val d’Or":
                            GenericCodeClass.HomeStationCodeString = "XLA";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XLA/";
                            //GenericCodeClass.HomeStationCodeString = "XLA";
                            break;

                        case "St. John’s":
                            GenericCodeClass.HomeStationCodeString = "WTP";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "WTP/";
                            //GenericCodeClass.HomeStationCodeString = "WTP";
                            break;
                        case "Corner Brook":
                            GenericCodeClass.HomeStationCodeString = "XME";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XME/";
                            //GenericCodeClass.HomeStationCodeString = "XME";
                            break;

                        case "Regina":
                            GenericCodeClass.HomeStationCodeString = "XBE";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XBE/";
                            //GenericCodeClass.HomeStationCodeString = "XBE";
                            break;
                        case "Saskatoon":
                            GenericCodeClass.HomeStationCodeString = "XRA";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XRA/";
                            //GenericCodeClass.HomeStationCodeString = "XRA";
                            break;

                        case "Winnipeg":
                            GenericCodeClass.HomeStationCodeString = "XWL";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XWL/";
                            //GenericCodeClass.HomeStationCodeString = "XWL";
                            break;
                        case "Brandon":
                            GenericCodeClass.HomeStationCodeString = "XWL";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XFW/";
                            //GenericCodeClass.HomeStationCodeString = "XFW";
                            break;

                        case "Halifax":
                            GenericCodeClass.HomeStationCodeString = "XGO";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XGO/";
                            //GenericCodeClass.HomeStationCodeString = "XGO";
                            break;
                        case "Sydney":
                            GenericCodeClass.HomeStationCodeString = "XMB";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XMB/";
                            //GenericCodeClass.HomeStationCodeString = "XMB";
                            break;

                        case "Moncton":
                            GenericCodeClass.HomeStationCodeString = "XNC";
                            GenericCodeClass.HomeStation = "http://dd.weatheroffice.gc.ca/radar/" + ChosenRadarType + "/GIF/" + "XNC/";
                            //GenericCodeClass.HomeStationCodeString = "XNC";
                            break;
                    }
                }

                GenericCodeClass.HomeStationName = StationComboBox.Items[StationComboBox.SelectedIndex].ToString();
                GenericCodeClass.HomeProvinceName = ProvinceComboBox.Items[ProvinceComboBox.SelectedIndex].ToString();
                GenericCodeClass.RadarTypeString = ChosenRadarType;
                GenericCodeClass.PrecipitationTypeString = ChosenPrecipitationType;
                //GenericCodeClass.HomeStation = ChosenCityURL;   //check for null?
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
                        
            if (SettingsChanged != null)
                SettingsChanged(this, EventArgs.Empty);
        }

        private void OptionsPage_Loaded(object sender, RoutedEventArgs e)
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

            switch(GenericCodeClass.RadarTypeString)
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

            ProvinceComboBox.SelectedItem = GenericCodeClass.HomeProvinceName;
            PopulateStationBox(ProvinceComboBox.SelectedIndex);
            StationComboBox.SelectedItem = GenericCodeClass.HomeStationName;
        }

        private void PopulateStationBox(int ProvinceBoxIndex)
        {
            if(StationComboBox != null)
            {
                StationComboBox.Items.Clear();
                switch (ProvinceBoxIndex)
                {
                    case 0: //AB
                        StationComboBox.Items.Add("Edmonton");
                        StationComboBox.Items.Add("Cold Lake");
                        StationComboBox.Items.Add("Grande Prairie");
                        StationComboBox.Items.Add("Medicine Hat");
                        StationComboBox.Items.Add("Calgary");
                        break;
                    case 1: //BC
                        StationComboBox.Items.Add("Vancouver");
                        StationComboBox.Items.Add("Prince George");
                        StationComboBox.Items.Add("Kelowna");
                        break;
                    case 2://MB
                        StationComboBox.Items.Add("Brandon");
                        StationComboBox.Items.Add("Winnipeg");
                        break;
                    case 3://NB
                        StationComboBox.Items.Add("Moncton");
                        break;
                    case 4://NL
                        StationComboBox.Items.Add("St. John’s");
                        StationComboBox.Items.Add("Corner Brook");
                        break;
                    case 5://ON
                        StationComboBox.Items.Add("Sudbury");
                        StationComboBox.Items.Add("Sault Ste. Marie");
                        StationComboBox.Items.Add("Toronto");
                        StationComboBox.Items.Add("London");
                        StationComboBox.Items.Add("Dryden");
                        StationComboBox.Items.Add("Ottawa");
                        StationComboBox.Items.Add("Thunder Bay");
                        StationComboBox.Items.Add("Timmins");
                        break;
                    case 6://QC
                        StationComboBox.Items.Add("Chicoutimi");
                        StationComboBox.Items.Add("Montreal");
                        StationComboBox.Items.Add("Quebec City");
                        StationComboBox.Items.Add("Rimouski");
                        StationComboBox.Items.Add("Val d’Or");
                        break;
                    case 7://SK
                        StationComboBox.Items.Add("Regina");
                        StationComboBox.Items.Add("Saskatoon");
                        break;
                }
                StationComboBox.SelectedIndex = 0;
            }
            
        }

        //private void StationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if(StationComboBox != null)
        //    {
        //        switch (StationComboBox.Items[StationComboBox.SelectedIndex].ToString())
        //        {
        //            //AB
        //            case "Edmonton":
        //                GenericCodeClass.HomeStationCodeString = "WHK";
        //                break;
        //            case "Cold Lake":
        //                GenericCodeClass.HomeStationCodeString = "WHN";
        //                break;
        //            case "Calgary":
        //                GenericCodeClass.HomeStationCodeString = "XSM";
        //                break;
        //            case "Grande Prairie":
        //                GenericCodeClass.HomeStationCodeString = "WWW";
        //                break;
        //            case "Medicine Hat":
        //                GenericCodeClass.HomeStationCodeString = "XBU";
        //                break;

        //            case "Vancouver":
        //                GenericCodeClass.HomeStationCodeString = "WUJ";
        //                break;
        //            case "Victoria":
        //                GenericCodeClass.HomeStationCodeString = "XSI";
        //                break;
        //            case "Kelowna":
        //                GenericCodeClass.HomeStationCodeString = "XSS";
        //                break;
        //            case "Prince George":
        //                GenericCodeClass.HomeStationCodeString = "XPG";
        //                break;

        //            case "Sudbury":
        //                GenericCodeClass.HomeStationCodeString = "WHK";
        //                break;
        //            case "Sault Ste. Marie":
        //                GenericCodeClass.HomeStationCodeString = "WBI";
        //                break;
        //            case "Toronto":
        //                GenericCodeClass.HomeStationCodeString = "WKR";
        //                break;
        //            case "London":
        //                GenericCodeClass.HomeStationCodeString = "WSO";
        //                break;
        //            case "Dryden":
        //                GenericCodeClass.HomeStationCodeString = "XDR";
        //                break;
        //            case "Ottawa":
        //                GenericCodeClass.HomeStationCodeString = "XFT";
        //                break;
        //            case "Timmins":
        //                GenericCodeClass.HomeStationCodeString = "XTI";
        //                break;
        //            case "Thunder Bay":
        //                GenericCodeClass.HomeStationCodeString = "XNI";
        //                break;

        //            case "Chicoutimi":
        //                GenericCodeClass.HomeStationCodeString = "WMB";
        //                break;
        //            case "Montreal":
        //                GenericCodeClass.HomeStationCodeString = "WMN";
        //                break;
        //            case "Quebec City":
        //                GenericCodeClass.HomeStationCodeString = "WVY";
        //                break;

        //            case "Rimouski":
        //                GenericCodeClass.HomeStationCodeString = "XAM";
        //                break;
        //            case "Val d’Or":
        //                GenericCodeClass.HomeStationCodeString = "XLA";
        //                break;

        //            case "St. John’s":
        //                GenericCodeClass.HomeStationCodeString = "WTP";
        //                break;
        //            case "Corner Brook":
        //                GenericCodeClass.HomeStationCodeString = "XME";
        //                break;

        //            case "Regina":
        //                GenericCodeClass.HomeStationCodeString = "XBE";
        //                break;
        //            case "Saskatoon":
        //                GenericCodeClass.HomeStationCodeString = "XRA";
        //                break;

        //            case "Winnipeg":
        //                GenericCodeClass.HomeStationCodeString = "XWL";
        //                break;
        //            case "Brandon":
        //                GenericCodeClass.HomeStationCodeString = "XFW";
        //                break;

        //            case "Halifax":
        //                GenericCodeClass.HomeStationCodeString = "XGO";
        //                break;
        //            case "Sydney":
        //                GenericCodeClass.HomeStationCodeString = "XMB";
        //                break;

        //            case "Moncton":
        //                GenericCodeClass.HomeStationCodeString = "XNC";
        //                break;
        //        }
        //        GenericCodeClass.HomeStationName = StationComboBox.Items[StationComboBox.SelectedIndex].ToString();
        //    }            
        //}
    }
}
