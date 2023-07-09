﻿using SEWVMI = Sucrose.Engine.WV.Manage.Internal;
using SSEST = Sucrose.Space.Enum.StretchType;

namespace Sucrose.Engine.WV.Helper
{
    internal static class Video
    {
        public static async void Pause()
        {
            await SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync("document.getElementsByTagName('video')[0].pause();");
        }

        public static async void Play()
        {
            await SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync("document.getElementsByTagName('video')[0].play();");
        }

        public static async Task<string> GetVideo()
        {
            return await SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].src");
        }

        public static async Task<bool> GetEnd()
        {
            string Duration = await SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].duration");
            string Current = await SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].currentTime");

            return Current.Equals(Duration);
        }

        public static void SetVideo(string Video)
        {
            SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].src = '{Video}';");
        }

        public static async void SetLoop(bool State)
        {
            await SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].loop = {State.ToString().ToLower()};");

            if (State)
            {
                bool Ended = await GetEnd();

                if (Ended)
                {
                    Play();
                }
            }
        }

        public static async void SetVolume(int Volume)
        {
            await SEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].volume = {(Volume / 100d).ToString().Replace(" ", ".").Replace(",", ".")};");
        }

        public static async void SetStretch(SSEST Stretch)
        {
            switch (Stretch)
            {
                case SSEST.None:
                    await SEWVMI.WebEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"none\";");
                    break;
                case SSEST.Fill:
                    await SEWVMI.WebEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"fill\";");
                    break;
                case SSEST.Uniform:
                    await SEWVMI.WebEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"contain\";");
                    break;
                case SSEST.UniformToFill:
                    await SEWVMI.WebEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"cover\";");
                    break;
                default:
                    break;
            }
        }
    }
}