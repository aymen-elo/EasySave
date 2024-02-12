using System;
using EasySave.Models;
using EasySave.Views;

namespace EasySave.Controllers
{
    public class TranslationController
    {
        private readonly TranslationManager _translationManager;
        private readonly TranslationView _translationView;

        public TranslationController()
        {
            _translationManager = new TranslationManager();
            _translationView = new TranslationView();
        }

        public void Run()
        {
            _translationView.DisplayMessage("Please choose a language (en/fr): ");
            string lang = Console.ReadLine().ToLower();

            TranslationModel translation;
            if (lang == "en" || lang == "fr")
            {
                translation = _translationManager.LoadTranslation(lang);
                _translationView.UpdateTranslations(translation); // Mettre à jour les traductions dans la vue
                _translationView.DisplayMessage(
                    "Language updated successfully."); // Afficher un message de confirmation
            }
            else
            {
                _translationView.DisplayMessage("Invalid language choice.");
            }
        }
    }
}
