using System;
using EasySave.Model.Translation;

namespace EasySave.View
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
