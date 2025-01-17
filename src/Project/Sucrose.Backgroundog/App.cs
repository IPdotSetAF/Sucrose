﻿using System.Globalization;
using System.Text;
using SBMI = Sucrose.Backgroundog.Manage.Internal;
using SHC = Skylark.Helper.Culture;
using SMMM = Sucrose.Manager.Manage.Manager;
using SMR = Sucrose.Memory.Readonly;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHS = Sucrose.Shared.Space.Helper.Security;
using SSWW = Sucrose.Shared.Watchdog.Watch;

namespace Sucrose.Backgroundog
{
    internal class App : IDisposable
    {
        public static async Task Main()
        {
            try
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;

                SHC.All = new CultureInfo(SMMM.Culture, true);

                if (SSSHI.Basic(SMR.BackgroundogMutex, SMR.Backgroundog))
                {
                    SSSHS.Apply();

                    SBMI.Initialize.Start();

                    do
                    {
                        SBMI.Initialize.Dispose();

                        await Task.Delay(SBMI.AppTime);
                    } while (SBMI.Exit);

                    SBMI.Initialize.Stop();
                }
            }
            catch (Exception Exception)
            {
                await SSWW.Watch_CatchException(Exception);
            }
            finally
            {
                Close();
            }
        }

        public static void Close()
        {
            Environment.Exit(0);
            Application.Exit();
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}