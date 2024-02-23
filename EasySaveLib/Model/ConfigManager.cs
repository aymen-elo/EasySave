using System.IO;

namespace EasySaveLib.Model
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
                sw.WriteLine("encryptionkey:");
                sw.WriteLine("cipherlist:");
                sw.WriteLine("prioritylist:");
            }
        }
        
        /* POSSIBLE REFACTOR FOR THE 3 METHODS */
        
        /* ***** SETTER ***** */

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
        
        public static void SaveEncryptionKey(string key)
        {
            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("encryptionkey")) { i++; }
            lines[i] = $"encryptionkey:{key}";
            
            File.WriteAllLines(ConfigFilePath, lines);
        }
        
        public static void SaveCipherList(string cipherList)
        {
            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("cipherlist")) { i++; }
            lines[i] = $"cipherlist:{cipherList}";
            
            File.WriteAllLines(ConfigFilePath, lines);
        }
        
        public static void SavePriorityList(string priorityList)
        {
            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("prioritylist")) { i++; }
            lines[i] = $"prioritylist:{priorityList}";
            
            File.WriteAllLines(ConfigFilePath, lines);
        }
        
        /* ***** GETTER ***** */

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
        
        public static string? GetEncryptionKey()
        {
            if (!File.Exists(ConfigFilePath)) { return null; }

            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("encryptionkey")) { i++; }
            var key = lines[i].Split(":")[1];

            return key;
        }
        
        public static string? GetCipherList()
        {
            if (!File.Exists(ConfigFilePath)) { return null; }

            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("cipherlist")) { i++; }
            var cipherList = lines[i].Split(":")[1];

            return cipherList;
        }
        
        public static string? GetPriorityList()
        {
            if (!File.Exists(ConfigFilePath)) { return null; }

            var lines = File.ReadAllLines(ConfigFilePath);
            
            var i = 0;
            while (i < lines.Length && !lines[i].Contains("prioritylist")) { i++; }
            var priorityList = lines[i].Split(":")[1];

            return priorityList;
        }

        /****************************************/
    }
}