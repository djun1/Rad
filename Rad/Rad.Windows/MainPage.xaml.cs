﻿using Rad.Common;
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
using Windows.UI.ApplicationSettings; //Settings flyout

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
        private static int CurrImgIndex = -1;
        private StorageFolder ImageFolder;
        private DispatcherTimer LoopTimer;
        private DispatcherTimer DownloadTimer;
        private OptionsPage OptionsPageFlyout = null;
        private AboutPage AboutPageFlyout = null;
      
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
            if (OptionsPageFlyout == null)
                OptionsPageFlyout = new OptionsPage();
            if (AboutPageFlyout == null)
                AboutPageFlyout = new AboutPage();
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested; //Settings flyout
            OptionsPageFlyout.SettingsChanged += this.MainPage_SettingsChanged;
            LoopTimer = new DispatcherTimer();
            DownloadTimer = new DispatcherTimer();
            LoopTimer.Tick += Timer_Handler;
            DownloadTimer.Tick += Timer_Handler;
            //Microsoft.Advertising.WinRT.UI.AdSettingsFlyout.HeaderBackground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF,0xA0,0x00,0x00));
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
            Task GetFileNamesTask, DownloadFilesTask;
            var LoadingimageUri = new Uri("ms-appx:///Assets/Loading.png");
            ImgBox.ImgSource = new BitmapImage(LoadingimageUri);

            LoadOverlayImages();
            SetNavigationButtonState(GenericCodeClass.IsLoopPaused, false);
            GetFileNamesTask = GenericCodeClass.GetListOfLatestFiles(Files);
            if(!GenericCodeClass.IsAppResuming)
                await GenericCodeClass.DeleteAllFiles(ImageFolder);
                                   
            StationBox.Text = GenericCodeClass.HomeStationName;
            GenericCodeClass.IsAppResuming = false;
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
                DownloadTimer.Start();
                SetNavigationButtonState(GenericCodeClass.IsLoopPaused,true);
                SetOverlayVisibilities(false);
            }
            else
            {
                //Show Error Message
                Uri ImageUri = new Uri("ms-appx:///Assets/Error.png");
                BitmapImage bitmap = ImgBox.ImgSource as BitmapImage;

                if (bitmap != null && bitmap.UriSource.AbsoluteUri != "ms-appx:/Assets/Error.png")
                    ImgBox.ImgSource = new BitmapImage(ImageUri);
            }    
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
            SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
            GenericCodeClass.IsAppResuming = true;
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

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Files.Count != 0)
                CurrImgIndex = ++CurrImgIndex % Files.Count;
            else
                CurrImgIndex = -1;

            await ChangeImage(CurrImgIndex);
        }

        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (Files.Count != 0)
                CurrImgIndex = (CurrImgIndex + Files.Count - 1) % Files.Count;
            else
                CurrImgIndex = -1;

            await ChangeImage(CurrImgIndex);
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            await GenericCodeClass.GetListOfLatestFiles(Files);
            await DownloadFiles();
            if(NextButton.IsEnabled == true)
                await ChangeImage(CurrImgIndex);
        }

        private async Task DownloadFiles()
        {
            int i;
            int RetCode;
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
                    continue;
                
                StatusBox.Text = "Downloading image " + DownloadedFiles.ToString() + "/" + Files.Count.ToString();
                FileDownloadProgBar.Visibility = Visibility.Visible;
                RetCode = await GenericCodeClass.GetFileUsingHttp(GenericCodeClass.HomeStation + Files[i], ImageFolder, Files[i]);
            
                if (RetCode == -1)
                {
                    Files.Remove(Files[i].ToString());
                }
                else
                {
                    ImgBox.ImgSource = await GenericCodeClass.GetBitmapImage(ImageFolder, Files[i]);
                    SetOverlayVisibilities(false);
                    DownloadedFiles += 1;
                    FileDownloadProgBar.Value += 1;
                    StatusBox.Text += " Finished.";
                }
            }

            FileDownloadProgBar.Visibility = Visibility.Collapsed;

            if (Files.Count > 1)
            {
                CurrImgIndex = 0;
                await ChangeImage(CurrImgIndex);
            }
            else
                CurrImgIndex = -1;
        }

        private async Task ChangeImage(int ImageIndex)
        {
            if (GenericCodeClass.IsLoopPaused == false)
                LoopTimer.Stop();   //Stop the loop timer to allow enough time to change image

            if (ImageFolder == null)
                ImageFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

            if (Files.Count != 0 && ImageIndex >= 0 && ImageIndex <= Files.Count)
            {
                DateTime LocalTime = GenericCodeClass.GetDateTimeFromFile(Files[ImageIndex]);
                StatusBox.Text = LocalTime.ToString("MMM dd HH:mm") + "   " + (ImageIndex + 1).ToString() + "/" + Files.Count.ToString() + " " + GenericCodeClass.RadarTypeString + "-" + GenericCodeClass.PrecipitationTypeString;
                ImgBox.ImgSource = await GenericCodeClass.GetBitmapImage(ImageFolder, Files[ImageIndex]);               
            }
            else
            {
                Uri ImageUri = new Uri("ms-appx:///Assets/Error.png");
                BitmapImage bitmap = ImgBox.ImgSource as BitmapImage;

                if (bitmap != null && bitmap.UriSource.AbsoluteUri != "ms-appx:/Assets/Error.png")
                    ImgBox.ImgSource = new BitmapImage(ImageUri);
                StatusBox.Text = "Error Downloading Images";
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
                await GenericCodeClass.GetListOfLatestFiles(Files);
                await DownloadFiles();
                DownloadTimer.Start();
            }
            LoopTimer.Start();
        }

        //Setting flyout
        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            //Do we also need privacy statement, about etc?
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Options", "Options", (handler) => ShowCustomSettingFlyout()));
            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "About", "About", (handler) => ShowAboutFlyout()));            
        }

        private void ShowCustomSettingFlyout()
        {
            if(OptionsPageFlyout == null)
                OptionsPageFlyout = new OptionsPage();
            OptionsPageFlyout.Show();
        }

        private void ShowAboutFlyout()
        {
            if (AboutPageFlyout == null)
                AboutPageFlyout = new AboutPage();
            AboutPageFlyout.Show();
        }
       //End settings flyout

        private async void MainPage_SettingsChanged(object sender, EventArgs e)
        {
            Task GetFileNamesTask, DeleteFilesTask, DownloadFilesTask;
            var LoadingimageUri = new Uri("ms-appx:///Assets/Loading.png");
            

            SetNavigationButtonState(GenericCodeClass.IsLoopPaused, false);

            if(GenericCodeClass.IsLoopPaused == false)
                LoopTimer.Stop();

             
            ImgBox.ImgSource = new BitmapImage(LoadingimageUri);

            GetFileNamesTask = GenericCodeClass.GetListOfLatestFiles(Files);

            if (GenericCodeClass.HomeStationChanged == true)
            {
                SetOverlayVisibilities(true);
                LoadOverlayImages();
                DeleteFilesTask = GenericCodeClass.DeleteAllFiles(ImageFolder);
                StationBox.Text = GenericCodeClass.HomeStationName;
                
                await DeleteFilesTask;
                //GenericCodeClass.HomeStationChanged = false;
            }

            await GetFileNamesTask;
            DownloadFilesTask = DownloadFiles();

            LoopTimer.Interval = GenericCodeClass.LoopInterval;

            try
            {
                await DownloadFilesTask; //maybe used the status field to check whether the task is worth waiting for
            }
            catch
            {

            }

            if (DownloadFilesTask.IsFaulted)
            {
                //Show Error Message
                Uri ImageUri = new Uri("ms-appx:///Assets/Error.png");
                BitmapImage bitmap = ImgBox.ImgSource as BitmapImage;

                if (bitmap != null && bitmap.UriSource.AbsoluteUri != "ms-appx:/Assets/Error.png")
                    ImgBox.ImgSource = new BitmapImage(ImageUri);
            }
            else
            {
                SetNavigationButtonState(GenericCodeClass.IsLoopPaused, true);
                if (GenericCodeClass.HomeStationChanged == false)
                    SetOverlayVisibilities(false);
                if (GenericCodeClass.IsLoopPaused == false)
                    LoopTimer.Start();
            }

            GenericCodeClass.HomeStationChanged = false;

            
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
                if (GenericCodeClass.CityOverlayFlag)
                    ImgBox.CitiesOverlayVisibility = Visibility.Visible;
                else
                    ImgBox.CitiesOverlayVisibility = Visibility.Collapsed;

//                if (GenericCodeClass.TownOverlayFlag)
//                    ImgBox.TownsOverlayVisibility = Visibility.Visible;
//                else
//                    ImgBox.TownsOverlayVisibility = Visibility.Collapsed;

                if (GenericCodeClass.RoadOverlayFlag)
                    ImgBox.RoadsOverlayVisibility = Visibility.Visible;
                else
                    ImgBox.RoadsOverlayVisibility = Visibility.Collapsed;

                if (GenericCodeClass.RoadNoOverlayFlag)
                    ImgBox.RoadNosOverlayVisibility = Visibility.Visible;
                else
                    ImgBox.RoadNosOverlayVisibility = Visibility.Collapsed;

                if (GenericCodeClass.RadarCircleOverlayFlag)
                    ImgBox.RadarCircleOverlayVisibility = Visibility.Visible;
                else
                    ImgBox.RadarCircleOverlayVisibility = Visibility.Collapsed;
            }
            else
            {
                ImgBox.CitiesOverlayVisibility = Visibility.Collapsed;
//                ImgBox.TownsOverlayVisibility = Visibility.Collapsed;
                ImgBox.RoadsOverlayVisibility = Visibility.Collapsed;
                ImgBox.RoadNosOverlayVisibility = Visibility.Collapsed;
                ImgBox.RadarCircleOverlayVisibility = Visibility.Collapsed;
            }            
        }

        private void LoadOverlayImages()
        {
            ImgBox.CitiesOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString.ToLower() + "_cities.gif")); ;
            //ImgBox.TownsOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString.ToLower() + "_moretowns.gif"));
            ImgBox.RoadsOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString.ToLower() + "_roads.gif"));
            ImgBox.RoadNosOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/" + GenericCodeClass.HomeStationCodeString + "/" + GenericCodeClass.HomeStationCodeString.ToLower() + "_numbers.gif"));
            ImgBox.RadarCircleOverlaySource = new BitmapImage(new Uri("ms-appx://Rad.Rad.Shared/Assets/Overlays/radar_circle.gif"));
        }
            
        //private void AdBox_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        //{
        //    AdBox.Visibility = Visibility.Collapsed;
        //}

    }
}
