﻿using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using SCHB = Skylark.Clipboard.Helper.Board;
using SEWTT = Skylark.Enum.WindowsThemeType;
using SMC = Sucrose.Memory.Constant;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMM = Sucrose.Manager.Manage.Manager;
using SMR = Sucrose.Memory.Readonly;
using SPMI = Sucrose.Portal.Manage.Internal;
using SPMM = Sucrose.Portal.Manage.Manager;
using SPSCIW = Sucrose.Portal.Services.Contracts.IWindow;
using SPVCTI = Sucrose.Portal.Views.Controls.TrayIcon;
using SPVMWMWVM = Sucrose.Portal.ViewModels.Windows.MainWindowViewModel;
using SPVPLP = Sucrose.Portal.Views.Pages.LibraryPage;
using SPVPSGSP = Sucrose.Portal.Views.Pages.Setting.GeneralSettingPage;
using SPVPSP = Sucrose.Portal.Views.Pages.StorePage;
using SPVPSSSP = Sucrose.Portal.Views.Pages.Setting.SystemSettingPage;
using SSCHV = Sucrose.Shared.Core.Helper.Version;
using SSDEACT = Sucrose.Shared.Dependency.Enum.ArgumentCommandsType;
using SSDMM = Sucrose.Shared.Dependency.Manage.Manager;
using SSSHU = Sucrose.Shared.Space.Helper.User;
using SSSMSD = Sucrose.Shared.Space.Model.SearchData;
using SSWW = Sucrose.Shared.Watchdog.Watch;
using SWHWT = Skylark.Wing.Helper.WindowsTheme;
using SXAGAB = Sucrose.XamlAnimatedGif.AnimationBehavior;

