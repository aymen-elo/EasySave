using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language_test.Models
{
    public class BackupManager
    {
        public event EventHandler<string> FileSaved;

        // Méthode pour effectuer une sauvegarde
        public void SaveFile(string fileName)
        {
            // Logique de sauvegarde (simulation)
            Console.WriteLine($"Le fichier {fileName} a été sauvegardé.");

            // Déclenchement de l'événement de sauvegarde
            FileSaved?.Invoke(this, fileName);
        }

        public List<BackupJob> BackupJobs { get; set; }

        public BackupManager()
        {
            BackupJobs = new List<BackupJob>();
        }

        internal void RemoveBackupJob(BackupJob backupJobToDelete)
        {
            BackupJobs.Remove(backupJobToDelete);
        }

        internal void AddBackupJob(BackupJob newBackupJob)
        {
            BackupJobs.Add(newBackupJob);
        }
    }
}
