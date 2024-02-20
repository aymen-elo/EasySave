using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EasySaveGUI.Command;
using Newtonsoft.Json.Linq;

namespace EasySaveGUI
{
    
    public class LangueSettingsViewModels 
    {
        public ICommand ChangeLanguageCommand { get; set; }

        public LangueSettingsViewModels()
        {
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
        }

        public void ChangeLanguage(string languageCode)
        {
            Uri resourceUri = GetResourceUri(languageCode);

            if (resourceUri == null)
            {
                MessageBox.Show("Chemin du fichier JSON invalide.");
                return;
            }

            ResourceDictionary dictionary = new JsonResourceDictionary(resourceUri);
            Application.Current.Resources.MergedDictionaries.Add(dictionary);       
        }

        private Uri GetResourceUri(string languageCode)
        {
            switch (languageCode)
            {
                case "en":
                    return new Uri("C:\\Users\\ruchu\\source\\repos\\EasySave\\EasySaveGUI\\lang\\StringsResources.en.json");
                case "fr":
                    return new Uri("C:\\Users\\ruchu\\source\\repos\\EasySave\\EasySaveGUI\\lang\\StringsResources.fr.json");
                default:
                    return new Uri("C:\\Users\\ruchu\\source\\repos\\EasySave\\EasySaveGUI\\lang\\StringsResources.en.json");
            }
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
                    MessageBox.Show("Le fichier JSON spécifié n'existe pas.");
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