﻿using MediaEngine = Mpv.NET.Player.MpvPlayer;

namespace Sucrose.Shared.Engine.MpvPlayer.Manage
{
    internal static class Internal
    {
        public static string Source;

        public static MediaEngine MediaEngine;

#if X86
        public static readonly string MediaPath = @"lib\libmpv-86.dll";
#elif X64
        public static readonly string MediaPath = @"lib\libmpv-64.dll";
#else
        public static readonly string MediaPath = @"lib\libmpv-a64.dll";
#endif

    }
}