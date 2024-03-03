using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveGUI.Helper;
using EasySaveLib.Model;

namespace EasySaveGUI.ViewModel
{
    /// <summary>
    /// ViewModel for handling language settings.
    /// </summary>
    public class LanguageSettingsViewModel
    {
        /// <summary>
        /// Command to change the language.
        /// </summary>
        public ICommand ChangeLanguageCommand { get; set; }

        /// <summary>
        /// Constructor for LanguageSettingsViewModel.
        /// Initializes the language settings and sets up the command to change the language.
        /// </summary>
        public LanguageSettingsViewModel()
        {
            InitializeLanguage();
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
        }

        /// <summary>
        /// Initializes the language settings by retrieving the saved language from the configuration manager.
        /// If a saved language exists, it changes the language to the saved language.
        /// </summary>
        private void InitializeLanguage()
        {
            string? savedLanguage = ConfigManager.GetSavedLanguage();
            if (!string.IsNullOrEmpty(savedLanguage))
            {
                ChangeLanguage(savedLanguage);
            }
        }

        /// <summary>
        /// Changes the language of the application.
        /// </summary>
        /// <param name="obj">The language code to change to.</param>
        public void ChangeLanguage(object obj)
        {
            var languageCode = (string)obj;
            Uri resourceUri = GetResourceUri(languageCode);

            ResourceDictionary dictionary = new JsonResourceDictionary(resourceUri);
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
            ConfigManager.SaveLanguage(languageCode);
        }

        /// <summary>
        /// Gets the resource URI for the specified language code.
        /// </summary>
        /// <param name="languageCode">The language code to get the resource URI for.</param>
        /// <returns>The resource URI for the specified language code.</returns>
        private Uri GetResourceUri(string languageCode)
        {
            string
                baseDirectory =
                    Environment.CurrentDirectory;
            string langFolderPath = Path.Combine(baseDirectory, ("..\\..\\..\\Assets"));

            switch (languageCode)
            {
                case "en":
                    return new Uri(Path.Combine(langFolderPath, "StringsResources.en.json"));
                case "fr":
                    return new Uri(Path.Combine(langFolderPath, "StringsResources.fr.json"));
                default:
                    return new Uri(Path.Combine(langFolderPath, "StringsResources.en.json"));
            }
        }
    }
}