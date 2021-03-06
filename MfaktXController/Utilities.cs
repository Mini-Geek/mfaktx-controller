﻿using Microsoft.WindowsAPICodePack.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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

        public static string IniFile(string identifier)
        {
            string keyName = string.Format("MfaktX{0}IniFileName", identifier);
            return ConfigurationManager.AppSettings[keyName];
        }

        public static string MfaktXArguments { get { return ConfigurationManager.AppSettings["MfaktXArguments"]; } }

        public static string ExeFile { get { return ConfigurationManager.AppSettings["MfaktXExeFileName"]; } }

        public static string SendCtrlCode { get { return ConfigurationManager.AppSettings["SendCtrlCode"]; } }

        public static int Timeout { get { return int.Parse(ConfigurationManager.AppSettings["Timeout"]); } }

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

        public static double IdleDelayInterval { get { return double.Parse(ConfigurationManager.AppSettings["IdleDelayInterval"]); } }

        public static double IdleDetectionInterval { get { return 500; } }

        public static double PauseSlowDetectionInterval { get { return double.Parse(ConfigurationManager.AppSettings["PauseSlowDetectionInterval"]); } }

        public static Speed? StartupSpeed
        {
            get
            {
                Speed result;
                if (Enum.TryParse<Speed>(ConfigurationManager.AppSettings["StartupSpeed"], true, out result))
                    switch (result)
                    {
                        case Speed.Slow:
                        case Speed.Medium:
                        case Speed.Fast:
                            return result;
                    }

                return null;
            }
        }

        public static FontFamily OutputLogFontFamily { get { return new FontFamily(ConfigurationManager.AppSettings["OutputLogFontFamily"]); } }

        public static string InstanceIdentifier { get { return ConfigurationManager.AppSettings["InstanceIdentifier"]; } }

        public static IEnumerable<string> PauseWhileRunning
        {
            get
            {
                var rawList = ConfigurationManager.AppSettings["PauseWhileRunning"];
                if (string.IsNullOrEmpty(rawList))
                    return Enumerable.Empty<string>();
                else
                {
                    var nameList = rawList.Split(',').Select(x => Path.GetFileNameWithoutExtension(x));
                    return nameList;
                }
            }
        }

        public static IEnumerable<string> SlowWhileRunning
        {
            get
            {
                var rawList = ConfigurationManager.AppSettings["SlowWhileRunning"];
                if (string.IsNullOrEmpty(rawList))
                    return Enumerable.Empty<string>();
                else
                {
                    var nameList = rawList.Split(',').Select(x => Path.GetFileNameWithoutExtension(x));
                    return nameList;
                }
            }
        }

        public static IDictionary<string, Speed> PauseOrSlowWhileRunning
        {
            get
            {
                var dict = PauseWhileRunning.ToDictionary(x => x, x => Speed.Stopped);
                foreach (var value in SlowWhileRunning)
                {
                    if (!dict.ContainsKey(value))
                        dict.Add(value, Speed.Slow);
                }
                return dict;
            }
        }

        public static JoinComparerProvider<T, TKey> WithComparer<T, TKey>(this IEnumerable<T> inner, IEqualityComparer<TKey> comparer)
        {
            return new JoinComparerProvider<T, TKey>(inner, comparer);
        }

        public sealed class JoinComparerProvider<T, TKey>
        {
            internal JoinComparerProvider(IEnumerable<T> inner, IEqualityComparer<TKey> comparer)
            {
                Inner = inner;
                Comparer = comparer;
            }

            public IEqualityComparer<TKey> Comparer { get; private set; }
            public IEnumerable<T> Inner { get; private set; }
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            JoinComparerProvider<TInner, TKey> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join(inner.Inner, outerKeySelector, innerKeySelector,
                              resultSelector, inner.Comparer);
        }

        public static DispatcherOperation BeginInvoke(this Dispatcher dispatcher, Action action)
        {
            return dispatcher.BeginInvoke((Delegate)action);
        }

        public static string WithBeginningSpace(this string value)
        {
            return value.WithBeginning(" ");
        }

        public static string WithBeginning(this string value, string prefix)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return prefix + value;
        }

        public static string WithEndingSpace(this string value)
        {
            return value.WithEnding(" ");
        }

        public static string WithEnding(this string value, string suffix)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value + suffix;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(
           int uAction, int uParam, ref bool lpvParam,
           int flags);

        private const int SPI_GETSCREENSAVERRUNNING = 114;

        /// <summary>
        /// Returns true if the screen saver is running or the monitor is off due to power settings
        /// </summary>
        private static bool ScreenInactive
        {
            get
            {
                return ScreenSaverRunning || !PowerManager.IsMonitorOn;
            }
        }

        /// <summary>
        /// Returns TRUE if the screen saver is actually running
        /// </summary>
        private static bool ScreenSaverRunning
        {
            get
            {
                bool isRunning = false;
                SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isRunning, 0);
                return isRunning;
            }
        }

        /// <summary>
        /// Returns true after a delay if the screen saver is running or the monitor is off due to power settings
        /// </summary>
        public static bool IsScreenInactiveAfterDelay()
        {
            if (Utilities.IdleDelayInterval <= 0)
                return ScreenInactive;
            if (!ScreenInactive)
            {
                currentInactiveStartTime = null;
                return false;
            }
            if (currentInactiveStartTime == null)
            {
                currentInactiveStartTime = DateTime.Now;
                return false;
            }
            return (DateTime.Now - currentInactiveStartTime.Value).TotalMilliseconds > Utilities.IdleDelayInterval;
        }

        private static DateTime? currentInactiveStartTime;
    }
}
