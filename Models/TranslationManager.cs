using Language_test.Views;
using System.Text.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language_test.Controllers
{
    public class TranslationManager
    {
        public TranslationModel LoadTranslation(string lang)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TranslationFiles", $"{lang}.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<TranslationModel>(jsonContent);
        }
    }
}
