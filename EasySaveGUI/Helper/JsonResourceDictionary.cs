using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace EasySaveGUI.Helper
{
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
                MessageBox.Show(ex.Message);
            }
        }
    }
}
