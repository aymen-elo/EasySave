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
        }

        public void StartBackup()
        {
            if (!_backupWorker.IsBusy)
            {
                _backupWorker.RunWorkerAsync();
            }
        }
    }
}