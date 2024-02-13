using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EasySave.Library;
using EasySave.Models;

namespace EasySave.Controllers
{
    public class JobsController
    { 
        public List<Job> Jobs { get; private set; }
        private Logger _logger;
        
        public event EventHandler<string> FileSaved;

        public JobsController(Logger logger)
        {
            Jobs = new List<Job>();
            this._logger = logger;
            Initialize();
            
            
            /* Jobs List outside of app lifecycle */
            
            // TODO : Check state.json for job
            
            // abonnement  à l'événement FileSaved du BackupManager
            FileSaved += HandleFileSaved;
        }

        public List<Job> GetJobs()
        {
            return Jobs;
        }

        public void Initialize()
        {
            FileSaved += HandleFileSaved;
        }

        private void HandleFileSaved(object sender, string fileName)
        {
            _logger.LogAction(fileName, "", "", 0, TimeSpan.Zero);
        }

        // Méthode pour créer un travail de sauvegarde
        public void CreateJob(string nom, string source, string destination, BackupType backupType)
        {
            Job newBackupJob = new Job(nom, backupType,source, destination);

            Jobs.Add(newBackupJob);
        }

        // Méthode pour modifier un travail de sauvegarde existant
        public void EditJob(string nom, string source, string destination, BackupType backupType)
        {
            Job existingJob = Jobs.FirstOrDefault(job => job.BackupName == nom);
            if (existingJob != null)
            {
                existingJob.Source = source;
                existingJob.Destination = destination;
                existingJob.BackupType = backupType;
            }
            else
            {
                Console.WriteLine("Le travail de sauvegarde spécifié n'existe pas.");
            }
        }

        // Méthode pour supprimer un travail de sauvegarde
        public void DeleteJob(string nom)
        {
            Job backupJobToDelete = Jobs.FirstOrDefault(job => job.BackupName == nom);
            if (backupJobToDelete != null)
            {
                Jobs.Remove(backupJobToDelete);
                Console.WriteLine("Le travail de sauvegarde a été supprimé avec succès.");
            }
            else
            {
                Console.WriteLine("Le travail de sauvegarde spécifié n'existe pas.");
            }
        }
        
        public void AddJob(Logger logger, TranslationModel translation, Menu menu)
        {
            Console.Clear();
            
            
            /* Backup Name */
            Regex rg = new Regex(@"^[a-zA-Z0-9\s]*$");
            Console.Write(translation.Messages.EnterBackupName);

            string name = Console.ReadLine();
            while (!PatternRegEx(name, rg))
            {
                Console.WriteLine(translation.Messages.InvalidBackupName);
                Console.Write("Nom de sauvegarde : ");
                name = Console.ReadLine();
            }
            
            /* Backup Source */ 
            rg = new Regex(@"^[a-zA-Z]:\\(?:[^<>:""/\\|?*]+\\)*[^<>:""/\\|?*]*$");
            Console.Write(translation.Messages.SourceDirectory);

            string source = Console.ReadLine();
            while (!PatternRegEx(source, rg))
            {
                Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                Console.Write(translation.Messages.SourceDirectory);
                source = Console.ReadLine();
            }
            
            /* Backup Destination */
            Console.Write(translation.Messages.DestinationDirectory);
            string destination = Console.ReadLine();
            while (!PatternRegEx(destination, rg))
            {
                Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                Console.Write(translation.Messages.DestinationDirectory);
                destination = Console.ReadLine();
            }

            // Backup Type Choice
            Console.WriteLine(translation.Messages.ChooseBackupType);
            Console.WriteLine($"1. {translation.Messages.CompleteBackup}");
            Console.WriteLine($"2. {translation.Messages.DifferentialBackup}");
            Console.Write(translation.Messages.Choice);
            string backupType = Console.ReadLine();

            // Convertir le choix de l'utilisateur en type de sauvegarde
            string type = backupType == "1" ? (translation.Messages.CompleteBackup) : backupType == "2" ? (translation.Messages.DifferentialBackup) : null;

            if (type != null)
            {
                // Créer un objet BackupJob avec les informations saisies
                var job = new Job(name, BackupType.Full, source, destination);

                // Ajouter le travail de sauvegarde en appelant la méthode correspondante du contrôleur
                Jobs.Add(job);
                
                // Logger l'action effectuée en utilisant l'instance de Logger stockée dans jobsController
                logger.LogAction(name, source, destination, 0, TimeSpan.Zero);

                // Copier les fichiers en utilisant FileCopier
                var fileCopier = new FileCopier(menu);
                fileCopier.CopyDirectory(job);

                // Afficher la liste des travaux de sauvegarde après l'ajout
                DisplayJobs(translation);
            }
            else
            {
                Console.WriteLine(translation.Messages.InvalidTypeChoice);
            }
        }

        public void LaunchJob(Job job)
        {
        }

        public void LaunchAllJobs()
        {
            foreach (var job in Jobs)
            {
                LaunchJob(job);
            }
        }

        public void DisplayJobs(TranslationModel translation)
        {
            Console.WriteLine(translation.Messages.ListBackupJobs);

            if (Jobs.Count == 0)
            {
                Console.WriteLine(translation.Messages.EmptyJobsList);
            }

            int i = 0;
            foreach (var travail in this.GetJobs())
            {
                Console.WriteLine(
                    $" ({i}) - {translation.Messages.EnterBackupName} : {travail.BackupName}, {translation.Messages.SourceDirectory} : {travail.Source}, {translation.Messages.DestinationDirectory} : {travail.Destination}, {translation.Messages.ChooseBackupType} {travail.BackupType}");
                i++;
            }
            
            Console.WriteLine("\nAppuyer sur une touche pour revenir au menu de sauvegarde...");
            Console.ReadKey();
            Console.Clear();
        }

        public static void EditJob(JobsController jobsController)
        {
            // Implémenter la logique de modification d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour modifier un travail existant
        }

        public void RemoveJob(JobsController jobsController, Logger logger)
        {
            Console.Write("Nom du travail de sauvegarde à supprimer : ");
            string nomTravail = Console.ReadLine();
            
            jobsController.DeleteJob(nomTravail);
            logger.LogAction(nomTravail, "", "", 0, TimeSpan.Zero);
        }
        static bool PatternRegEx(string text, Regex pattern)
        {
            Match m = pattern.Match(text);
            return m.Success;
        }
    }
}
