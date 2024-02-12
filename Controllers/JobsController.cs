using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _logger.LogAction(fileName, "", "", 0, 0);
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

        internal void AddJob(Job nouveauTravailSauvegarde)
        {
            Jobs.Add(nouveauTravailSauvegarde);
        }

    }
}

