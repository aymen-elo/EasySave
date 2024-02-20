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
                }
                
            }
        }
        public static void SaveLanguage(string languageCode)
        {
            Directory.CreateDirectory(LogsDirectoryPath); // Assurez-vous que le répertoire existe
            File.WriteAllText(ConfigFilePath, languageCode);
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