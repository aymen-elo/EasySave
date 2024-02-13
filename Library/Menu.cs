using System;
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
        }

        private void ManageJobs()
        {
            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine(_translation.Menu.BackupManage);
                Console.WriteLine($"1. {_translation.Menu.AddBackupJob}");
                Console.WriteLine($"2. {_translation.Menu.EditBackupJob}");
                Console.WriteLine($"3. {_translation.Menu.DeleteBackupJob}");
                Console.WriteLine($"4. {_translation.Menu.ReturnToMainMenu}");

                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        AddJob(_jobsController, _logger);
                        break;
                    case "2":
                        break;
                    case "3":
                        RemoveJob(_jobsController, _logger);
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
        
         void AddJob(JobsController jobsController, Logger logger)
        {
            Console.Clear();
            // Demander à l'utilisateur de saisir les informations pour ajouter un travail de sauvegarde
            Console.Write("Nom de sauvegarde : ");
            
            Regex rg = new Regex(@"^[a-zA-Z0-9\s]*$");
            string nom = Console.ReadLine();
            while (!PatternRegEx(nom, rg))
            {
                Console.WriteLine("Erreur : Veuillez écrire un nom de sauvegarde composé de lettres et/ou de chiffre");
                Console.Write("Nom de sauvegarde : ");
                nom = Console.ReadLine();
            }
            
            rg = new Regex(@"^[a-zA-Z]:\\(?:[^<>:""/\\|?*]+\\)*[^<>:""/\\|?*]*$");
            Console.Write("Répertoire source : ");
            string repertoireSource = Console.ReadLine();
            while (!PatternRegEx(repertoireSource, rg))
            {
                Console.WriteLine("Erreur : Veuillez écrire un chemin de sauvegarde correct.");
                Console.Write("Répertoire source : ");
                repertoireSource = Console.ReadLine();
            }
            Console.Write("Répertoire cible : ");
            string repertoireCible = Console.ReadLine();
            while (!PatternRegEx(repertoireCible, rg))
            {
                Console.WriteLine("Erreur : Veuillez écrire un chemin de sauvegarde correct.");
                Console.Write("Répertoire source : ");
                repertoireCible = Console.ReadLine();
            }

            // Demander le type de sauvegarde à l'utilisateur
            Console.WriteLine("Type de sauvegarde :");
            Console.WriteLine($"1. {_translation.Messages.CompleteBackup}");
            Console.WriteLine($"2. {_translation.Messages.DifferentialBackup}");
            Console.Write(_translation.Messages.Choice);
            string choixType = Console.ReadLine();

            // Convertir le choix de l'utilisateur en type de sauvegarde
            string type = choixType == "1" ? (_translation.Messages.CompleteBackup) : choixType == "2" ? (_translation.Messages.DifferentialBackup) : null;

            if (type != null)
            {
                // Créer un objet BackupJob avec les informations saisies
                var nouveauTravailSauvegarde = new Job(nom, BackupType.Full, repertoireSource, repertoireCible);

                // Ajouter le travail de sauvegarde en appelant la méthode correspondante du contrôleur
                jobsController.AddJob(nouveauTravailSauvegarde);

                // Logger l'action effectuée en utilisant l'instance de Logger stockée dans jobsController
                logger.LogAction(nom, repertoireSource, repertoireCible, 0, 0);

                // Copier les fichiers en utilisant FileCopier
                var fileCopier = new FileCopier();
                fileCopier.CopyDirectory(nouveauTravailSauvegarde);

                // Afficher la liste des travaux de sauvegarde après l'ajout
                DisplayJobs(jobsController);
            }
            else
            {
                Console.WriteLine(_translation.Messages.InvalidTypeChoice);
            }
        }


         void DisplayJobs(JobsController jobsController)
        {
            Console.WriteLine(_translation.Messages.ListBackupJobs);
            foreach (var travail in jobsController.GetJobs())
            {
                Console.WriteLine(
                    $"Nom : {travail.BackupName}, Répertoire source : {travail.Source}, Répertoire cible : {travail.Destination}, Type : {travail.BackupType}");
            }
        }

        static void EditJob(JobsController jobsController)
        {
            // Implémenter la logique de modification d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour modifier un travail existant
        }

        static void RemoveJob(JobsController jobsController, Logger logger)
        {
            // Implémenter la logique de suppression d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour supprimer un travail existant
            Console.Write("Nom du travail de sauvegarde à supprimer : ");
            string nomTravail = Console.ReadLine();

            // Supprimer le travail de sauvegarde en appelant la méthode correspondante du contrôleur
            jobsController.DeleteJob(nomTravail);

            // Logger l'action effectuée en utilisant l'instance de Logger passée en paramètre
            logger.LogAction(nomTravail, "", "", 0, 0);
        }
        static bool PatternRegEx(string text, Regex pattern)
        {
            Match m = pattern.Match(text);
            if (m.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
