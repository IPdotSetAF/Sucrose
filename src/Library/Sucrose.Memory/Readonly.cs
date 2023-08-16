﻿namespace Sucrose.Memory
{
    public static class Readonly
    {
        public static readonly string Language = "EN";

        public static readonly string Store = "Store";

        public static readonly string Owner = "Taiizor";

        public static readonly Random Randomise = new();

        public static readonly string LogFolder = "Log";

        public static readonly string GifFolder = "Gif";

        public static readonly string Bundle = "Bundle";

        public static readonly string Key = string.Empty;

        public static readonly string Branch = "develop";

        public static readonly string AppName = "Sucrose";

        public static readonly string Content = "Content";

        public static readonly char StartCommandChar = '-';

        public static readonly string CacheFolder = "Cache";

        public static readonly string CefSharp = "CefSharp";

        public static readonly string WebView2 = "WebView2";

        public static readonly string Repository = "Sucrose";

        public static readonly char ValueSeparatorChar = '|';

        public static readonly string WallpaperSource = "src";

        public static readonly string StoreFile = "Store.json";

        public static readonly string SettingFolder = "Setting";

        public static readonly string Update = "Sucrose.Update.exe";

        public static readonly string Portal = "Sucrose.Portal.exe";

        public static readonly string TaskName = "Autorun for Sucrose";

        public static readonly string SucroseInfo = "SucroseInfo.json";

        public static readonly string WallpaperRepository = "Wallpaper";

        public static readonly string Launcher = "Sucrose.Launcher.exe";

        public static readonly string VideoContent = "VideoContent.html";

        public static readonly string Commandog = "Sucrose.Commandog.exe";

        public static readonly string StartCommand = $"{StartCommandChar}";

        public static readonly string NebulaLive = "Sucrose.Live.Nebula.exe";

        public static readonly string VexanaLive = "Sucrose.Live.Vexana.exe";

        public static readonly string AuroraLive = "Sucrose.Live.Aurora.exe";

        public static readonly string YouTubeContent = "YouTubeContent.html";

        public static readonly string LiveMutex = "{Sucrose-Wallpaper-Live}";

        public static readonly string UserAgent = "Sucrose Wallpaper Engine";

        public static readonly string WebViewLive = "Sucrose.Live.WebView.exe";

        public static readonly string ValueSeparator = $"{ValueSeparatorChar}";

        public static readonly string LogDescription = "SucroseWatchdog Thread";

        public static readonly string UpdateMutex = "{Sucrose-Wallpaper-Update}";

        public static readonly string DiscordApplication = "1126294965950103612";

        public static readonly string CefSharpLive = "Sucrose.Live.CefSharp.exe";

        public static readonly string TaskDescription = "Sucrose Wallpaper Engine";

        public static readonly string SucroseProperties = "SucroseProperties.json";

        public static readonly string SucroseCompatible = "SucroseCompatible.json";

        public static readonly string PortalMutex = "{Sucrose-Wallpaper-Engine-Portal}";

        public static readonly string LauncherMutex = "{Sucrose-Wallpaper-Engine-Launcher}";

        public static readonly string BrowseWebsite = $"https://github.com/{Owner}/{Repository}";

        public static readonly string WallpaperWebsite = $"https://github.com/{Owner}/{WallpaperRepository}";

        public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public static readonly string ReportWebsite = $"https://github.com/{Owner}/{Repository}/issues/new/choose";

        public static readonly string DownloadWebsite = $"https://github.com/{Owner}/{Repository}/releases/latest";

        public static readonly string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}