using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using EasySaveGUI.View;
using EasySaveLib.Model;

namespace EasySaveGUI.Model
{
    public class BackupProcess
    {
        private readonly Job _job;
        private readonly BackgroundWorker _backupWorker;
        private readonly System.Windows.Controls.ProgressBar _progressBar; // Utilisation du même type de ProgressBar

        public BackupProcess(Job job, System.Windows.Controls.ProgressBar progressBar)
        {
            _job = job;
            _backupWorker = new BackgroundWorker();
            _backupWorker.WorkerReportsProgress = true;
            _backupWorker.DoWork += BackupWorker_DoWork;
            _backupWorker.ProgressChanged += BackupWorker_ProgressChanged;
            _backupWorker.RunWorkerCompleted += BackupWorker_RunWorkerCompleted;
            _progressBar = progressBar; // Stockez la référence à la barre de progression
        }



        public void StartBackup()
        {
            if (!_backupWorker.IsBusy)
            {
                _backupWorker.RunWorkerAsync();
            }
        }

        private void BackupWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DateTime timestamp = DateTime.Now;
            try
            {
                string sourcePath = _job.SourceFilePath;
                string destinationPath = _job.TargetFilePath;

                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                int totalFilesToCopy = files.Length;
                long totalFileSize = 0;
                foreach (string filePath in files)
                {
                    totalFileSize += new FileInfo(filePath).Length;
                }

                int nbFilesCopied = 0;

                foreach (string filePath in files)
                {
                    string destinationFile = Path.Combine(destinationPath, Path.GetFileName(filePath));
                    destinationFile = GetUniqueFileName(destinationFile);
                    File.Copy(filePath, destinationFile, true);
                    nbFilesCopied++;
                    int progression = (int)((double)nbFilesCopied / totalFilesToCopy * 100);
                    _backupWorker.ReportProgress(progression);
                }

                // Code pour enregistrer l'état du travail dans un fichier JSON (comme dans votre exemple)
            }
            catch (Exception ex)
            {
                // Gérer les exceptions
                MessageBox.Show("Une erreur s'est produite lors de la copie : " + ex.Message, "Erreur de copie", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackupWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Mettre à jour la barre de progression
            Application.Current.Dispatcher.Invoke(() =>
            {
                _progressBar.Value = e.ProgressPercentage;
            });
        }


        private void BackupWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Afficher un message de succès ou d'erreur après la fin du travail (comme dans votre exemple)
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("La sauvegarde a été effectuée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private string GetUniqueFileName(string filePath)
        {
            if (File.Exists(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);

                int counter = 1;
                string newFilePath;

                do
                {
                    string counterString = $"_{counter}";
                    newFilePath = Path.Combine(directory, fileName + counterString + extension);
                    counter++;
                } while (File.Exists(newFilePath));

                return newFilePath;
            }

            return filePath; 
        }

    }
}