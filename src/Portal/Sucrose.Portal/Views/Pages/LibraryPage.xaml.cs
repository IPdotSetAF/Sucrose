﻿using System.IO;
using System.Windows;
using Wpf.Ui.Controls;
using SHG = Skylark.Helper.Generator;
using SMC = Sucrose.Memory.Constant;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMM = Sucrose.Manager.Manage.Manager;
using SMR = Sucrose.Memory.Readonly;
using SPMI = Sucrose.Portal.Manage.Internal;
using SPMM = Sucrose.Portal.Manage.Manager;
using SPVCTI = Sucrose.Portal.Views.Controls.ThemeImport;
using SPVMPLVM = Sucrose.Portal.ViewModels.Pages.LibraryViewModel;
using SPVPLELP = Sucrose.Portal.Views.Pages.Library.EmptyLibraryPage;
using SPVPLFLP = Sucrose.Portal.Views.Pages.Library.FullLibraryPage;
using SRER = Sucrose.Resources.Extension.Resources;
using SSDECT = Sucrose.Shared.Dependency.Enum.CompatibilityType;
using SSDESKT = Sucrose.Shared.Dependency.Enum.SortKindType;
using SSDESMT = Sucrose.Shared.Dependency.Enum.SortModeType;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSZEZ = Sucrose.Shared.Zip.Extension.Zip;
using SSZHA = Sucrose.Shared.Zip.Helper.Archive;

namespace Sucrose.Portal.Views.Pages
{
    /// <summary>
    /// LibraryPage.xaml etkileşim mantığı
    /// </summary>
    public partial class LibraryPage : INavigableView<SPVMPLVM>, IDisposable
    {
        private SPVPLELP EmptyLibraryPage { get; set; }

        private SPVPLFLP FullLibraryPage { get; set; }

        public SPVMPLVM ViewModel { get; }

        public LibraryPage(SPVMPLVM ViewModel)
        {
            this.ViewModel = ViewModel;
            DataContext = this;

            InitializeThemes();

            InitializeComponent();

            Library();

            Search();
        }

        private void Search()
        {
            string Search = SPMI.SearchService.SearchText;

            SPMI.SearchService.Dispose();

            SPMI.SearchService = new()
            {
                SearchText = Search
            };

            SPMI.SearchService.SearchTextChanged += SearchService_SearchTextChanged;
        }

        private void Library()
        {
            SPMI.LibraryService.Dispose();

            SPMI.LibraryService = new();

            SPMI.LibraryService.CreatedWallpaper += LibraryService_CreatedWallpaper;
        }

        private void InitializeThemes()
        {
            SPMI.Themes = SMMM.Themes;

            if (Directory.Exists(SMMM.LibraryLocation))
            {
                if (SPMI.Themes.Any())
                {
                    foreach (string Theme in SPMI.Themes.ToList())
                    {
                        string ThemePath = Path.Combine(SMMM.LibraryLocation, Theme);
                        string InfoPath = Path.Combine(ThemePath, SMR.SucroseInfo);

                        if (!Directory.Exists(ThemePath) || !File.Exists(InfoPath) || !SSTHI.CheckJson(SSTHI.ReadInfo(InfoPath)))
                        {
                            SPMI.Themes.Remove(Theme);

                            if (Directory.Exists(ThemePath) && SMMM.LibraryDelete)
                            {
                                Directory.Delete(ThemePath, true);
                            }
                        }
                    }
                }

                string[] Folders = Directory.GetDirectories(SMMM.LibraryLocation);

                if (Folders.Any())
                {
                    foreach (string Folder in Folders)
                    {
                        string InfoPath = Path.Combine(Folder, SMR.SucroseInfo);

                        if (File.Exists(InfoPath) && SSTHI.CheckJson(SSTHI.ReadInfo(InfoPath)))
                        {
                            if (!SPMI.Themes.Contains(Path.GetFileName(Folder)))
                            {
                                SPMI.Themes.Add(Path.GetFileName(Folder));
                            }
                        }
                        else if (SMMM.LibraryDelete)
                        {
                            Directory.Delete(Folder, true);
                        }
                    }
                }
            }
            else
            {
                SPMI.Themes.Clear();
            }

            SMMI.ThemesManager.SetSetting(SMC.Themes, SPMI.Themes);

            if (SPMM.LibrarySortMode == SSDESMT.None)
            {
                if (SPMM.LibrarySortKind == SSDESKT.Descending)
                {
                    SPMI.Themes.Reverse();
                }
            }
            else
            {
                Dictionary<string, object> Themes = new();

                foreach (string Theme in SPMI.Themes)
                {
                    string InfoPath = Path.Combine(SMMM.LibraryLocation, Theme, SMR.SucroseInfo);

                    if (SPMM.LibrarySortMode == SSDESMT.Name)
                    {
                        Themes.Add(Theme, SSTHI.ReadJson(InfoPath).Title);
                    }
                    else if (SPMM.LibrarySortMode == SSDESMT.Creation)
                    {
                        Themes.Add(Theme, Directory.GetCreationTime(Path.Combine(SMMM.LibraryLocation, Theme)));
                    }
                    else if (SPMM.LibrarySortMode == SSDESMT.Modification)
                    {
                        Themes.Add(Theme, Directory.GetLastWriteTime(InfoPath));
                    }
                }

                if (SPMM.LibrarySortKind == SSDESKT.Ascending)
                {
                    Themes = Themes.OrderBy(Theme => Theme.Value).ToDictionary(Theme => Theme.Key, Theme => Theme.Value);
                }
                else
                {
                    Themes = Themes.OrderByDescending(Theme => Theme.Value).ToDictionary(Theme => Theme.Key, Theme => Theme.Value);
                }

                SPMI.Themes.Clear();
                SPMI.Themes.AddRange(Themes.Select(Theme => Theme.Key));
            }
        }

