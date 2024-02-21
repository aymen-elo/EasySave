using System.IO;
using System.Net;

namespace EasySaveGUI.Model
{
    public class ConfigManager
    {
        public static string LogsDirectoryPath = @"C:\Prosoft\EasySave\";
        private const string ConfigFileName = "config.conf";
        private static string ConfigFilePath => Path.Combine(LogsDirectoryPath, ConfigFileName);

        public ConfigManager()
        {
            if (File.Exists(ConfigFilePath)) { return;}
            
            using (var sw = File.CreateText(ConfigFilePath))
            {
                sw.WriteLine("language:en");
                sw.WriteLine("logformat:json");
            }
        }
        
        /* POSSIBLE REFACTOR FOR THE 3 METHODS */
        public static void SaveLanguage(string languageCode)
        {
            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("language")) { i++; }
            lines[i] = $"language:{languageCode}";
            
            File.WriteAllLines(ConfigFilePath, lines);
        }
        
        public static void SaveLogFormat(string logFormat)
        {
            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("logformat")) { i++; }
            lines[i] = $"logformat:{logFormat}";
            
            File.WriteAllLines(ConfigFilePath, lines);
        }

        public static string? GetSavedLanguage()
        {
            if (!File.Exists(ConfigFilePath)) { return null; }

            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("language")) { i++; }
            var lang = lines[i].Split(":")[1];

            return lang;
        }
        
        public static string? GetLogFormat()
        {
            if (!File.Exists(ConfigFilePath)) { return null; }

            var lines = File.ReadAllLines(ConfigFilePath);

            var i = 0;
            while (i < lines.Length && !lines[i].Contains("logformat")) { i++; }
            var logFormat = lines[i].Split(":")[1];

            return logFormat;
        }

        /****************************************/
    }
}