using System.IO;
namespace EasySaveGUI.Model
{
    public class ConfigManager
    {
        public static string LogsDirectoryPath = @"C:\Prosoft\EasySave\";
        private const string ConfigFileName = "config.conf";
        private static string ConfigFilePath => Path.Combine(LogsDirectoryPath, ConfigFileName);

        public static void SaveLanguage(string languageCode)
        {
            Directory.CreateDirectory(LogsDirectoryPath);
            string configContent = $"language:{languageCode}";
            File.WriteAllText(ConfigFilePath, configContent);
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