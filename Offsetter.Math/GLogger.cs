using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Offsetter.Math
{
    public class GLogger
    {
        public static bool Active { get; set; } = false;

        public static void Clear() { log.Clear(); }

        public static void Log(string statement)
        {
            if (Active)
                log.Add(statement);
        }

        public static void LogFileCreate(string logPath)
        {
            if (!Active)
                return;

            if (File.Exists(logPath))
                File.Delete(logPath);

            using (StreamWriter fs = new StreamWriter(logPath))
            {
                foreach (string str in log)
                { fs.WriteLine(str); }
            }
        }

        public static void Assert(bool condition, string message)
        {
            if (condition)
                return;

            if (Active)
            {
                Log(message);
                throw new Exception(message);
            }
            else
            {
                Debug.Assert(false, message);
            }
        }

        private static List<string> log = new List<string>();
    }
}
