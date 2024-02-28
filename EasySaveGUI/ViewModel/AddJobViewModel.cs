using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using EasySaveGUI.Command;
using System.Windows.Forms;
using EasySaveGUI.Controller;
using EasySaveLib.Model;


namespace EasySaveGUI.ViewModel
{
    public class AddJobViewModel : ViewModelBase
    {
        public event Action RequestClose;
        
        public ICommand AddJobCommand { get; set; }
        private Logger _logger;
        private CopyController _copyController = new CopyController();
        
        public string JobName { get; set; }
        public string JobSource { get; set; }
        public string JobTarget { get; set; }
        public int JobTypeIdx { get; set; }
        
        private ObservableCollection<Job> _jobs;
        
        public AddJobViewModel(ObservableCollection<Job> jobs)
        {
            _logger = Logger.GetInstance();
            _jobs = jobs;

            AddJobCommand = new RelayCommand(AddJob);
            
            /* Job details from the Edit Window */

        }
        
        private void AddJob(object obj)
        {
            Job j = new Job(JobName, JobTypeIdx == 0 ? BackupType.Full : BackupType.Diff, JobSource, JobTarget);
            UpdateJobData(j.Name, j);
            
            _jobs.Add(j);
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
