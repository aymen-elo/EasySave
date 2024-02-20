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
            if (!File.Exists(ConfigFilePath))
            {
                using (StreamWriter sw = File.CreateText(ConfigFilePath))
                {
                    sw.WriteLine("language:en");
                    sw.WriteLine("logformat:json");
                }
                
            }
        }
        public static void SaveLanguage(string languageCode)
        {
            if (!Directory.Exists(LogsDirectoryPath))
            {
                Directory.CreateDirectory(LogsDirectoryPath);   
            }

            if (!File.Exists(LogsDirectoryPath + @"\config.conf"))
            {
                ///   TODO
            }
            
            string[] lines = File.ReadAllLines(ConfigFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("logformat"))
                {
                    lines[i] = $"logformat:{logFormat}";
                }
            }
            File.WriteAllLines(ConfigFilePath, lines);
            
            File.WriteAllText(ConfigFilePath, languageCode);
        }
        
        public static void SaveLogFormat(string logFormat)
        {
            Directory.CreateDirectory(LogsDirectoryPath); 
    
            if (File.Exists(ConfigFilePath))
            {
                string[] lines = File.ReadAllLines(ConfigFilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("logformat"))
                    {
                        lines[i] = $"logformat:{logFormat}";
                    }
                }
                File.WriteAllLines(ConfigFilePath, lines);
            }
        }

        public static string GetSavedLanguage()
        {
            if (File.Exists(ConfigFilePath))
            {
                return File.ReadAllText(ConfigFilePath);
            }
            return null; // Aucune langue enregistrée
        }
    }
}