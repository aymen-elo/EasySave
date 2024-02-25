using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using EasySaveGUI.View;
using EasySaveLib.Model;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace EasySaveGUI.Model
{
    public class BackupProcess
    {
        private readonly Job _job;
        private readonly BackgroundWorker _backupWorker;
        private readonly System.Windows.Controls.ProgressBar _progressBar; // Utilisation du même type de ProgressBar

        public BackupProcess(Job job)
        {
            _job = job;
            _backupWorker = new BackgroundWorker();
            _backupWorker.WorkerReportsProgress = true;
            _backupWorker.DoWork += BackupWorker_DoWork;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur s'est produite lors de la copie : " + ex.Message, "Erreur de copie",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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