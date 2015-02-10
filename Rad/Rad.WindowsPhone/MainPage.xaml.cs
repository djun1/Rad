using Rad.Common;
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
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Rad
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private static List<string> Files = new List<string>();
        private static List<string> LegendFiles = new List<string>();
        private static List<string> WarningsFiles = new List<string>();
        private static int CurrImgIndex = -1;
        private StorageFolder ImageFolder;
        private DispatcherTimer LoopTimer;
        private DispatcherTimer DownloadTimer;
        
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            LoopTimer = new DispatcherTimer();
            DownloadTimer = new DispatcherTimer();
            LoopTimer.Tick += Timer_Handler;
            DownloadTimer.Tick += Timer_Handler;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Task GetFileNamesTask, DownloadFilesTask; //DeleteFilesTask,
            //SetSystemImage("ms-appx:///Assets/Loading.png");
            
            GenericCodeClass.GetSavedAppData();
			GetFileNamesTask = GenericCodeClass.GetListOfLatestFiles(Files, LegendFiles, WarningsFiles);
            LoadOverlayImages();
            SetNavigationButtonState(GenericCodeClass.IsLoopPaused, false);
            StationBox.Text = GenericCodeClass.HomeStationName;

            if(GenericCodeClass.HomeStationChanged == true)
                await GenericCodeClass.DeleteFiles(ImageFolder,null,true);

            await GetFileNamesTask;

            DownloadFilesTask = DownloadFiles();

            LoopTimer.Interval = GenericCodeClass.LoopInterval; //Create a timer that trigger every 1 s
            DownloadTimer.Interval = GenericCodeClass.DownloadInterval; //Create a timer that triggers every 30 min

            try
            {
                await DownloadFilesTask; //maybe used the status field to check whether the task is worth waiting for
            }
            catch
            {

            }

            if (!DownloadFilesTask.IsFaulted)
            {
                if(!GenericCodeClass.IsLoopPaused)
                    LoopTimer.Start();
                
                if(GenericCodeClass.FileDownloadPeriod != 0)
                    SetNavigationButtonState(GenericCodeClass.IsLoopPaused, true);
                SetOverlayVisibilities(false);
                DownloadTimer.Start();
            }
            else
            {
                //Show Error Message
                //SetSystemImage("ms-appx:///Assets/Error.png");
                StatusBox.Text = "Error Downloading Images";
                ImgBox.MakeImagesInvisible();
            }

            GenericCodeClass.HomeStationChanged = false;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            LoopTimer.Stop();
            DownloadTimer.Stop();
            GenericCodeClass.SaveAppData(false);
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void PlayPauseButton_Click(object sender, TappedRoutedEventArgs e)
        {
            if (GenericCodeClass.IsLoopPaused == false)
                LoopTimer.Stop();
            else
                LoopTimer.Start();

            GenericCodeClass.IsLoopPaused = !GenericCodeClass.IsLoopPaused;
            SetNavigationButtonState(GenericCodeClass.IsLoopPaused,true);
        }

        private async void NextButton_Click(object sender, TappedRoutedEventArgs e)
        {
            if (Files.Count != 0)
                CurrImgIndex = ++CurrImgIndex % Files.Count;
            else
                CurrImgIndex = -1;

            await ChangeImage(CurrImgIndex);
        }

        private async void PrevButton_Click(object sender, TappedRoutedEventArgs e)
        {
            if (Files.Count != 0)
                CurrImgIndex = (CurrImgIndex + Files.Count - 1) % Files.Count;
            else
                CurrImgIndex = -1;

            await ChangeImage(CurrImgIndex);
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            await GenericCodeClass.GetListOfLatestFiles(Files, LegendFiles, WarningsFiles);
            await DownloadFiles();
            if(NextButton.IsEnabled == true)
                await ChangeImage(CurrImgIndex);
        }

        private async Task DownloadFiles()
        {
            int i;
            int RetCode, LegendRetcode, WarningsRetcode;
            int DownloadedFiles = 1;

            if (ImageFolder == null)
                ImageFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

            FileDownloadProgBar.IsIndeterminate = false;
            FileDownloadProgBar.Maximum = Files.Count;
            FileDownloadProgBar.Minimum = 0;
            FileDownloadProgBar.Value = 0;
            
            for (i = 0; i < Files.Count; i++)
            {
                if (GenericCodeClass.ExistingFiles.Contains(Files[i].ToString()) && GenericCodeClass.HomeStationChanged == false)
                {
                    GenericCodeClass.ExistingFiles.Remove(Files[i].ToString());
                    continue;
                }                    
                
                StatusBox.Text = "Downloading image " + DownloadedFiles.ToString() + "/" + Files.Count.ToString();
                FileDownloadProgBar.Visibility = Visibility.Visible;
                RetCode = await GenericCodeClass.GetFileUsingHttp(GenericCodeClass.HomeStation + Files[i], ImageFolder, Files[i]);
                if(!GenericCodeClass.CanadaSelected && !GenericCodeClass.HomeProvinceName.Equals("Regional Composites"))
                {
                    LegendRetcode = await GenericCodeClass.GetFileUsingHttp("http://radar.weather.gov/ridge/Legend/" + GenericCodeClass.RadarTypeString +
                        "/" + GenericCodeClass.HomeStationCodeString + "/" + LegendFiles[i], ImageFolder, LegendFiles[i]);
                    WarningsRetcode = await GenericCodeClass.GetFileUsingHttp("http://radar.weather.gov/ridge/Warnings/Short/" + GenericCodeClass.HomeStationCodeString
                        + "/" + WarningsFiles[i], ImageFolder, WarningsFiles[i]);
                }
            
                if (RetCode == -1)
                {
                    Files.Remove(Files[i].ToString());
                }
                else
                {
                    ImgBox.ImgSource = await GenericCodeClass.GetBitmapImage(ImageFolder, Files[i]);
                    if (!GenericCodeClass.CanadaSelected && !GenericCodeClass.HomeProvinceName.Equals("Regional Composites"))
                    {
                        ImgBox.LegendSource = await GenericCodeClass.GetBitmapImage(ImageFolder, LegendFiles[i]);
                        ImgBox.WarningsSource = await GenericCodeClass.GetBitmapImage(ImageFolder, WarningsFiles[i]);
                    }
                    SetOverlayVisibilities(false);
                    DownloadedFiles += 1;
                    FileDownloadProgBar.Value += 1;
                    StatusBox.Text += " Finished.";
                }
            }

            FileDownloadProgBar.Visibility = Visibility.Collapsed;

            if (Files.Count > 0)
            {
                CurrImgIndex = 0;
                await ChangeImage(CurrImgIndex);
            }
            else
                CurrImgIndex = -1;

            await GenericCodeClass.DeleteFiles(ImageFolder, GenericCodeClass.ExistingFiles, false);
        }

        private async Task ChangeImage(int ImageIndex)
        {
            string StatBoxString;

            if (GenericCodeClass.IsLoopPaused == false)
                LoopTimer.Stop();   //Stop the loop timer to allow enough time to change image

            if (ImageFolder == null)
                ImageFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

            if (Files.Count != 0 && ImageIndex >= 0 && ImageIndex <= Files.Count)
            {
                DateTime LocalTime = GenericCodeClass.GetDateTimeFromFile(Files[ImageIndex]);
                StatBoxString = LocalTime.ToString("MMM dd HH:mm") + "   " + (ImageIndex + 1).ToString() + "/" + Files.Count.ToString() + " ";
                ImgBox.ImgSource = await GenericCodeClass.GetBitmapImage(ImageFolder, Files[ImageIndex]);
                if (!GenericCodeClass.CanadaSelected)
                {
                    if(!GenericCodeClass.HomeProvinceName.Equals("Regional Composites"))
                    {
                        ImgBox.LegendSource = await GenericCodeClass.GetBitmapImage(ImageFolder, LegendFiles[ImageIndex]);
                        ImgBox.WarningsSource = await GenericCodeClass.GetBitmapImage(ImageFolder, WarningsFiles[ImageIndex]);
                    }

                    if(GenericCodeClass.RadarTypeString.Equals("N0R"))
                        StatBoxString += "Base";
                    else
                        StatBoxString += "Composite";

                }
                else
                {
                    StatBoxString += GenericCodeClass.RadarTypeString + "-" + GenericCodeClass.PrecipitationTypeString;
                }

                StatusBox.Text = StatBoxString;
            }
            else
            {
                //SetSystemImage("ms-appx:///Assets/Error.png");
                StatusBox.Text = "Error Downloading Images";
                ImgBox.MakeImagesInvisible();
            }

            if (GenericCodeClass.IsLoopPaused == false)
                LoopTimer.Start();
        }

        private async void Timer_Handler(object sender, object e)
        {
            DispatcherTimer tmpTimer = (DispatcherTimer)sender;

            LoopTimer.Stop();

            if (tmpTimer.Equals(LoopTimer))
            {
                if (Files.Count != 0)
                    CurrImgIndex = ++CurrImgIndex % Files.Count;
                else
                    CurrImgIndex = -1;
                await ChangeImage(CurrImgIndex);
            }
            else if (tmpTimer.Equals(DownloadTimer))
            {
                await GenericCodeClass.GetListOfLatestFiles(Files, LegendFiles, WarningsFiles);
                await DownloadFiles();
                DownloadTimer.Start();
            }
            LoopTimer.Start();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OptionsPage));
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AboutPage));
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HelpPage));
        }

        //Set the state for the navigation buttons. 
        //EnableAll is used to enable/disable all the buttons. If EnableAll is true all buttons are enabled depending on the. 
        //state of the loop. Otherwise they are all disabled.
        private void SetNavigationButtonState(bool LoopPaused, bool EnableAll)
        {
            if (EnableAll)
            {
                if (LoopPaused)
                    PlayPauseButton.Icon = new SymbolIcon(Symbol.Play);
                else
                    PlayPauseButton.Icon = new SymbolIcon(Symbol.Pause);
            }
            
            PlayPauseButton.IsEnabled = EnableAll;
            NextButton.IsEnabled = EnableAll && LoopPaused;
            PrevButton.IsEnabled = EnableAll && LoopPaused;
            
        }

        private void SetOverlayVisibilities(bool CollapseAll)
        {
            if(!CollapseAll)
            {
                if(!GenericCodeClass.CanadaSelected)
                {
                    ImgBox.LegendOverlayVisibility = Visibility.Visible;
                    ImgBox.RadarCircleOverlayVisibility = Visibility.Visible;

                    if (GenericCodeClass.RoadNoOverlayFlag)
                        ImgBox.WarningsOverlayVisibility = Visibility.Visible;
                    else
                        ImgBox.WarningsOverlayVisibility = Visibility.Collapsed;

                    if (GenericCodeClass.RadarCircleOverlayFlag)
                        ImgBox.CountiesOverlayVisibility = Visibility.Visible;
                    else
                        ImgBox.CountiesOverlayVisibility = Visibility.Collapsed;

                }   
                else
                {
                    if (GenericCodeClass.RoadNoOverlayFlag)
                        ImgBox.RoadNosOverlayVisibility = Visibility.Visible;
                    else
                        ImgBox.RoadNosOverlayVisibility = Visibility.Collapsed;

                    if (GenericCodeClass.RadarCircleOverlayFlag)
                        ImgBox.RadarCircleOverlayVisibility = Visibility.Visible;
                    else
                        ImgBox.RadarCircleOverlayVisibility = Visibility.Collapsed;
                }

                if (GenericCodeClass.CityOverlayFlag)
                    ImgBox.CitiesOverlayVisibility = Visibility.Visible;
                else
                    ImgBox.CitiesOverlayVisibility = Visibility.Collapsed;

                if (GenericCodeClass.RoadOverlayFlag)
                    ImgBox.RoadsOverlayVisibility = Visibility.Visible;
                else
                    ImgBox.RoadsOverlayVisibility = Visibility.Collapsed;
            }
            else
            {
                ImgBox.CitiesOverlayVisibility = Visibility.Collapsed;
                ImgBox.RoadsOverlayVisibility = Visibility.Collapsed;
                ImgBox.RoadNosOverlayVisibility = Visibility.Collapsed;
                ImgBox.RadarCircleOverlayVisibility = Visibility.Collapsed;
                ImgBox.WarningsOverlayVisibility = Visibility.Collapsed;
                ImgBox.CountiesOverlayVisibility = Visibility.Collapsed;
            }            
        }

        private void LoadOverlayImages()
        {
            if (!GenericCodeClass.CanadaSelected)
            {
                ImgBox.TopoBackgroundSource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/USA/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString + "_Topo_Short.jpg"));
                ImgBox.CitiesOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/USA/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString + "_City_Short.gif")); ;
                ImgBox.RoadsOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/USA/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString + "_Highways_Short.gif"));
                ImgBox.CountiesSource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/USA/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString + "_County_Short.gif"));
                ImgBox.RadarCircleOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/USA/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString + "_RangeRing_Short.gif"));
            }
            else
            {
                ImgBox.TopoBackgroundSource = null;
                ImgBox.CountiesSource = null;
                ImgBox.LegendSource = null;
                ImgBox.WarningsSource = null;
                ImgBox.CitiesOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/Canada/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString.ToLower() + "_cities.gif")); ;
                ImgBox.RoadsOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/Canada/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString.ToLower() + "_roads.gif"));
                ImgBox.RoadNosOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/Canada/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString.ToLower() + "_numbers.gif"));
                ImgBox.RadarCircleOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/Canada/radar_circle.gif"));
            }
                
            
        }

        //do not delete
        //private void SetSystemImage(string URI)
        //{
        //    Uri ImageUri = new Uri(URI);
        //    BitmapImage bitmap = ImgBox.Source as BitmapImage;

        //    try
        //    {
        //        if (bitmap != null && !bitmap.UriSource.AbsoluteUri.Equals(URI))
        //            ImgBox.Source = new BitmapImage(ImageUri);
        //    }
        //    catch
        //    {
        //        ImgBox.Source = new BitmapImage(ImageUri);
        //    }
        //}
            
        //private void AdBox_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        //{
        //    AdBox.Visibility = Visibility.Collapsed;
        //}

        }
    }        
