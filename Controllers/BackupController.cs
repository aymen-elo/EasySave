using EasySave.Models;
using Language_test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language_test.Controllers
{
    public class BackupController
    {
        private  BackupManager backupManager;
        private  Logger logger;
        public Logger Logger { get { return logger; } }


        public BackupController(BackupManager backupManager, Logger logger)
        {
            this.backupManager = backupManager;
            this.logger = logger;
            Initialize();

            // abonnement  à l'événement FileSaved du BackupManager
            this.backupManager.FileSaved += HandleFileSaved;
        }

        private void Initialize()
        {
            backupManager.FileSaved += HandleFileSaved;
        }

        public List<BackupJob> GetBackupJobs()
        {
            return backupManager.BackupJobs;
        }

        public void PerformBackup(string fileName)
        {
            backupManager.SaveFile(fileName);
        }

        private void HandleFileSaved(object sender, string fileName)
        {
            logger.LogAction($"Le fichier {fileName} a été sauvegardé.");
        }

        // Méthode pour créer un travail de sauvegarde
        public void CreateBackupJob(string nom, string repertoireSource, string repertoireCible, string type)
        {
            BackupJob newBackupJob = new BackupJob
            {
                Nom = nom,
                RepertoireSource = repertoireSource,
                RepertoireCible = repertoireCible,
                Type = type
            };
            backupManager.AddBackupJob(newBackupJob);
        }

        // Méthode pour modifier un travail de sauvegarde existant
        public void ModifyBackupJob(string nom, string newRepertoireSource, string newRepertoireCible, string newType)
        {
            BackupJob existingBackupJob = backupManager.BackupJobs.FirstOrDefault(job => job.Nom == nom);
            if (existingBackupJob != null)
            {
                existingBackupJob.RepertoireSource = newRepertoireSource;
                existingBackupJob.RepertoireCible = newRepertoireCible;
                existingBackupJob.Type = newType;
            }
            else
            {
                Console.WriteLine("Le travail de sauvegarde spécifié n'existe pas.");
            }
        }

        // Méthode pour supprimer un travail de sauvegarde
        public void DeleteBackupJob(string nom)
        {
            BackupJob backupJobToDelete = backupManager.BackupJobs.FirstOrDefault(job => job.Nom == nom);
            if (backupJobToDelete != null)
            {
                backupManager.RemoveBackupJob(backupJobToDelete);
                Console.WriteLine("Le travail de sauvegarde a été supprimé avec succès.");
            }
            else
            {
                Console.WriteLine("Le travail de sauvegarde spécifié n'existe pas.");
            }
        }

        internal void AddBackupJob(BackupJob nouveauTravailSauvegarde)
        {
            backupManager.AddBackupJob(nouveauTravailSauvegarde);
        }

    }
}

