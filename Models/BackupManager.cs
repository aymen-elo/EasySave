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

        public List<BackupJob> BackupJobs { get; private set; }

        public BackupManager()
        {
            BackupJobs = new List<BackupJob>();
        }

        public void SaveFile(string fileName)
        {
            // Logique de sauvegarde réelle ici...
            Console.WriteLine($"Le fichier {fileName} a été sauvegardé.");
            OnFileSaved(fileName);
        }

        internal void AddBackupJob(BackupJob newBackupJob)
        {
            BackupJobs.Add(newBackupJob);
        }

        internal void RemoveBackupJob(BackupJob backupJobToDelete)
        {
            BackupJobs.Remove(backupJobToDelete);
        }

        protected virtual void OnFileSaved(string fileName)
        {
            FileSaved?.Invoke(this, fileName);
        }
    }
}
