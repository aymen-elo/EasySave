﻿using EasySave.Models;
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
            // Instanciation du modèle (Model)
            var jobsManager = new JobsManager();
            var logger = new Logger();
            var jobsController = new JobsController(jobsManager, logger);
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
                        menu.JobsManager(jobsController, logger);
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