namespace Sucrose.Portal.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SPSCIW, IDisposable
    {
        private CancellationTokenSource Searching { get; set; }

        private string ActivePage { get; set; }

        public SPVMWMWVM ViewModel { get; }

        public MainWindow(SPVMWMWVM ViewModel, INavigationService NavigationService, IServiceProvider ServiceProvider, ISnackbarService SnackbarService, IContentDialogService ContentDialogService)
        {
            this.ViewModel = ViewModel;
            DataContext = this;

            InitializeComponent();

            if (SPMM.BackdropType == WindowBackdropType.Auto)
            {
                if (SWHWT.GetTheme() == SEWTT.Dark)
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    Light.Visibility = Visibility.Collapsed;
                    SMMI.GeneralSettingManager.SetSetting(SMC.ThemeType, SEWTT.Dark);
                }
                else
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    Dark.Visibility = Visibility.Collapsed;
                    SMMI.GeneralSettingManager.SetSetting(SMC.ThemeType, SEWTT.Light);
                }
            }
            else
            {
                if (SSDMM.ThemeType == SEWTT.Dark)
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    Light.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    Dark.Visibility = Visibility.Collapsed;
                }
            }

            RootView.SetServiceProvider(ServiceProvider);

            NavigationService.SetNavigationControl(RootView);
            ContentDialogService.SetDialogHost(RootContentDialog);
            SnackbarService.SetSnackbarPresenter(SnackbarPresenter);

            SPMI.ServiceProvider = ServiceProvider;
            SPMI.SnackbarService = SnackbarService;
            SPMI.NavigationService = NavigationService;
            SPMI.ContentDialogService = ContentDialogService;

            string[] Args = Environment.GetCommandLineArgs();

            if (Args.Count() > 1 && Args[1] == $"{SSDEACT.SystemSetting}")
            {
                ApplySetting(false);
                RootView.Loaded += (_, _) => RootView.Navigate(typeof(SPVPSSSP));
            }
            else if (Args.Count() > 1 && Args[1] == $"{SSDEACT.GeneralSetting}")
            {
                ApplySetting(false);
                RootView.Loaded += (_, _) => RootView.Navigate(typeof(SPVPSGSP));
            }
            else
            {
                ApplyGeneral(false);
                RootView.Loaded += (_, _) => RootView.Navigate(typeof(SPVPLP));
            }

            SXAGAB.SetClientUserAgent(SMMM.UserAgent);
            SXAGAB.SetDownloadCacheExpiration(TimeSpan.FromHours(SMMM.StoreDuration));
            SXAGAB.SetDownloadCacheLocation(Path.Combine(SMR.AppDataPath, SMR.AppName, SMR.CacheFolder, SMR.Store, SMR.Temporary));
        }

        private void ApplyTheme(Button Button)
        {
            if (Button.Name == "Dark")
            {
                Dark.Visibility = Visibility.Collapsed;
                Light.Visibility = Visibility.Visible;
            }
            else
            {
                Dark.Visibility = Visibility.Visible;
                Light.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplySearch(double Width)
        {
            if (ViewModel.Donater == Visibility.Visible)
            {
                SearchBox.Margin = new Thickness(0, 0, ((Width - SearchBox.MaxWidth) / 2) - 250, 0);
            }
            else
            {
                SearchBox.Margin = new Thickness(0, 0, ((Width - SearchBox.MaxWidth) / 2) - 230, 0);
            }
        }

        private void ApplyGeneral(bool Mode = true)
        {
            foreach (NavigationViewItem Menu in RootView.MenuItems)
            {
                if (Menu.Name.Contains("General"))
                {
                    Menu.Visibility = Visibility.Visible;
                }
                else
                {
                    Menu.Visibility = Visibility.Collapsed;
                }
            }

            FooterDock.Visibility = Visibility.Visible;
            Setting.Visibility = Visibility.Visible;

            if (Mode)
            {
                RootView.Navigate(typeof(SPVPLP));
            }
        }

        private void ApplySetting(bool Mode = true)
        {
            foreach (NavigationViewItem Menu in RootView.MenuItems)
            {
                if (Menu.Name.Contains("Setting"))
                {
                    Menu.Visibility = Visibility.Visible;
                }
                else
                {
                    Menu.Visibility = Visibility.Collapsed;
                }
            }

            FooterDock.Visibility = Visibility.Collapsed;
            Setting.Visibility = Visibility.Collapsed;

            if (Mode)
            {
                RootView.Navigate(typeof(SPVPSGSP));
            }
            else
            {
                ApplySearch(Width);
            }
        }

        private void ThemeChange_click(object sender, RoutedEventArgs e)
        {
            ApplyTheme(sender as Button);

            Dispose();
        }

        private void OtherOptions_Click(object sender, RoutedEventArgs e)
        {
            OtherOptions.ContextMenu.PlacementTarget = OtherOptions;
            OtherOptions.ContextMenu.IsOpen = true;
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Topmost = false;
            ShowInTaskbar = true;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                e.Handled = true;
            }
        }

        private void NavigationChange_Click(object sender, RoutedEventArgs e)
        {
            NavigationViewItem View = sender as NavigationViewItem;

            if (View.Name == "Setting")
            {
                ApplySetting();
            }
            else
            {
                ApplyGeneral();
            }
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (SMMM.HintTrayIcon)
            {
                e.Cancel = true;

                SPVCTI TrayIcon = new();

                await TrayIcon.ShowAsync();

                TrayIcon.Dispose();

                Close();
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplySearch(e.NewSize.Width);

            Dispose();
        }

        private void Quoting_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SCHB.SetText(Quoting.Text);
        }

        private void RootView_Navigated(NavigationView sender, NavigatedEventArgs args)
        {
            Dispose();
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Searching?.Cancel();

            Searching = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, Searching.Token);

                SPMI.SearchService.SearchText = SearchBox.Text;

                if (!string.IsNullOrEmpty(SearchBox.Text) && !string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    try
                    {
                        if (SMMM.Statistics)
                        {
                            using HttpClient Client = new();

                            HttpResponseMessage Response = new();

                            Client.DefaultRequestHeaders.Add("User-Agent", SMMM.UserAgent);

                            try
                            {
                                SSSMSD SearchData = new(ActivePage, SearchBox.Text.Trim(), SSCHV.GetText());

                                StringContent Content = new(JsonConvert.SerializeObject(SearchData, Formatting.Indented), Encoding.UTF8, "application/json");

                                Response = await Client.PostAsync($"{SMR.SoferityWebsite}/{SMR.SoferityVersion}/{SMR.SoferityReport}/{SMR.SoferitySearch}/{SSSHU.GetGuid()}", Content);
                            }
                            catch (Exception Exception)
                            {
                                await SSWW.Watch_CatchException(Exception);
                            }
                        }
                    }
                    catch (Exception Exception)
                    {
                        await SSWW.Watch_CatchException(Exception);
                    }
                }

                Dispose();
            }
            catch (TaskCanceledException) { }
        }

        private void RootView_Navigating(NavigationView sender, NavigatingCancelEventArgs args)
        {
            ActivePage = Type.GetType($"{args.Page}") switch
            {
                Type T when T == typeof(SPVPLP) => "Library",
                Type T when T == typeof(SPVPSP) => "Store",
                _ => SMR.Unknown,
            };

            Dispose();
        }

        public void Dispose()
        {
            RootView.ClearJournal();

            ViewModel?.Dispose();

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}