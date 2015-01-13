using System;
using Windows.Web.Http;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;

static class GenericCodeClass
{
    private static TimeSpan LoopTimerInterval = new TimeSpan(0,0,0,0,500); //Loop timer interval in seconds
    private static TimeSpan DownloadTimerInterval = new TimeSpan(0,15,0); //Download time interval in minutes
    private static string HomeStationURL;
    private static string HomeStationString;
    private static bool IsHomeStationChanged = false;
    private static bool IsECLightningDataSelected = false;
    private static HttpClient Client;
    private static HttpResponseMessage Message;
    private static int DownloadPeriod = 1;  //Change this value to set the time period for which images are downloaded (1h or 3h)
    public static List<string> ExistingFiles = new List<string>();
    public static bool IsLoopPaused = false;
    public static bool IsAppResuming = false;
    private static string PrecipitationType;
    private static string RadarType;
    private static string HomeStationCode;
    private static string HomeProvince;
    private static bool OverlayCities;
    //private static bool OverlayTowns;
    private static bool OverlayRoads;
    private static bool OverlayRoadNos;
    private static bool OverlayCircles;
    private static bool IsCanadaSelected;
    private static int YearStart;
    private static int TimeStart;
    private static int DateStart;

    //Provide access to private property specifying Loop timer Interval
    public static TimeSpan LoopInterval
    {
        get { return LoopTimerInterval; }
        set { LoopTimerInterval = value; }
    }

    //Provide access to private property specifying Download timer Interval
    public static TimeSpan DownloadInterval
    {
        get { return DownloadTimerInterval; }
        set { DownloadTimerInterval = value; }
    }

    public static int FileDownloadPeriod
    {
        get { return DownloadPeriod; }
        set { DownloadPeriod = value; }
    }

    //Provide access to private property specifying whether home station has changed
    public static bool HomeStationChanged
    {
        get { return IsHomeStationChanged; }
        set { IsHomeStationChanged = value; }
    }

    //Provide access to private property specifying Home Weather Station
    public static string HomeStation
    {
        get { return HomeStationURL; }
        set 
        {
            if (HomeStationURL != value)
                IsHomeStationChanged = true;

            HomeStationURL = value;
        }
    }

    public static string HomeStationName
    {
        get { return HomeStationString; }
        set { HomeStationString = value;}
    }

    //Provide access to private property specifying whether home station has changed
    public static bool LightningDataSelected
    {
        get { return IsECLightningDataSelected; }
        set { IsECLightningDataSelected = value; }
    }

    //Provide access to private property specifying the type of precipitation (SNOW/RAIN)
    public static string PrecipitationTypeString
    {
        get { return PrecipitationType; }
        set { PrecipitationType = value; }
    }

    //Provide access to private property specifying the type of radar (CAPPI/PRECIPET)
    public static string RadarTypeString
    {
        get { return RadarType; }
        set { RadarType = value; }
    }

    //Provide access to private property specifying the home station code
    public static string HomeStationCodeString
    {
        get { return HomeStationCode; }
        set { HomeStationCode = value; }
    }

    //Provide access to private property specifying the home province
    public static string HomeProvinceName
    {
        get { return HomeProvince; }
        set { HomeProvince = value; }
    }

    public static bool CityOverlayFlag
    {
        get { return OverlayCities; }
        set { OverlayCities = value; }
    }

    public static bool RoadOverlayFlag
    {
        get { return OverlayRoads; }
        set { OverlayRoads = value; }
    }

    public static bool RoadNoOverlayFlag
    {
        get { return OverlayRoadNos; }
        set { OverlayRoadNos = value; }
    }

    public static bool RadarCircleOverlayFlag
    {
        get { return OverlayCircles; }
        set { OverlayCircles = value; }
    }

    public static bool CanadaSelected
    {
        get { return IsCanadaSelected; }
        set { IsCanadaSelected = value; }
    }

