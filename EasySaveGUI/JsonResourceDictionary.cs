using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveGUI.Model;
using Newtonsoft.Json.Linq;

namespace EasySaveGUI
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
                // TOFIX : What's the logic here? (P) - spam pop up
                // MessageBox.Show("Chemin du fichier JSON invalide.");
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

        public class JsonResourceDictionary : ResourceDictionary
        {
            public JsonResourceDictionary(Uri uri)
            {
                LoadJson(uri);
            }

            private void LoadJson(Uri uri)
            {
                try
                {
                    if (!File.Exists(uri.LocalPath))
                    {
                        // TOFIX ; What's the logic here? (P) - spam pop up
                        //MessageBox.Show("Le fichier JSON spécifié n'existe pas.");
                        return;
                    }

                    var jsonString = File.ReadAllText(uri.LocalPath);
                    var jsonObject = JObject.Parse(jsonString);

                    foreach (var property in jsonObject)
                    {
                        this.Add(property.Key, property.Value.ToObject<object>());
                    }
                }
                catch (Exception ex)
                {
                    // Gérer les autres erreurs de chargement de fichier JSON
                    MessageBox.Show($"Erreur lors du chargement du fichier JSON : {ex.Message}");
                }
            }
        }
    }
}