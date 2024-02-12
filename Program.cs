using EasySave.Models;
using EasySave.Views;
using System;
using System.Collections.Generic;
using EasySave.Controllers;
using EasySave.Library;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new Logger();
            var jobsController = new JobsController(logger);
            var translationController = new TranslationController();
            var menu = new Menu();
            var translationManager = new TranslationManager();

            translationController.Run();

            TranslationModel translation = translationManager.LoadTranslation(translationController.Language);

            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                Console.WriteLine(translation.Menu.PrincipalMenu);
                Console.WriteLine($"1. {translation.Menu.Option}");
                Console.WriteLine($"2. {translation.Menu.BackupManage}");
                Console.WriteLine($"3. {translation.Menu.DoBackup}");
                Console.WriteLine($"4. {translation.Menu.Quit}");

                Console.Write("Choix : ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        menu.Options(translationController);
                        break;
                    case "2":
                        menu.ManageJobs(jobsController, logger);
                        break;
                    case "3":
                        break;
                    case "4":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine(translation.Messages.InvalidChoice);
                        break;
                }
                
            }
            logger.DisplayLog();
        }
    }
}
