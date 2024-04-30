﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using SEIT = Skylark.Enum.InputType;
using SEST = Skylark.Enum.ScreenType;
using SMC = Sucrose.Memory.Constant;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMM = Sucrose.Manager.Manage.Manager;
using SPMM = Sucrose.Portal.Manage.Manager;
using SPVCEC = Sucrose.Portal.Views.Controls.ExpanderCard;
using SRER = Sucrose.Resources.Extension.Resources;
using SSDEAET = Sucrose.Shared.Dependency.Enum.ApplicationEngineType;
using SSDEET = Sucrose.Shared.Dependency.Enum.EngineType;
using SSDEGET = Sucrose.Shared.Dependency.Enum.GifEngineType;
using SSDEST = Sucrose.Shared.Dependency.Enum.StretchType;
using SSDEUET = Sucrose.Shared.Dependency.Enum.UrlEngineType;
using SSDEVET = Sucrose.Shared.Dependency.Enum.VideoEngineType;
using SSDEWET = Sucrose.Shared.Dependency.Enum.WebEngineType;
using SSDEYTET = Sucrose.Shared.Dependency.Enum.YouTubeEngineType;
using SSLMM = Sucrose.Shared.Live.Manage.Manager;
using SWUD = Skylark.Wing.Utility.Desktop;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Sucrose.Portal.ViewModels.Pages
{
    public partial class WallpaperSettingViewModel : ObservableObject, INavigationAware, IDisposable
    {
        [ObservableProperty]
        private List<UIElement> _Contents = new();

        private bool _isInitialized;

        public WallpaperSettingViewModel()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        private void InitializeViewModel()
        {
            TextBlock AppearanceBehaviorArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "AppearanceBehavior"),
                Margin = new Thickness(0, 0, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(AppearanceBehaviorArea);

            SPVCEC InputMode = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = true,
                IsExpand = true
            };

            InputMode.LeftIcon.Symbol = SymbolRegular.KeyboardMouse16;
            InputMode.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "InputMode");
            InputMode.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "InputMode", "Description");

            ComboBox InputType = new();

            InputType.SelectionChanged += (s, e) => InputTypeSelected(InputMode, InputType.SelectedIndex);

            foreach (SEIT Type in Enum.GetValues(typeof(SEIT)))
            {
                InputType.Items.Add(new ComboBoxItem()
                {
                    Content = SRER.GetValue("Portal", "Enum", "InputType", $"{Type}")
                });
            }

            InputType.SelectedIndex = (int)SMMM.InputType;

            InputMode.HeaderFrame = InputType;

            StackPanel InputContent = new()
            {
                Orientation = Orientation.Vertical
            };

            CheckBox InputDesktop = new()
            {
                Content = SRER.GetValue("Portal", "WallpaperSettingPage", "InputMode", "InputDesktop"),
                IsChecked = SMMM.InputDesktop
            };

            InputDesktop.Checked += (s, e) => InputDesktopChecked(true);
            InputDesktop.Unchecked += (s, e) => InputDesktopChecked(false);

            CheckBox DesktopIcon = new()
            {
                Content = SRER.GetValue("Portal", "WallpaperSettingPage", "InputMode", "DesktopIcon"),
                IsChecked = !SWUD.GetDesktopIconVisibility(),
                Margin = new Thickness(0, 10, 0, 0)
            };

            DesktopIcon.Checked += (s, e) => DesktopIconChecked(false);
            DesktopIcon.Unchecked += (s, e) => DesktopIconChecked(true);

            InputContent.Children.Add(InputDesktop);
            InputContent.Children.Add(DesktopIcon);

            InputMode.FooterCard = InputContent;

            Contents.Add(InputMode);

            SPVCEC ScreenLayout = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            ScreenLayout.LeftIcon.Symbol = SymbolRegular.DesktopFlow24;
            ScreenLayout.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "ScreenLayout");
            ScreenLayout.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "ScreenLayout", "Description");

            ComboBox ScreenType = new();

            ScreenType.SelectionChanged += (s, e) => ScreenTypeSelected(ScreenType.SelectedIndex);

            foreach (SEST Type in Enum.GetValues(typeof(SEST)))
            {
                ScreenType.Items.Add(new ComboBoxItem()
                {
                    Content = SRER.GetValue("Portal", "Enum", "ScreenType", $"{Type}")
                });
            }

            ScreenType.SelectedIndex = (int)SPMM.ScreenType;

            ScreenLayout.HeaderFrame = ScreenType;

            Contents.Add(ScreenLayout);

            SPVCEC StretchMode = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            StretchMode.LeftIcon.Symbol = SymbolRegular.ArrowMinimize24;
            StretchMode.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "StretchMode");
            StretchMode.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "StretchMode", "Description");

            ComboBox StretchType = new();

            StretchType.SelectionChanged += (s, e) => StretchTypeSelected(StretchType.SelectedIndex);

            foreach (SSDEST Type in Enum.GetValues(typeof(SSDEST)))
            {
                StretchType.Items.Add(new ComboBoxItem()
                {
                    Content = SRER.GetValue("Portal", "Enum", "StretchType", $"{Type}")
                });
            }

            StretchType.SelectedIndex = (int)SPMM.StretchType;

            StretchMode.HeaderFrame = StretchType;

            Contents.Add(StretchMode);

            SPVCEC LoopMode = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            LoopMode.LeftIcon.Symbol = SymbolRegular.ArrowRepeatAll24;
            LoopMode.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "LoopMode");
            LoopMode.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "LoopMode", "Description");

            ToggleSwitch LoopState = new()
            {
                IsChecked = SMMM.Loop
            };

            LoopState.Checked += (s, e) => LoopStateChecked(true);
            LoopState.Unchecked += (s, e) => LoopStateChecked(false);

            LoopMode.HeaderFrame = LoopState;

            Contents.Add(LoopMode);

            SPVCEC ShuffleMode = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            ShuffleMode.LeftIcon.Symbol = SymbolRegular.ArrowShuffle24;
            ShuffleMode.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "ShuffleMode");
            ShuffleMode.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "ShuffleMode", "Description");

            ToggleSwitch ShuffleState = new()
            {
                IsChecked = SMMM.Shuffle
            };

            ShuffleState.Checked += (s, e) => ShuffleStateChecked(true);
            ShuffleState.Unchecked += (s, e) => ShuffleStateChecked(false);

            ShuffleMode.HeaderFrame = ShuffleState;

            Contents.Add(ShuffleMode);

            TextBlock ExtensionsArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "Extensions"),
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(ExtensionsArea);

            SPVCEC GifPlayer = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            GifPlayer.LeftIcon.Symbol = SymbolRegular.Gif24;
            GifPlayer.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "GifPlayer");
            GifPlayer.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "GifPlayer", "Description");

            ComboBox GifEngine = new();

            GifEngine.SelectionChanged += (s, e) => GifEngineSelected(GifEngine.SelectedItem as ComboBoxItem);

            foreach (SSDEGET Type in Enum.GetValues(typeof(SSDEGET)))
            {
                GifEngine.Items.Add(new ComboBoxItem()
                {
                    IsSelected = Type == (SSDEGET)SSLMM.GApp,
                    Content = $"{Type}"
                });
            }

            GifPlayer.HeaderFrame = GifEngine;

            Contents.Add(GifPlayer);

            SPVCEC VideoPlayer = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            VideoPlayer.LeftIcon.Symbol = SymbolRegular.VideoClip24;
            VideoPlayer.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "VideoPlayer");
            VideoPlayer.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "VideoPlayer", "Description");

            ComboBox VideoEngine = new();

            VideoEngine.SelectionChanged += (s, e) => VideoEngineSelected(VideoEngine.SelectedItem as ComboBoxItem);

            foreach (SSDEVET Type in Enum.GetValues(typeof(SSDEVET)))
            {
                VideoEngine.Items.Add(new ComboBoxItem()
                {
                    IsSelected = Type == (SSDEVET)SSLMM.VApp,
                    Content = $"{Type}"
                });
            }

            VideoPlayer.HeaderFrame = VideoEngine;

            Contents.Add(VideoPlayer);

            TextBlock EnginesArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "Engines"),
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(EnginesArea);

            SPVCEC UrlPlayer = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            UrlPlayer.LeftIcon.Symbol = SymbolRegular.SlideLink24;
            UrlPlayer.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "UrlPlayer");
            UrlPlayer.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "UrlPlayer", "Description");

            ComboBox UrlEngine = new();

            UrlEngine.SelectionChanged += (s, e) => UrlEngineSelected(UrlEngine.SelectedItem as ComboBoxItem);

            foreach (SSDEUET Type in Enum.GetValues(typeof(SSDEUET)))
            {
                UrlEngine.Items.Add(new ComboBoxItem()
                {
                    IsSelected = Type == (SSDEUET)SSLMM.UApp,
                    Content = $"{Type}"
                });
            }

            UrlPlayer.HeaderFrame = UrlEngine;

            Contents.Add(UrlPlayer);

            SPVCEC WebPlayer = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            WebPlayer.LeftIcon.Symbol = SymbolRegular.GlobeDesktop24;
            WebPlayer.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "WebPlayer");
            WebPlayer.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "WebPlayer", "Description");

            ComboBox WebEngine = new();

            WebEngine.SelectionChanged += (s, e) => WebEngineSelected(WebEngine.SelectedItem as ComboBoxItem);

            foreach (SSDEWET Type in Enum.GetValues(typeof(SSDEWET)))
            {
                WebEngine.Items.Add(new ComboBoxItem()
                {
                    IsSelected = Type == (SSDEWET)SSLMM.WApp,
                    Content = $"{Type}"
                });
            }

            WebPlayer.HeaderFrame = WebEngine;

            Contents.Add(WebPlayer);

            SPVCEC YouTubePlayer = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            YouTubePlayer.LeftIcon.Symbol = SymbolRegular.VideoRecording20;
            YouTubePlayer.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "YouTubePlayer");
            YouTubePlayer.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "YouTubePlayer", "Description");

            ComboBox YouTubeEngine = new();

            YouTubeEngine.SelectionChanged += (s, e) => YouTubeEngineSelected(YouTubeEngine.SelectedItem as ComboBoxItem);

            foreach (SSDEYTET Type in Enum.GetValues(typeof(SSDEYTET)))
            {
                YouTubeEngine.Items.Add(new ComboBoxItem()
                {
                    IsSelected = Type == (SSDEYTET)SSLMM.YApp,
                    Content = $"{Type}"
                });
            }

            YouTubePlayer.HeaderFrame = YouTubeEngine;

            Contents.Add(YouTubePlayer);

            SPVCEC ApplicationPlayer = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            ApplicationPlayer.LeftIcon.Symbol = SymbolRegular.AppGeneric24;
            ApplicationPlayer.Title.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "ApplicationPlayer");
            ApplicationPlayer.Description.Text = SRER.GetValue("Portal", "WallpaperSettingPage", "ApplicationPlayer", "Description");

            ComboBox ApplicationEngine = new();

            ApplicationEngine.SelectionChanged += (s, e) => ApplicationEngineSelected(ApplicationEngine.SelectedItem as ComboBoxItem);

            foreach (SSDEAET Type in Enum.GetValues(typeof(SSDEAET)))
            {
                ApplicationEngine.Items.Add(new ComboBoxItem()
                {
                    IsSelected = Type == (SSDEAET)SSLMM.AApp,
                    Content = $"{Type}"
                });
            }

            ApplicationPlayer.HeaderFrame = ApplicationEngine;

            Contents.Add(ApplicationPlayer);

            _isInitialized = true;
        }

        public void OnNavigatedTo()
        {
            //
        }

        public void OnNavigatedFrom()
        {
            //Dispose();
        }

        private void LoopStateChecked(bool State)
        {
            SMMI.EngineSettingManager.SetSetting(SMC.Loop, State);
        }

        private void ScreenTypeSelected(int Index)
        {
            SEST NewScreen = (SEST)Index;

            if (NewScreen != SPMM.ScreenType)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.ScreenType, NewScreen);
            }
        }

        private void StretchTypeSelected(int Index)
        {
            SSDEST NewStretch = (SSDEST)Index;

            if (NewStretch != SPMM.StretchType)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.StretchType, NewStretch);
            }
        }

        private void DesktopIconChecked(bool State)
        {
            SWUD.SetDesktopIconVisibility(State);
        }

        private void InputDesktopChecked(bool State)
        {
            SMMI.EngineSettingManager.SetSetting(SMC.InputDesktop, State);
        }

        private SymbolRegular InputSymbol(SEIT Type)
        {
            return Type switch
            {
                SEIT.Close => SymbolRegular.HandDraw24,
                SEIT.OnlyMouse => SymbolRegular.DesktopCursor24,
                SEIT.OnlyKeyboard => SymbolRegular.DesktopKeyboard24,
                SEIT.MouseKeyboard => SymbolRegular.KeyboardMouse16,
                _ => SymbolRegular.Desktop24,
            };
        }

        private void ShuffleStateChecked(bool State)
        {
            SMMI.EngineSettingManager.SetSetting(SMC.Shuffle, State);
        }

        private void GifEngineSelected(ComboBoxItem Item)
        {
            if (Enum.TryParse($"{Item.Content}", out SSDEGET Type) && (SSDEET)Type != SSLMM.GApp)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.GApp, Type);
            }
        }

        private void UrlEngineSelected(ComboBoxItem Item)
        {
            if (Enum.TryParse($"{Item.Content}", out SSDEUET Type) && (SSDEET)Type != SSLMM.UApp)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.UApp, Type);
            }
        }

        private void WebEngineSelected(ComboBoxItem Item)
        {
            if (Enum.TryParse($"{Item.Content}", out SSDEWET Type) && (SSDEET)Type != SSLMM.WApp)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.WApp, Type);
            }
        }

        private void VideoEngineSelected(ComboBoxItem Item)
        {
            if (Enum.TryParse($"{Item.Content}", out SSDEVET Type) && (SSDEET)Type != SSLMM.VApp)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.VApp, Type);
            }
        }

        private void YouTubeEngineSelected(ComboBoxItem Item)
        {
            if (Enum.TryParse($"{Item.Content}", out SSDEYTET Type) && (SSDEET)Type != SSLMM.YApp)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.YApp, Type);
            }
        }

        private void InputTypeSelected(SPVCEC Input, int Index)
        {
            SEIT NewInput = (SEIT)Index;

            if (NewInput != SMMM.InputType)
            {
                Input.LeftIcon.Symbol = InputSymbol(NewInput);
                SMMI.EngineSettingManager.SetSetting(SMC.InputType, NewInput);
            }
        }

        private void ApplicationEngineSelected(ComboBoxItem Item)
        {
            if (Enum.TryParse($"{Item.Content}", out SSDEAET Type) && (SSDEET)Type != SSLMM.AApp)
            {
                SMMI.EngineSettingManager.SetSetting(SMC.AApp, Type);
            }
        }

        public void Dispose()
        {
            Contents.Clear();

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}