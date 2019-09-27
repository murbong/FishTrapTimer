
using System;
using System.Diagnostics;

namespace FishTrapTimer.Core
{
    public static class GlobalManager
    {
        public static Process SelectedProcess { get; set; }
        public static DateTime StartTime { get; set; }
        public static TimeSpan SettingTime { get; set; }
        public static TimeSpan ElaspedTime { get; set; }
        public static uint Count { get; set; }

        public static RECT Offset;

        public static RECT ProcessBorder;
        public static RECT RealBorder => ProcessBorder + Offset;
    }
}
