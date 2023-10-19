﻿using System.Globalization;
using System.Windows;
using Application = System.Windows.Application;
using SELLT = Skylark.Enum.LevelLogType;
using SEWTT = Skylark.Enum.WindowsThemeType;
using SHC = Skylark.Helper.Culture;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMM = Sucrose.Manager.Manage.Manager;
using SMR = Sucrose.Memory.Readonly;
using SWMM = Sucrose.Watchdog.Manage.Manager;
using SSRHR = Sucrose.Shared.Resources.Helper.Resources;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SWVDEMB = Sucrose.Watchdog.View.DarkErrorMessageBox;
using SWVLEMB = Sucrose.Watchdog.View.LightErrorMessageBox;
using SSWW = Sucrose.Shared.Watchdog.Watch;

namespace Sucrose.Watchdog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool HasError { get; set; } = true;

        public App()
        {
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                Exception Exception = e.Exception;

                SSWW.Watch_FirstChanceException(Exception);

                //Close();
                //Message(Exception.Message);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception Exception = (Exception)e.ExceptionObject;

                SSWW.Watch_GlobalUnhandledException(Exception);

                //Close();
                Message(Exception.Message);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Exception Exception = e.Exception;

                SSWW.Watch_UnobservedTaskException(Exception);

                e.SetObserved();

                //Close();
                Message(Exception.Message);
            };

            Current.DispatcherUnhandledException += (s, e) =>
            {
                Exception Exception = e.Exception;

                SSWW.Watch_DispatcherUnhandledException(Exception);

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

                string Path = SMMI.WatchdogLogManager.LogFile();

                switch (SWMM.ThemeType)
                {
                    case SEWTT.Dark:
                        SWVDEMB DarkMessageBox = new(Message, Path);
                        DarkMessageBox.ShowDialog();
                        break;
                    default:
                        SWVLEMB LightMessageBox = new(Message, Path);
                        LightMessageBox.ShowDialog();
                        break;
                }

                Close();
            }
        }

        protected void Configure(string[] Args)
        {
            if (Args.Any())
            {
                string[] Arguments = Args.First().Split(SMR.ValueSeparatorChar);

                if (Arguments.Any() && (Arguments.Count() == 2 || Arguments.Count() == 4))
                {
                    string Path = Arguments[1];
                    string Message = Arguments[0];
                    string Source = Arguments.Count() == 4 ? Arguments[2] : string.Empty;
                    string Text = Arguments.Count() == 4 ? Arguments[3] : string.Empty;

                    switch (SWMM.ThemeType)
                    {
                        case SEWTT.Dark:
                            SWVDEMB DarkMessageBox = new(Message, Path, Source, Text);
                            DarkMessageBox.ShowDialog();
                            break;
                        default:
                            SWVLEMB LightMessageBox = new(Message, Path, Source, Text);
                            LightMessageBox.ShowDialog();
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

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SSRHR.SetLanguage(SMMM.Culture);

            ShutdownMode = ShutdownMode.OnLastWindowClose;

            Configure(e.Args);
        }
    }
}