using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            string lang = Console.ReadLine();

            TranslationModel translation;
            if (lang.ToLower() == "en")
            {
                translation = _translationManager.LoadTranslation("en");
            }
            else
            {
                translation = _translationManager.LoadTranslation("fr");
            }

        }
    }
}
