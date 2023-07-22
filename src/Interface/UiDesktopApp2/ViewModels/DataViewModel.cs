﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using UiDesktopApp2.Models;
using Wpf.Ui.Common.Interfaces;

namespace UiDesktopApp2.ViewModels
{
    public partial class DataViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private IEnumerable<DataColor> _colors;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        public void OnNavigatedFrom()
        {
        }

        private void InitializeViewModel()
        {
            Random random = new();
            List<DataColor> colorCollection = new();

            for (int i = 0; i < 8192; i++)
            {
                colorCollection.Add(new DataColor
                {
                    Color = new SolidColorBrush(Color.FromArgb(
                        (byte)200,
                        (byte)random.Next(0, 250),
                        (byte)random.Next(0, 250),
                        (byte)random.Next(0, 250)))
                });
            }

            Colors = colorCollection;

            _isInitialized = true;
        }
    }
}