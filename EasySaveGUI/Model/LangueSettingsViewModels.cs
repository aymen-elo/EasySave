using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveGUI.Model;

namespace EasySaveGUI.Model
{
    public class LangueSettingsViewModels
    { 
        public ICommand ChangeLanguageCommand { get; set; }
        
                public LangueSettingsViewModels()
                {
                    InitializeLanguage(); 
                    ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
                }
        
                private void InitializeLanguage()
                {
                    string savedLanguage = ConfigManager.GetSavedLanguage();
                    if (!string.IsNullOrEmpty(savedLanguage))
                    {
                        ChangeLanguage(savedLanguage);
                    }
                }
        
                public void ChangeLanguage(string languageCode)
                {
                    Uri resourceUri = GetResourceUri(languageCode);
        
                    if (resourceUri == null)
                    {
                        return;
                    }
        
                    ResourceDictionary dictionary = new JsonResourceDictionary(resourceUri);
                    Application.Current.Resources.MergedDictionaries.Add(dictionary);
                    ConfigManager.SaveLanguage(languageCode);
        
                }
        
                private Uri GetResourceUri(string languageCode)
                {
                    string
                        baseDirectory =
                            Environment.CurrentDirectory;
                    string langFolderPath = Path.Combine(baseDirectory, "..", "..", "..", "lang");
        
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
  