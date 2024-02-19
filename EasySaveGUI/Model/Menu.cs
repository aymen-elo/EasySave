using System;
using EasySaveGUI.Controller;
using EasySaveGUI.Model.Translation;

namespace EasySaveGUI.Model
{
    public class Menu
    {
        private readonly TranslationController _translationController;
        private readonly JobsController _jobsController;
        private readonly Logger _logger;
        private readonly TranslationManager _translationManager;
        private TranslationModel _translation; 


        public Menu(TranslationController translationController, JobsController jobsController, Logger logger, TranslationManager translationManager)
        {
            _translationController = translationController;
            _jobsController = jobsController;
            _logger = logger;
            _translationManager = translationManager;
        }

        public void Run()
        {
        }

        private void Options()
        {
            //_translationController.Run();
            //_translation = _translationManager.LoadTranslation(_translationController.Language);

            Console.WriteLine(_translation.Menu.OptionsMenu);
            Console.WriteLine($"1. {_translation.Menu.ChangeLanguage}");
            Console.WriteLine($"2. {_translation.Menu.ChangeLogFormat}");
            Console.WriteLine($"3. {_translation.Menu.ReturnToMainMenu}");

            Console.Write(_translation.Messages.Choice);
            string choice = Console.ReadLine();
            

            switch (choice)
            {
                case "1":
                    ChangeLanguage();
                    break;
                case "2":
                    ChangeLogFormat();
                    break;
                case "3":
                    // Retour au menu principal
                    break;
                default:
                    Console.WriteLine(_translation.Messages.InvalidChoice);
                    break;
            }
        }
        private void ChangeLanguage()
        {
            Console.WriteLine(_translation.Messages.SelectLanguage);
            Console.WriteLine("1. English");
            Console.WriteLine("2. French");
            // Ajoutez d'autres langues selon vos besoins

            Console.Write(_translation.Messages.Choice);
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    _translationController.Language = "en"; // Anglais
                    break;
                case "2":
                    _translationController.Language = "fr"; // Français
                    break;
                // Ajoutez d'autres cas pour les langues supplémentaires
                default:
                    Console.WriteLine(_translation.Messages.InvalidChoice);
                    break;
            }

            _translation = _translationManager.LoadTranslation(_translationController.Language);
        }

        private void ChangeLogFormat()
        {
            Console.WriteLine(_translation.Menu.LogFormatMenu);
            Console.WriteLine($"1. JSON");
            Console.WriteLine($"2. XML");

            Console.Write(_translation.Messages.Choice);
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    _logger.LogFormat = "json";
                    Console.WriteLine(_translation.Messages.LogFormatChangedJson);
                    break;
                case "2":
                    _logger.LogFormat = "xml";
                    Console.WriteLine(_translation.Messages.LogFormatChangedXml);
                    break;
                default:
                    Console.WriteLine(_translation.Messages.InvalidChoice);
                    ChangeLogFormat();
                    break;
            }
        }

        private void ManageJobs()
        {
            
        }
        private void AddBackupJob() { }
    }
}
