using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using EasySaveGUI.Command;
using System.Windows.Forms;
using EasySaveGUI.Helper;
using EasySaveLib.Model;

namespace EasySaveGUI.ViewModel
{
    /// <summary>
    /// ViewModel for adding a backup job.
    /// </summary>
    public class AddJobViewModel : ViewModelBase
    {
        /// <summary>
        /// Close event, triggered when the job addition is completed.
        /// </summary>
        public event Action RequestClose;

        /// <summary>
        /// Command to add the job.
        /// </summary>
        public ICommand AddJobCommand { get; set; }
        
        /// <summary>
        /// Commands for Windows Explorer (Paths)
        /// </summary>
        public ICommand OpenSourceCommand { get; private set; }
        public ICommand OpenDestinationCommand { get; private set; }

        private Logger _logger;
        private CopyController _copyController = new CopyController();

        public string JobName { get; set; }
        public string JobSource { get; set; }
        public string JobTarget { get; set; }
        public int JobTypeIdx { get; set; }

        private ObservableCollection<Job> _jobs;

        /// <summary>
        /// Constructor for AddJobViewModel.
        /// Initializes the jobs collection and sets up the commands for adding the job and opening the source and destination dialogs.
        /// </summary>
        /// <param name="jobs">The jobs collection to add the job to.</param>
        public AddJobViewModel(ObservableCollection<Job> jobs)
        {
            _logger = Logger.GetInstance();
            _jobs = jobs;

            AddJobCommand = new RelayCommand(AddJob);

            OpenSourceCommand = new RelayCommand(OpenSourceDialog);
            OpenDestinationCommand = new RelayCommand(OpenDestinationDialog);
        }

        /// <summary>
        /// Opens the source dialog to select the source path for the job.
        /// </summary>
        /// <param name="obj">Not used.</param>
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

        /// <summary>
        /// Opens the destination dialog to select the destination path for the job.
        /// </summary>
        /// <param name="obj">Not used.</param>
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

        /// <summary>
        /// Adds a job with the details from the add window.
        /// </summary>
        /// <param name="obj">Not used.</param>
        private void AddJob(object obj)
        {
            Job j = new Job(JobName, JobTypeIdx == 0 ? BackupType.Full : BackupType.Diff, JobSource, JobTarget);
            UpdateJobData(j.Name, j);

            _jobs.Add(j);
            RequestClose?.Invoke();
        }

        /// <summary>
        /// Updates the job data with the details from the add window.
        /// </summary>
        /// <param name="newName">The new name for the job.</param>
        /// <param name="job">The job to update.</param>
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