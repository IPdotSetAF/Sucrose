﻿using System.Diagnostics;
using System.Windows;
using SEDST = Skylark.Enum.DisplayScreenType;
using SSEHD = Sucrose.Shared.Engine.Helper.Data;
using SWE = Skylark.Wing.Engine;
using SWHPI = Skylark.Wing.Helper.ProcessInterop;
using SWHWI = Skylark.Wing.Helper.WindowInterop;
using SWHWO = Skylark.Wing.Helper.WindowOperations;
using SWNM = Skylark.Wing.Native.Methods;
using SWUS = Skylark.Wing.Utility.Screene;

namespace Sucrose.Shared.Engine.Event
{
    internal static class Handler
    {
        public static void WindowLoaded(Window Window)
        {
            IntPtr Handle = SWHWI.Handle(Window);

            //ShowInTaskbar = false : causing issue with Windows10-Windows11 Taskview.
            SWHWO.RemoveWindowFromTaskbar(Handle);

            //this hides the window from taskbar and also fixes crash when Win10-Win11 taskview is launched. 
            Window.ShowInTaskbar = true;
            Window.ShowInTaskbar = false;
        }

        public static void ContentRendered(Window Window)
        {
            switch (SSEHD.GetDisplayScreenType())
            {
                case SEDST.SpanAcross:
                    SWE.WallpaperWindow(Window, SSEHD.GetExpandScreenType(), SSEHD.GetScreenType());
                    break;
                case SEDST.SameDuplicate:
                    SWE.WallpaperWindow(Window, SSEHD.GetDuplicateScreenType(), SSEHD.GetScreenType());
                    break;
                default:
                    SWE.WallpaperWindow(Window, SSEHD.GetScreenIndex(), SSEHD.GetScreenType());
                    break;
            }
        }

        public static void ApplicationLoaded(Process Process)
        {
            IntPtr Handle = SWHPI.MainWindowHandle(Process);

            //ShowInTaskbar = false : causing issue with Windows10-Windows11 Taskview.
            SWHWO.RemoveWindowFromTaskbar(Handle);

            SWNM.ShowWindow(Handle, (int)SWNM.SHOWWINDOW.SW_HIDE);

            int currentStyle = SWNM.GetWindowLong(Handle, (int)SWNM.GWL.GWL_STYLE);
            SWNM.SetWindowLong(Handle, (int)SWNM.GWL.GWL_STYLE, currentStyle & ~((int)SWNM.WindowStyles.WS_CAPTION | (int)SWNM.WindowStyles.WS_THICKFRAME | (int)SWNM.WindowStyles.WS_MINIMIZE | (int)SWNM.WindowStyles.WS_MAXIMIZE | (int)SWNM.WindowStyles.WS_SYSMENU | (int)SWNM.WindowStyles.WS_DLGFRAME | (int)SWNM.WindowStyles.WS_BORDER | (int)SWNM.WindowStyles.WS_EX_CLIENTEDGE));

            SWHWO.BorderlessWinStyle(Handle);
        }

        public static void ApplicationRendered(Process Process)
        {
            switch (SSEHD.GetDisplayScreenType())
            {
                case SEDST.SpanAcross:
                    SWE.WallpaperProcess(Process, SSEHD.GetExpandScreenType(), SSEHD.GetScreenType());
                    break;
                case SEDST.SameDuplicate:
                    //SWE.WallpaperProcess(Process, SSEHD.GetDuplicateScreenType(), SSEHD.GetScreenType());
                    break;
                default:
                    SWE.WallpaperProcess(Process, SSEHD.GetScreenIndex(), SSEHD.GetScreenType());
                    break;
            }

            SWNM.ShowWindow(SWHPI.MainWindowHandle(Process), (int)SWNM.SHOWWINDOW.SW_SHOW);
        }

        public static async void DisplaySettingsChanged(Window Window)
        {
            Window.Hide();

            await Task.Delay(2000);

            SWUS.Initialize();

            await Task.Delay(500);

            ContentRendered(Window);

            Window.Show();
        }

        public static async void DisplaySettingsChanged(Process Process, IntPtr Handle)
        {
            SWNM.ShowWindow(Handle, (int)SWNM.SHOWWINDOW.SW_HIDE);

            await Task.Delay(2000);

            SWUS.Initialize();

            await Task.Delay(500);

            ApplicationRendered(Process);

            SWNM.ShowWindow(Handle, (int)SWNM.SHOWWINDOW.SW_SHOW);
        }
    }
}