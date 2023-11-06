
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogHelper
{
    public enum LogType
    {
        Message,
        Debug
    }
    public sealed class Logger
    {
        private static bool debugMode=false;
        public static bool Debug
        {

            get { return debugMode; }
            set { debugMode=value; }
        }
        public static void Log(string message, LogType logType=LogType.Message,bool debug = false)
        {
            if (logType == LogType.Debug && !debug) return;
            Console.WriteLine(message);
        }

        public static void DBGLog(string message)
        {
            Log(message, LogType.Debug,debugMode);
        }

        public static bool GetYNInput(string displayMessage)
        {
            Log(displayMessage+" (Y/N)", LogType.Message);
            return Console.ReadLine().ToLower() != "n";
        }
    }
}
