using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Offsetter.Solver
{
    public class GSettings
    {
        public GSettings() { }

        public bool DumpContactCandidatesInternal { get; set; } = false;
        public bool DumpContactCandidatesClient { get; set; } = false;
        public bool DumpPositionAtClient { get; set; } = false;
        public bool DumpChChains { get; set; } = false;
        public bool DumpPath { get; set; } = false;
        public bool DumpRpts { get; set; } = false;
    }

    public class GConfig
    {
        public static GSettings Values = new GSettings();

        public static void ConfigLoad()
        {
            string path = ConfigPath();
            if (!File.Exists(path))
                ConfigSave();  // Create a default config file.

            string contents = File.ReadAllText(path);
            Values = JsonConvert.DeserializeObject<GSettings>(contents)!;
        }

        private static bool ConfigSave()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            string path = ConfigPath();
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(JsonConvert.SerializeObject(Values, Formatting.Indented));
            }

            return true;
        }

        private static string ConfigPath()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            int indx = path.IndexOf(@"\bin\");
            Debug.Assert((indx > 0), "ConfigPath()");
            if (indx <= 0)
                return string.Empty;

            path = path.Substring(0, indx);
            indx = path.LastIndexOf(@"\");
            Debug.Assert((indx > 0), "ConfigPath()");
            if (indx <= 0)
                return string.Empty;

            return path.Substring(0, indx) + @"\data\config.json";
        }
    }
}
