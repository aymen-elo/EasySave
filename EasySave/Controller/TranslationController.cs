using System;
using EasySave.Models;
using EasySave.Views;

namespace EasySave.Controllers
{
    public class TranslationController
    {
        private readonly TranslationManager _translationManager;
        private readonly TranslationView _translationView;
        TranslationModel translation;


        public string Language { get; set; }

        public TranslationController()
        {
            _translationManager = new TranslationManager();
            _translationView = new TranslationView();
        }

        public void Run()
        {
            _translationView.DisplayMessage("Please choose a language (en/fr): ");
            string lang = Console.ReadLine().ToLower();

            if (lang == "en" || lang == "fr")
            {
                Language = lang;

                translation = _translationManager.LoadTranslation(lang);
                _translationView.DisplayMessage(translation.Messages.LanguageUpdated);
            }
            else
            {
                Console.WriteLine("Invalid language choice");
                Console.Clear();
                Run();
            }
        }
    }
}
