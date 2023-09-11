﻿using System.Media;
using System.Windows;
using SMR = Sucrose.Memory.Readonly;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommandsType;
using SSSHD = Sucrose.Shared.Space.Helper.Dark;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using SWHWI = Skylark.Wing.Helper.WindowInterop;

namespace Sucrose.Shared.Watchdog
{
    /// <summary>
    /// Interaction logic for DarkErrorMessageBox.xaml
    /// </summary>
    public partial class DarkErrorMessageBox : Window
    {
        private static string Path = string.Empty;
        private static string Text = string.Empty;
        private static string Address = string.Empty;

        public DarkErrorMessageBox(string ErrorMessage, string LogPath, string HelpAddress = null, string HelpText = null)
        {
            InitializeComponent();

            Path = LogPath;
            Text = HelpText;
            Address = HelpAddress;

            SystemSounds.Hand.Play();

            Help_Button.Content = Text ?? Help_Button.Content;

            Error_Message.Text += Environment.NewLine + ErrorMessage;

            SourceInitialized += DarkErrorMessageBox_SourceInitialized;
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            SSSHP.Run(SSSMI.Commandog, $"{SMR.StartCommand}{SSDECT.Log}{SMR.ValueSeparator}{Path}");
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Address))
            {
                SSSHP.Run(SSSMI.Commandog, $"{SMR.StartCommand}{SSDECT.Wiki}{SMR.ValueSeparator}{SMR.WikiWebsite}");
            }
            else
            {
                SSSHP.Run(SSSMI.Commandog, $"{SMR.StartCommand}{SSDECT.Wiki}{SMR.ValueSeparator}{Address}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DarkErrorMessageBox_SourceInitialized(object sender, EventArgs e)
        {
            SSSHD.Apply(SWHWI.Handle(this));
        }
    }
}