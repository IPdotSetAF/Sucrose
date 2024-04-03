﻿using SEDST = Skylark.Enum.DuplicateScreenType;
using SEEST = Skylark.Enum.ExpandScreenType;
using SEST = Skylark.Enum.ScreenType;
using SMC = Sucrose.Memory.Constant;
using SMMI = Sucrose.Manager.Manage.Internal;
using SSDEDT = Sucrose.Shared.Dependency.Enum.DisplayType;
using SSDEST = Sucrose.Shared.Dependency.Enum.StretchType;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommunicationType;

namespace Sucrose.Shared.Engine.Manage
{
    internal static class Manager
    {
        public static SSDECT CommunicationType => SMMI.BackgroundogSettingManager.GetSetting(SMC.CommunicationType, SSDECT.Signal);

        public static SEDST DuplicateScreenType => SMMI.EngineSettingManager.GetSetting(SMC.DuplicateScreenType, SEDST.Default);

        public static SEEST ExpandScreenType => SMMI.EngineSettingManager.GetSetting(SMC.ExpandScreenType, SEEST.Default);

        public static SEST ScreenType => SMMI.EngineSettingManager.GetSetting(SMC.ScreenType, SEST.DisplayBound);

        public static SSDEDT DisplayType => SMMI.EngineSettingManager.GetSetting(SMC.DisplayType, SSDEDT.Screen);

        public static SSDEST StretchType => SMMI.EngineSettingManager.GetSetting(SMC.StretchType, SSDEST.Fill);

        public static SSDEST DefaultStretchType => SSDEST.None;
    }
}