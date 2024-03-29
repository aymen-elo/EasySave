﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using EasySaveGUI.Command;
using System.Windows.Forms;
using EasySaveGUI.Helper;
using EasySaveLib.Model;
using EasySaveRemote.Packets;
using Newtonsoft.Json;


namespace EasySaveRemote.ViewModel
{
    public class AddJobViewModel : ViewModelBase
    {
        public event Action RequestClose;
        
        public ICommand AddJobCommand { get; set; }
        private Logger _logger;
        private CopyController _copyController = new CopyController();
        private MainWindow _mainWindow;
        
        public string JobName { get; set; }
        public string JobSource { get; set; }
        public string JobTarget { get; set; }
        public int JobTypeIdx { get; set; }
        
        private ObservableCollection<Job> _jobs;
        
        public ICommand OpenSourceCommand { get; private set; }
        public ICommand OpenDestinationCommand { get; private set; }

        
        public AddJobViewModel(ObservableCollection<Job> jobs, MainWindow mainWindow)
        {
            _logger = Logger.GetInstance();
            _jobs = jobs;
            _mainWindow = mainWindow;

            AddJobCommand = new RelayCommand(AddJob);
            
            /* Job details from the Edit Window */
            OpenSourceCommand = new RelayCommand(OpenSourceDialog);
            OpenDestinationCommand = new RelayCommand(OpenDestinationDialog);

        }
        
        private void OpenSourceDialog(object obj)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    JobSource = dialog.SelectedPath;
                    OnPropertyChanged(nameof(JobSource)); 
                }
            }
        }

        private void OpenDestinationDialog(object obj)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    JobTarget = dialog.SelectedPath;
                    OnPropertyChanged(nameof(JobTarget)); 
                }
            }
        }
        
        private void AddJob(object obj)
        {
            Job _job = new Job(JobName, JobTypeIdx == 0 ? BackupType.Full : BackupType.Diff, JobSource, JobTarget);
            
            string jobJson = JsonConvert.SerializeObject(_job);

            
            SendMessage.SendMessageTo("127.0.0.1", 13,jobJson , MessageType.NJ, _mainWindow);

            RequestClose?.Invoke();
        }
        
        private void UpdateJobData(string newName, Job job)
        {
            // helpers to calculate the new directory's size info
            DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
            long totalFilesSize = _copyController._fileGetter.DirSize(diSource);
            int totalFilesToCopy = _copyController._fileGetter.GetAllFiles(job.SourceFilePath).Count;
                
            job.TotalFilesSize = totalFilesSize;
            job.NbFilesLeftToDo = totalFilesToCopy;
            job.TotalFilesToCopy = totalFilesToCopy;
            job.NbSavedFiles = 0;

            _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), newName);
        }
    }
}
