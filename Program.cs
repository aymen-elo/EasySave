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

            // Affichage du menu principal et gestion des interactions utilisateur
            bool continuer = true;
            while (continuer)
            {
                Console.Clear();
                menu.ManageJobs(jobsController, logger);
            }
            // Afficher le contenu du fichier journal dans la console à la fin de l'exécution
            logger.DisplayLog();
        }
    }
}
