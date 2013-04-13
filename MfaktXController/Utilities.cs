using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MfaktXController
{
    static class Utilities
    {
        public static int MaxInstances
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MaxInstances"];
                int parsedValue;
                if (int.TryParse(value, out parsedValue))
                    return parsedValue;
                else
                    return 1;
            }
        }

        public static int MaxLines
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MaxLines"];
                int parsedValue;
                if (int.TryParse(value, out parsedValue))
                    return parsedValue;
                else
                    return 300;
            }
        }

        public static string IniFile(Speed? speed)
        {
            string keyName = string.Format("MfaktX{0}IniFileName", speed.ToString());
            return ConfigurationManager.AppSettings[keyName];
        }

        public static string ExeFile { get { return ConfigurationManager.AppSettings["MfaktXExeFileName"]; } }

        public static bool EnableIdleDetection
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnableIdleDetection"]))
                    return true;
                else
                    try
                    {
                        return Convert.ToBoolean(ConfigurationManager.AppSettings["EnableIdleDetection"]);
                    }
                    catch
                    {
                        return true;
                    }
            }
        }

        public static DispatcherOperation BeginInvoke(this Dispatcher dispatcher, Action action)
        {
            return dispatcher.BeginInvoke((Delegate)action);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(
           int uAction, int uParam, ref bool lpvParam,
           int flags);

        private const int SPI_GETSCREENSAVERACTIVE = 16;
        private const int SPI_GETSCREENSAVERRUNNING = 114;

        /// <summary>
        /// Returns TRUE if the screen saver is active 
        /// (enabled, but not necessarily running).
        /// </summary>
        public static bool ScreenSaverActive
        {
            get
            {
                bool isActive = false;
                SystemParametersInfo(SPI_GETSCREENSAVERACTIVE, 0, ref isActive, 0);
                return isActive;
            }
        }

        /// <summary>
        /// Returns TRUE if the screen saver is actually running
        /// </summary>
        public static bool ScreenSaverRunning
        {
            get
            {
                bool isRunning = false;
                SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isRunning, 0);
                return isRunning;
            }
        }
    }
}