        private async Task Start(bool Progress = false)
        {
            if (SPMI.Themes.Any())
            {
                FullLibraryPage = new(SPMI.Themes);

                FrameLibrary.Content = FullLibraryPage;
            }
            else
            {
                EmptyLibraryPage = new();

                FrameLibrary.Content = EmptyLibraryPage;
            }

            if (!Progress)
            {
                await Task.Delay(500);

                FrameLibrary.Visibility = Visibility.Visible;
                ProgressLibrary.Visibility = Visibility.Collapsed;
            }
        }

        private void GridLibrary_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) || e.AllowedEffects.HasFlag(DragDropEffects.Copy) == false)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
                DropRectangle.Visibility = Visibility.Visible;
            }
        }

        private void GridLibrary_DragLeave(object sender, DragEventArgs e)
        {
            DropRectangle.Visibility = Visibility.Collapsed;
        }

        private async void GridLibrary_Drop(object sender, DragEventArgs e)
        {
            DropRectangle.Visibility = Visibility.Collapsed;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool State = false;
                List<SSDECT> Types = new();
                List<string> Messages = new();

                string[] Files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (Files.Any())
                {
                    foreach (string Record in Files)
                    {
                        SSDECT Result = SSZHA.Check(Record);

                        if (Result == SSDECT.Pass)
                        {
                            if (!Directory.Exists(SMMM.LibraryLocation))
                            {
                                Directory.CreateDirectory(SMMM.LibraryLocation);
                            }

                            string Name;

                            do
                            {
                                Name = SHG.GenerateString(SMMM.Chars, 25, SMR.Randomise);
                            } while (File.Exists(Path.Combine(SMMM.LibraryLocation, Name)));

                            Result = await Task.Run(() => SSZEZ.Extract(Record, Path.Combine(SMMM.LibraryLocation, Name)));

                            if (Result == SSDECT.Pass)
                            {
                                State = true;
                                Messages.Add(string.Format(SRER.GetValue("Portal", "LibraryPage", "Import", "Successful"), Path.GetFileNameWithoutExtension(Record)));
                            }
                            else
                            {
                                Messages.Add(string.Format(SRER.GetValue("Portal", "LibraryPage", "Import", "Unsuccessful"), Path.GetFileNameWithoutExtension(Record), Result));
                            }
                        }
                        else
                        {
                            if (!Types.Contains(Result))
                            {
                                Types.Add(Result);
                            }

                            Messages.Add(string.Format(SRER.GetValue("Portal", "LibraryPage", "Import", "Unsuccessful"), Path.GetFileNameWithoutExtension(Record), Result));
                        }
                    }
                }

                if (Messages.Any() || Types.Any())
                {
                    SPVCTI ThemeImport = new()
                    {
                        Types = Types,
                        Messages = Messages
                    };

                    await ThemeImport.ShowAsync();

                    ThemeImport.Dispose();
                }

                if (State)
                {
                    Dispose();

                    InitializeThemes();

                    await Start(true);
                }
            }
        }

        private async void GridLibrary_Loaded(object sender, RoutedEventArgs e)
        {
            await Start();
        }

        private async void SearchService_SearchTextChanged(object sender, EventArgs e)
        {
            Dispose();

            await Start(true);
        }

        private async void LibraryService_CreatedWallpaper(object sender, EventArgs e)
        {
            Dispose();

            InitializeThemes();

            await Start(true);
        }

        public void Dispose()
        {
            FullLibraryPage?.Dispose();
            EmptyLibraryPage?.Dispose();

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}