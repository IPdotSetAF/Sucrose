﻿using System.IO;
using SEWTT = Skylark.Enum.WindowsThemeType;
using SMC = Sucrose.Memory.Constant;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMR = Sucrose.Memory.Readonly;
using SSCEBT = Sucrose.Shared.Core.Enum.BundleType;
using SWHWT = Skylark.Wing.Helper.WindowsTheme;

namespace Sucrose.Update.Manage
{
    internal static class Manager
    {
        public static SSCEBT BundleType => SMMI.BundleSettingManager.GetSetting(SMC.BundleType, SSCEBT.Compressed);

        public static string CachePath => Path.Combine(SMR.AppDataPath, SMR.AppName, SMR.CacheFolder, SMR.Bundle);

        public static SEWTT Theme => SMMI.GeneralSettingManager.GetSetting(SMC.ThemeType, SWHWT.GetTheme());

        public static Mutex Mutex => new(true, SMR.UpdateMutex);
    }
}