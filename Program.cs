using EasySave.Models;
using EasySave.Views;
using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Controllers;
using EasySave.Library;

namespace EasySave
{
    class Program
    {
        
        // The directory where the files & hashes will be saved
        public static string LogsDirectoryPath = @"C:\Program Files\Prosoft\EasySave\Logs";
        static void Main(string[] args)
        {
            var logger = new Logger();
            var jobsController = new JobsController(logger);
            var translationController = new TranslationController();
            var menu = new Menu();
            var translationManager = new TranslationManager();
            // Charger les traductions
            TranslationModel translation = translationManager.LoadTranslation("en");
            
            

            // Affichage du menu principal et gestion des interactions utilisateur
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
                        // Gestion des sauvegardes
                        menu.ManageJobs(jobsController, logger);
                        break;
                    case "3":
                        // Ajoutez ici votre code pour l'option 3
                        break;
                    case "4":
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine("Choix invalide. Veuillez réessayer.");
                        break;
                }
                
            }
            // Afficher le contenu du fichier journal dans la console à la fin de l'exécution
            logger.DisplayLog();
        }
    }
}
