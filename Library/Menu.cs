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
        public TranslationModel _translation; // Champ de classe pour stocker la traduction

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
                Console.WriteLine($"0. Display all Backup Jobs");
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
                        _jobsController.AddJob(_logger, _translation);
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
        
         void AddJob(JobsController jobsController, Logger logger)
        {
            Console.Clear();
            // Demander à l'utilisateur de saisir les informations pour ajouter un travail de sauvegarde
            Console.Write(_translation.Messages.EnterBackupName); 
            
            Regex rg = new Regex(@"^[a-zA-Z0-9\s]*$");
            string nom = Console.ReadLine();
            while (!PatternRegEx(nom, rg))
            { 
                Console.WriteLine(_translation.Messages.InvalidBackupName);
                Console.Write(_translation.Messages.EnterBackupName);
                nom = Console.ReadLine();
            }
            
            rg = new Regex(@"^[a-zA-Z]:\\(?:[^<>:""/\\|?*]+\\)*[^<>:""/\\|?*]*$");
            Console.Write(_translation.Messages.SourceDirectory); 
            string repertoireSource = Console.ReadLine();
            while (!PatternRegEx(repertoireSource, rg))
            {
                Console.WriteLine(_translation.Messages.InvalidBackupDirectory);
                Console.Write(_translation.Messages.SourceDirectory); 
                repertoireSource = Console.ReadLine();
            }
            Console.Write(_translation.Messages.DestinationDirectory); 
            string repertoireCible = Console.ReadLine();
            while (!PatternRegEx(repertoireCible, rg))
            {
                Console.WriteLine(_translation.Messages.InvalidBackupDirectory);
                Console.Write(_translation.Messages.SourceDirectory); 
                repertoireCible = Console.ReadLine();
            }

            // Demander le type de sauvegarde à l'utilisateur
            Console.WriteLine(_translation.Messages.ChooseBackupType);
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
                logger.LogAction(nom, repertoireSource, repertoireCible, 0, TimeSpan.Zero);

                // Copier les fichiers en utilisant FileCopier
                var fileCopier = new FileCopier(this);
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
                Console.WriteLine($"{_translation.Messages.EnterBackupName} {travail.BackupName}, {_translation.Messages.EnterSourceDirectory} {travail.Source}, {_translation.Messages.EnterTargetDirectory} {travail.Destination}, {_translation.Messages.ChooseBackupType} {travail.BackupType}");

            }
        }

        static void EditJob(JobsController jobsController)
        {
            // Implémenter la logique de modification d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour modifier un travail existant
        }

         void RemoveJob(JobsController jobsController, Logger logger)
        {
            // Implémenter la logique de suppression d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour supprimer un travail existant
            Console.Write(_translation.Messages.EnterJobNameToDelete);
            string nomTravail = Console.ReadLine();

            // Supprimer le travail de sauvegarde en appelant la méthode correspondante du contrôleur
            jobsController.DeleteJob(nomTravail);

            // Logger l'action effectuée en utilisant l'instance de Logger passée en paramètre
            logger.LogAction(nomTravail, "", "", 0, TimeSpan.Zero);
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
