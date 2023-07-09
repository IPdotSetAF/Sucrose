﻿using CefSharp;
using SECSMI = Sucrose.Engine.CS.Manage.Internal;
using SSEST = Sucrose.Space.Enum.StretchType;

namespace Sucrose.Engine.CS.Helper
{
    internal static class Video
    {
        public static void Pause()
        {
            SECSMI.CefEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].pause();");
        }

        public static void Play()
        {
            SECSMI.CefEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].play();");
        }

        public static async Task<bool> GetEnd()
        {
            JavascriptResponse Response;
            string Current = string.Empty;
            string Duration = string.Empty;

            if (SECSMI.CefEngine.CanExecuteJavascriptInMainFrame)
            {
                Response = await SECSMI.CefEngine.EvaluateScriptAsync($"document.getElementsByTagName('video')[0].duration");

                if (Response.Success)
                {
                    Duration = Response.Result.ToString();
                }

                Response = await SECSMI.CefEngine.EvaluateScriptAsync($"document.getElementsByTagName('video')[0].currentTime");

                if (Response.Success)
                {
                    Current = Response.Result.ToString();
                }
            }
            else
            {
                IFrame Frame = SECSMI.CefEngine.GetMainFrame();

                Response = await Frame.EvaluateScriptAsync($"document.getElementsByTagName('video')[0].duration");

                if (Response.Success)
                {
                    Duration = Response.Result.ToString();
                }

                Response = await Frame.EvaluateScriptAsync($"document.getElementsByTagName('video')[0].currentTime");

                if (Response.Success)
                {
                    Current = Response.Result.ToString();
                }
            }

            return Current.Equals(Duration);
        }

        public static async void SetLoop(bool State)
        {
            SECSMI.CefEngine.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].loop = {State.ToString().ToLower()};");

            if (State)
            {
                bool Ended = await GetEnd();

                if (Ended)
                {
                    Play();
                }
            }
        }

        public static void SetVolume(int Volume)
        {
            SECSMI.CefEngine.ExecuteScriptAsync($"document.getElementsByTagName('video')[0].volume = {(Volume / 100d).ToString().Replace(" ", ".").Replace(",", ".")};");
        }

        public static void SetStretch(SSEST Stretch)
        {
            switch (Stretch)
            {
                case SSEST.None:
                    SECSMI.CefEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"none\";");
                    break;
                case SSEST.Fill:
                    SECSMI.CefEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"fill\";");
                    break;
                case SSEST.Uniform:
                    SECSMI.CefEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"contain\";");
                    break;
                case SSEST.UniformToFill:
                    SECSMI.CefEngine.ExecuteScriptAsync("document.getElementsByTagName('video')[0].style.objectFit = \"cover\";");
                    break;
                default:
                    break;
            }
        }
    }
}