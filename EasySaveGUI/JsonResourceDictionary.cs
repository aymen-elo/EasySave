using System;
using System.Windows;
using System.Windows.Input;
using EasySave_2._0.Command;
using Newtonsoft.Json.Linq;

namespace EasySave_2._0
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
            ResourceDictionary dictionary = new JsonResourceDictionary(GetResourceUri(languageCode));
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
                var jsonString = System.IO.File.ReadAllText(uri.LocalPath);
                var jsonObject = JObject.Parse(jsonString);

                foreach (var property in jsonObject)
                {
                    this.Add(property.Key, property.Value.ToObject<object>());
                }
            }
            catch (Exception ex)
            {
                // Gérer les erreurs de chargement de fichier
                MessageBox.Show($"Erreur lors du chargement du fichier JSON : {ex.Message}");
            }
        }
    }
}