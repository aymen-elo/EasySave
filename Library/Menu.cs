﻿using System;
using System.Text.RegularExpressions;
using EasySave.Controllers;
using EasySave.Models;

namespace EasySave.Library
{
    public class Menu
    {
        private readonly TranslationController _translationController;
        private readonly JobsController _jobsController;
        private readonly Logger _logger;
        private readonly TranslationManager _translationManager;
        private TranslationModel _translation; // Champ de classe pour stocker la traduction

        public Menu(TranslationController translationController, JobsController jobsController, Logger logger, TranslationManager translationManager)
        {
            _translationController = translationController;
            _jobsController = jobsController;
            _logger = logger;
            _translationManager = translationManager;
        }

        public void Run()
        {
            _translationController.Run();
            _translation = _translationManager.LoadTranslation(_translationController.Language); // Assigner la traduction au champ de classe

            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                Console.WriteLine(_translation.Menu.PrincipalMenu);
                Console.WriteLine($"1. {_translation.Menu.Option}");
                Console.WriteLine($"2. {_translation.Menu.BackupManage}");
                Console.WriteLine($"3. {_translation.Menu.DoBackup}");
                Console.WriteLine($"4. {_translation.Menu.Quit}");

                Console.Write("Choix : ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Options();
                        break;
                    case "2":
                        ManageJobs();
                        break;
                    case "3":
                        break;
                    case "4":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine(_translation.Messages.InvalidChoice);
                        break;
                }
            }

            _logger.DisplayLog();
        }

        private void Options()
        {
            _translationController.Run();
            _translation = _translationManager.LoadTranslation(_translationController.Language);
        }

        private void ManageJobs()
        {
            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine(_translation.Menu.BackupManage);
                Console.WriteLine($"0. {_translation.Menu.DisplayJobs}");
                Console.WriteLine($"1. {_translation.Menu.AddBackupJob}");
                Console.WriteLine($"2. {_translation.Menu.EditBackupJob}");
                Console.WriteLine($"3. {_translation.Menu.DeleteBackupJob}");
                Console.WriteLine($"4. {_translation.Menu.ReturnToMainMenu}");

                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "0":
                        _jobsController.DisplayJobs(_translation);
                        break;
                    case "1":
                        _jobsController.AddJob(_logger, _translation, this);
                        break;
                    case "2":
                        break;
                    case "3":
                        _jobsController.RemoveJob(_jobsController, _logger);
                        break;
                    case "4":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine(_translation.Messages.InvalidChoice);
                        break;
                }
            }
        }
    }
}