    public static DateTime GetDateTimeFromFile(string Filename)
    {
        DateTime LocalDateTime;

        string Time;
        string Year;
        string Day;
        string Month;

        Time = Filename.Substring(TimeStart, 4);
        Year = Filename.Substring(YearStart, 4);
        Day = Filename.Substring(DateStart+2, 2);
        Month = Filename.Substring(DateStart, 2);
        LocalDateTime = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), Convert.ToInt32(Day),
            Convert.ToInt32(Time.Substring(0, 2)), Convert.ToInt32(Time.Substring(2, 2)), 0);
        LocalDateTime = LocalDateTime.ToLocalTime();

        return LocalDateTime;
    }

    public static async Task GetListOfLatestFiles(List<string> FileNames, List<string> LegendFileNames, List<string> WarningFileNames)
    {
        var URI = new Uri(HomeStationURL);
        string StartDateTimeString;
        Regex RegExp;
        string RegExpString;
        
        ExistingFiles.Clear();

        if (IsHomeStationChanged == false)
        {
            foreach (string str in FileNames)
            {
                ExistingFiles.Add(str);
            }
        }
        
        FileNames.Clear();
        LegendFileNames.Clear();
        WarningFileNames.Clear();


        if (Client == null)
            Client = new HttpClient();
        
        //string RegExpString = ">\\s*";
        DateTime CurrDateTime = DateTime.Now.ToUniversalTime();
        DateTime StartOfYearDate = new DateTime(CurrDateTime.Year - 1, 12, 31);
        DateTime StartDateTime = CurrDateTime.Subtract(new TimeSpan(DownloadPeriod, 0, 0));    //Subtract 3 hours from the Current Time
        //TimeSpan NoOfDays = CurrDateTime.Subtract(StartOfYearDate);

        Client.DefaultRequestHeaders.IfModifiedSince = StartDateTime;
        var HttpClientTask = Client.GetAsync(URI);

        if(IsCanadaSelected)
        {
            StartDateTimeString = StartDateTime.Year.ToString() + StartDateTime.Month.ToString("D2") + StartDateTime.Day.ToString("D2") + StartDateTime.Hour.ToString("D2") + StartDateTime.Minute.ToString("D2")
            + "_" + HomeStationCode + "_" + RadarType + "_" + PrecipitationType + ".gif";

            RegExpString = ">[0-9]+_[A-Z]+_" + RadarType + "_" + PrecipitationType + ".gif<";

            if (RadarType.Equals("CAPPI"))
            {
                if (PrecipitationType.Equals("SNOW"))
                {
                    StartDateTimeString = StartDateTimeString.Insert(StartDateTimeString.Length - 8, "1.0_");
                    RegExpString = RegExpString.Insert(RegExpString.Length - 9, "1.0_");
                }
                else
                {
                    StartDateTimeString = StartDateTimeString.Insert(StartDateTimeString.Length - 8, "1.5_");
                    RegExpString = RegExpString.Insert(RegExpString.Length - 9, "1.5_");
                }

            }

            if (HomeProvince.Equals("Regional Composites"))
            {
                StartDateTimeString = StartDateTimeString.Insert(StartDateTimeString.Length - 4, "_WT");
                RegExpString = RegExpString.Insert(RegExpString.Length - 5, "_WT");
            }
            YearStart = 0;
            DateStart = 4;
            TimeStart = 8;
        }
        else
        {
            if (HomeProvince.Equals("Regional Composites"))
            {
                StartDateTimeString = HomeStationCode + "_" + StartDateTime.Year.ToString() + StartDateTime.Month.ToString("D2") + StartDateTime.Day.ToString("D2")
                    + "_" + StartDateTime.Hour.ToString("D2") + StartDateTime.Minute.ToString("D2") + ".gif";
                RegExpString = ">" + HomeStationCode + "_[0-9]+_[0-9]+.gif<";
                YearStart = HomeStationCode.Length + 1;
                DateStart = YearStart + 4;
                TimeStart = DateStart + 5;

            }
            else
            {
                StartDateTimeString = HomeStationCode + "_" + StartDateTime.Year.ToString() + StartDateTime.Month.ToString("D2") + StartDateTime.Day.ToString("D2") + "_" + StartDateTime.Hour.ToString("D2") + StartDateTime.Minute.ToString("D2")
            + "_" + RadarType + ".gif";

                RegExpString = ">[A-Z]+_[0-9]+_[0-9]+_" + RadarType + ".gif<";
                YearStart = 4;
                DateStart = 8;
                TimeStart = 13;
            }
        }
        
        
        FileNames.Add(StartDateTimeString);
        RegExp = new Regex(RegExpString);

        try
        {
            //Message = await Client.GetAsync(URI);
            Message = await HttpClientTask;
        }
        catch (Exception e)
        {
            FileNames.Remove(StartDateTimeString);
            return;
        }


        if (Message.IsSuccessStatusCode)
        {
            int MessageLength = Message.Content.ToString().Length;
            MatchCollection Matches = RegExp.Matches(Message.Content.ToString());//.Substring(MessageLength/2));
            int Location;
            
            if (Matches.Count > 0)
            {
                foreach (Match match in Matches)
                {
                    if (match.Success)
                    {
                        string tmp = match.ToString();	//The regular expression matches the "<" and ">" signs around the filename. These signs have to be removed before adding the filename to the list
                        FileNames.Add(tmp.Substring(1, tmp.Length - 2));
                    }
                }
                FileNames.Sort();
                Location = FileNames.IndexOf(StartDateTimeString);
                FileNames.RemoveRange(0, Location + 1);
                if (!IsCanadaSelected && !HomeProvince.Equals("Regional Composites"))
                {
                    foreach (string str in FileNames)
                    {
                        LegendFileNames.Add(str.Replace(".gif", "_Legend.gif"));
                        WarningFileNames.Add(str.Replace(GenericCodeClass.RadarTypeString + ".gif", "Warnings.gif"));
                    }
                }
            }
        }
        else
        {
            //return some sort of error code? Is it good to clear the StartFileName from the Filenames list?
        }
    }

    public static void GetWeatherDataURLs(List<string> FileNames, int NoOfFiles)
    {
        DateTime CurrDateTime = DateTime.Now.ToUniversalTime();
        int i;

        //No need to save previous files as that is done in the function GetLatestFiles()

        CurrDateTime = CurrDateTime.AddMinutes(-CurrDateTime.Minute % 10);


        for (i = 0; i < NoOfFiles; i++)
        {
            FileNames.Add("PAC_" + CurrDateTime.Year.ToString() + CurrDateTime.Month.ToString("D2") + CurrDateTime.Day.ToString("D2") + CurrDateTime.Hour.ToString("D2") + CurrDateTime.Minute.ToString("D2") + ".png");
            CurrDateTime = CurrDateTime.AddMinutes(-10);
        }

        FileNames.Reverse();
    }

    public static async Task<int> GetFileUsingHttp(string URL, StorageFolder Folder, string FileName)
    {
        var URI = new Uri(URL);
        StorageFile sampleFile;
                
        if (Client == null)
            Client = new HttpClient();

        Message = await Client.GetAsync(URI);

        if (Message.IsSuccessStatusCode)
        {
            sampleFile = await Folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);// this line throws an exception
            var FileBuffer = await Message.Content.ReadAsBufferAsync();
            await FileIO.WriteBufferAsync(sampleFile, FileBuffer);
            return 0; //Return code to show an image was successfully downloaded.
        }
        else
        {
            return -1; //Error code to show image was not downloaded successfully.
        }
    }

    public static bool FileExists(IReadOnlyList<StorageFile> FileList, string FileName)
    {
        int i;

        for (i = 0; i < FileList.Count; i++)
            if (FileName.Equals(FileList[i].Name))
                return true;

        return false;
    }

    public static async Task<BitmapImage> GetBitmapImage(StorageFolder ImageFolder, string FileName)
    {
        if (ImageFolder == null)
        {
            ImageFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
        }

        StorageFile ImageFile = await ImageFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
        BitmapImage Image;

        Image = await LoadBitmapImage(ImageFile);
                
        return Image;
    }

    private static async Task<BitmapImage> LoadBitmapImage(StorageFile file)
    {
        BitmapImage Image = new BitmapImage();
        FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

        Image.SetSource(stream);

        return Image;

    }

    public static async Task<WriteableBitmap> GetWriteableBitmap(StorageFolder ImageFolder, string FileName)
    {
        StorageFile ImageFile;
        WriteableBitmap ImageBitmap;
        //BitmapImage Image = new BitmapImage();

        if(ImageFolder == null)
        {
             ImageFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
        }

        ImageFile = await ImageFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
        ImageBitmap = await LoadWriteableBitmap(ImageFile);
               
        return ImageBitmap;
    }

    private static async Task<WriteableBitmap> LoadWriteableBitmap(StorageFile file)
    {
        WriteableBitmap ImageBitmap;
        FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

        ImageBitmap = new WriteableBitmap(720, 480);//Image.PixelWidth,Image.PixelHeight);
        ImageBitmap.SetSource(stream);

        return ImageBitmap;

    }

    public static async Task DeleteFiles(StorageFolder ImageFolder,List<string> FilesToDelete, bool DeleteAllFiles)
    {
        StorageFile File;
        int i;
        
        if (ImageFolder == null)
            ImageFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

        if(DeleteAllFiles)
        {
            //Get list of files currently in the local data folder
            var FileList = await ImageFolder.GetFilesAsync();
            for (i = 0; i < FileList.Count; i++)
            {
                File = await ImageFolder.GetFileAsync(FileList[i].Name);
                await File.DeleteAsync();
            }
        }
        else
        {
            for (i = 0; i < FilesToDelete.Count; i++)
            {
                File = await ImageFolder.GetFileAsync(FilesToDelete[i].ToString());
                await File.DeleteAsync();
            }
        }

                
    }
	
	public static void SaveAppData(bool SettingsChanged)
    {
        Windows.Storage.ApplicationDataContainer RoamingSettings =
                Windows.Storage.ApplicationData.Current.RoamingSettings;

        if(RoamingSettings != null)
        {
            if(SettingsChanged == true)
            {
                RoamingSettings.Values["RadarType"] = RadarType;
                RoamingSettings.Values["PrecipitationType"] = PrecipitationType;
                RoamingSettings.Values["HomeStationCode"] = HomeStationCode;
                RoamingSettings.Values["HomeStationURL"] = HomeStationURL;
                RoamingSettings.Values["HomeStationString"] = HomeStationString;
                RoamingSettings.Values["HomeProvince"] = HomeProvince;
                RoamingSettings.Values["DownloadPeriod"] = DownloadPeriod;
                RoamingSettings.Values["LoopTimerInterval"] = LoopTimerInterval.Milliseconds;
                RoamingSettings.Values["OverlayCities"] = OverlayCities;
                RoamingSettings.Values["OverlayRoadNos"] = OverlayRoadNos;
                RoamingSettings.Values["OverlayRoads"] = OverlayRoads;
                RoamingSettings.Values["OverlayCircles"] = OverlayCircles;
                RoamingSettings.Values["IsCanadaSelected"] = IsCanadaSelected;
            }
            
            RoamingSettings.Values["IsLoopPaused"] = GenericCodeClass.IsLoopPaused;
        }
    }

    public static void GetSavedAppData()
    {
        Windows.Storage.ApplicationDataContainer RoamingSettings =
                Windows.Storage.ApplicationData.Current.RoamingSettings;

        if(RoamingSettings != null)
        {
            try
            {
                HomeStationURL = RoamingSettings.Values["HomeStationURL"].ToString();
                LoopTimerInterval = new TimeSpan(0, 0, 0, 0, (int)RoamingSettings.Values["LoopTimerInterval"]);
                HomeStationString = RoamingSettings.Values["HomeStationString"].ToString();
                DownloadPeriod = (int)RoamingSettings.Values["DownloadPeriod"];
                PrecipitationType = RoamingSettings.Values["PrecipitationType"].ToString();
                RadarType = RoamingSettings.Values["RadarType"].ToString();
                HomeStationCode = RoamingSettings.Values["HomeStationCode"].ToString();
                HomeProvince = RoamingSettings.Values["HomeProvince"].ToString();
                OverlayCities = (bool)RoamingSettings.Values["OverlayCities"];
                OverlayRoadNos = (bool)RoamingSettings.Values["OverlayRoadNos"];
                OverlayRoads = (bool)RoamingSettings.Values["OverlayRoads"];
                OverlayCircles = (bool)RoamingSettings.Values["OverlayCircles"];
                IsLoopPaused = (bool)RoamingSettings.Values["IsLoopPaused"];
                IsCanadaSelected = (bool)RoamingSettings.Values["IsCanadaSelected"];

            }
            catch(Exception e)
            {
                HomeStationURL = "http://dd.weatheroffice.gc.ca/radar/CAPPI/GIF/WUJ/";
                LoopTimerInterval = new TimeSpan(0, 0, 0, 0, 500);
                HomeStationString = "Vancouver";
                DownloadPeriod = 1;
                PrecipitationType = "SNOW";
                RadarType = "CAPPI";
                HomeStationCode = "WUJ";
                HomeProvince = "British Columbia";
                OverlayCities = true;
                OverlayRoadNos = false;
                OverlayRoads = true;
                OverlayCircles = false;
                IsCanadaSelected = true;
            }
        }
    }
    public static bool IsError(string s)
    {
        return s.Equals("Error.png");
    }
}
