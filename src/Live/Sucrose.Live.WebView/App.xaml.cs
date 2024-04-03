﻿using Microsoft.Web.WebView2.Core;
using System.Globalization;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using SHC = Skylark.Helper.Culture;
using SMC = Sucrose.Memory.Constant;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMM = Sucrose.Manager.Manage.Manager;
using SMR = Sucrose.Memory.Readonly;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSDEWT = Sucrose.Shared.Dependency.Enum.WallpaperType;
using SSEHR = Sucrose.Shared.Engine.Helper.Run;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEWVMI = Sucrose.Shared.Engine.WebView.Manage.Internal;
using SSEWVVG = Sucrose.Shared.Engine.WebView.View.Gif;
using SSEWVVU = Sucrose.Shared.Engine.WebView.View.Url;
using SSEWVVV = Sucrose.Shared.Engine.WebView.View.Video;
using SSEWVVW = Sucrose.Shared.Engine.WebView.View.Web;
using SSEWVVYT = Sucrose.Shared.Engine.WebView.View.YouTube;
using SSLHK = Sucrose.Shared.Live.Helper.Kill;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHS = Sucrose.Shared.Space.Helper.Security;
using SSSHW = Sucrose.Shared.Space.Helper.Watchdog;
using SSTHC = Sucrose.Shared.Theme.Helper.Compatible;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSTHV = Sucrose.Shared.Theme.Helper.Various;
using SSWW = Sucrose.Shared.Watchdog.Watch;

namespace Sucrose.Live.WebView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool HasError { get; set; } = true;

        public App()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);

            System.Windows.Forms.Application.ThreadException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWW.Watch_ThreadException(Exception);

                //Close();
                Message(Exception.Message);
            };

            AppDomain.CurrentDomain.FirstChanceException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWW.Watch_FirstChanceException(Exception);

                //Close();
                //Message(Exception.Message);
            };

            AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
            {
                Exception Exception = (Exception)e.ExceptionObject;

                await SSWW.Watch_GlobalUnhandledException(Exception);

                //Close();
                Message(Exception.Message);
            };

            TaskScheduler.UnobservedTaskException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWW.Watch_UnobservedTaskException(Exception);

                e.SetObserved();

                //Close();
                Message(Exception.Message);
            };

            Current.DispatcherUnhandledException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWW.Watch_DispatcherUnhandledException(Exception);

                e.Handled = true;

                //Close();
                Message(Exception.Message);
            };

            SHC.All = new CultureInfo(SMMM.Culture, true);
        }

        protected void Close()
        {
            Environment.Exit(0);
            Current.Shutdown();
            Shutdown();
        }

        protected void Message(string Message)
        {
            if (HasError)
            {
                HasError = false;

                string Path = SMMI.WebViewLiveLogManager.LogFile();

                SSSHW.Start(Message, Path);

                Close();
            }
        }

        protected void Configure()
        {
            if (SMMI.LibrarySettingManager.CheckFile() && !string.IsNullOrEmpty(SMMM.LibrarySelected))
            {
                string InfoPath = Path.Combine(SMMM.LibraryLocation, SMMM.LibrarySelected, SMR.SucroseInfo);
                string PropertiesPath = Path.Combine(SMMM.LibraryLocation, SMMM.LibrarySelected, SMR.SucroseProperties);
                string CompatiblePath = Path.Combine(SMMM.LibraryLocation, SMMM.LibrarySelected, SMR.SucroseCompatible);

                if (File.Exists(InfoPath))
                {
                    SSLHK.StopSubprocess();

                    CoreWebView2EnvironmentOptions Options = new()
                    {
                        Language = SMMM.Culture
                    };

                    SSEMI.BrowserSettings.WebView = SMMM.WebArguments;

                    if (!SSEMI.BrowserSettings.WebView.Any())
                    {
                        SSEMI.BrowserSettings.WebView = SSEMI.WebArguments;

                        SMMI.EngineSettingManager.SetSetting(SMC.WebArguments, SSEMI.BrowserSettings.WebView);
                    }

                    Options.AdditionalBrowserArguments = string.Join(" ", SSEMI.BrowserSettings.WebView);

                    Task<CoreWebView2Environment> Environment = CoreWebView2Environment.CreateAsync(null, Path.Combine(SMR.AppDataPath, SMR.AppName, SMR.CacheFolder, SMR.WebView2), Options);

                    SSEWVMI.WebEngine.EnsureCoreWebView2Async(Environment.Result);

                    SSTHI Info = SSTHI.ReadJson(InfoPath);

                    string Source = Info.Source;

                    if (!SSTHV.IsUrl(Source))
                    {
                        Source = Path.Combine(SMMM.LibraryLocation, SMMM.LibrarySelected, Source);
                    }

                    SMMI.BackgroundogSettingManager.SetSetting(SMC.PipeRequired, false);
                    SMMI.BackgroundogSettingManager.SetSetting(SMC.AudioRequired, false);
                    SMMI.BackgroundogSettingManager.SetSetting(SMC.SignalRequired, false);

                    if (SSTHV.IsUrl(Source) || File.Exists(Source))
                    {
                        SSSHS.Apply();

                        if (File.Exists(PropertiesPath))
                        {
                            SSEMI.Properties = SSTHP.ReadJson(PropertiesPath);
                            SSEMI.Properties.State = true;
                        }

                        if (File.Exists(CompatiblePath))
                        {
                            SSEMI.Compatible = SSTHC.ReadJson(CompatiblePath);
                            SSEMI.Compatible.State = true;
                        }

                        switch (Info.Type)
                        {
                            case SSDEWT.Gif:
                                SSEWVVG Gif = new(Source);
                                Gif.Show();
                                break;
                            case SSDEWT.Url:
                                SSEWVVU Url = new(Source);
                                Url.Show();
                                break;
                            case SSDEWT.Web:
                                SSEWVVW Web = new(Source);
                                Web.Show();
                                break;
                            case SSDEWT.Video:
                                SSEWVVV Video = new(Source);
                                Video.Show();
                                break;
                            case SSDEWT.YouTube:
                                SSEWVVYT YouTube = new(Source);
                                YouTube.Show();
                                break;
                            default:
                                Close();
                                break;
                        }
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            //

            Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SRHR.SetLanguage(SMMM.Culture);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (SSSHI.Basic(SMR.LiveMutex, SMR.WebViewLive) && SSEHR.Check())
            {
                Configure();
            }
            else
            {
                Close();
            }
        }
    }
}