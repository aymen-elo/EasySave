using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.Models;

namespace EasySave.Views
{
    public class TranslationView
    {
        private TranslationModel _translation;

        public void UpdateTranslations(TranslationModel translation)
        {
            _translation = translation;
        }

        public void DisplayMessage(string message)
        {
            // Utiliser les traductions chargées pour afficher le message
            Console.WriteLine(message);
        }
    }

}
