﻿using System.Windows;
using SSEEH = Sucrose.Shared.Engine.Event.Handler;
using SSEWVEW = Sucrose.Shared.Engine.WebView.Event.Web;
using SSEWVMI = Sucrose.Shared.Engine.WebView.Manage.Internal;

namespace Sucrose.Shared.Engine.WebView.View
{
    /// <summary>
    /// Interaction logic for Web.xaml
    /// </summary>
    public sealed partial class Web : Window
    {
        public Web(string Web)
        {
            InitializeComponent();

            ContentRendered += (s, e) => SSEEH.ContentRendered(this);

            Content = SSEWVMI.WebEngine;

            SSEWVMI.Web = Web;

            SSEWVMI.WebEngine.CoreWebView2InitializationCompleted += SSEWVEW.WebEngineInitializationCompleted;

            Closing += (s, e) => SSEWVMI.WebEngine.Dispose();
            Loaded += (s, e) => SSEEH.WindowLoaded(this);
        }
    }
}