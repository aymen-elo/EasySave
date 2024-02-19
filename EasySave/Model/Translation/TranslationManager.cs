using EasySave.View;
using System.Text.Json;
using System.IO;
using System;

namespace EasySave.Model.Translation
{
    public class TranslationManager
    {
        public TranslationModel LoadTranslation(string lang)
        {
            
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", $"{lang}.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }
            
            string jsonContent = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<TranslationModel>(jsonContent);
            
        }
        
        
    }
}
