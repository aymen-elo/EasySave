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
        private JobManager _jobManager;
        private  Logger _logger;
        public Logger Logger { get { return _logger; } }

        public JobsController()
        {
            _jobManager = new JobManager();
            Initialize();
        }

        public JobsController(JobManager jobManager, Logger logger)
        {
            this._jobManager = jobManager;
            this._logger = logger;
            Initialize();

            // abonnement  à l'événement FileSaved du BackupManager
            this._jobManager.FileSaved += HandleFileSaved;
        }

        public List<Job> GetBackupJobs()
        {
            return _jobManager.Jobs;
        }

        public void Initialize()
        {
            _jobManager.FileSaved += HandleFileSaved;
        }

        public void PerformBackup(string fileName)
        {
            _jobManager.SaveFile(fileName);
        }

        private void HandleFileSaved(object sender, string fileName)
        {
            _logger.LogAction($"Le fichier {fileName} a été sauvegardé.");
        }

        // Méthode pour créer un travail de sauvegarde
        public void CreateBackupJob(string nom, string repertoireSource, string repertoireCible, string type)
        {
            Job newBackupJob = new Job(nom, BackupType.Full,repertoireSource, repertoireCible);

            _jobManager.AddJob(newBackupJob);
        }

        // Méthode pour modifier un travail de sauvegarde existant
        public void ModifyBackupJob(string nom, string newRepertoireSource, string newRepertoireCible, BackupType type)
        {
            Job existingBackupJob = _jobManager.Jobs.FirstOrDefault(job => job.BackupName == nom);
            if (existingBackupJob != null)
            {
                existingBackupJob.Source = newRepertoireSource;
                existingBackupJob.Destination = newRepertoireCible;
                // FIX :
                existingBackupJob.BackupType = BackupType.Full;
            }
            else
            {
                Console.WriteLine("Le travail de sauvegarde spécifié n'existe pas.");
            }
        }

        // Méthode pour supprimer un travail de sauvegarde
        public void DeleteBackupJob(string nom)
        {
            Job backupJobToDelete = _jobManager.Jobs.FirstOrDefault(job => job.BackupName == nom);
            if (backupJobToDelete != null)
            {
                _jobManager.RemoveJob(backupJobToDelete);
                Console.WriteLine("Le travail de sauvegarde a été supprimé avec succès.");
            }
            else
            {
                Console.WriteLine("Le travail de sauvegarde spécifié n'existe pas.");
            }
        }

        internal void AddBackupJob(Job nouveauTravailSauvegarde)
        {
            _jobManager.AddJob(nouveauTravailSauvegarde);
        }

    }
}

