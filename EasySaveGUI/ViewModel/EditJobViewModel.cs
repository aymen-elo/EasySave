using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveLib.Model;
using MessageBox = System.Windows.MessageBox;

namespace EasySaveGUI.ViewModel
{
    /// <summary>
    /// ViewModel for the logic of editing a backup job.
    /// </summary>
    public class EditJobViewModel : ViewModelBase
    {
        /// <summary>
        /// Event that is triggered when the job editing is completed.
        /// </summary>
        public event Action RequestClose;

        /// <summary>
        /// Command to edit the job.
        /// </summary>
        public ICommand EditJobCommand { get; set; }

        private Job _job;
        private Logger _logger;

        public string JobName { get; set; }
        public string JobSource { get; set; }
        public string JobTarget { get; set; }
        public int JobTypeIdx { get; set; }
        
        public ICommand OpenSourceCommand { get; private set; }
        public ICommand OpenDestinationCommand { get; private set; }

        /// <summary>
        /// Constructor for EditJobViewModel.
        /// Initializes the job details and sets up the commands for editing the job and opening the source and destination dialogs.
        /// </summary>
        /// <param name="job">The job to edit.</param>
        public EditJobViewModel(Job job)
        {
            _job = job;
            _logger = Logger.GetInstance();

            /* Job details from the Edit Window */
            JobName = job.Name;
            JobSource = job.SourceFilePath;
            JobTarget = job.TargetFilePath;
            JobTypeIdx = job.BackupType == BackupType.Full ? 0 : 1;

            EditJobCommand = new RelayCommand(EditJob);
            OpenSourceCommand = new RelayCommand(OpenSourceDialog);
            OpenDestinationCommand = new RelayCommand(OpenDestinationDialog);
        }
        
        /// <summary>
        /// Opens the source dialog to select the source path for the job.
        /// </summary>
        /// <param name="obj">Not used.</param>
        private void OpenSourceDialog(object obj)
        {
            string selectedPath = ShowFolderBrowserDialog(JobSource);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                JobSource = selectedPath;
                OnPropertyChanged(nameof(JobSource));
            }
        }

        /// <summary>
        /// Opens the destination dialog to select the destination path for the job.
        /// </summary>
        /// <param name="obj">Not used.</param>
        private void OpenDestinationDialog(object obj)
        {
            string selectedPath = ShowFolderBrowserDialog(JobTarget);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                JobTarget = selectedPath;
                OnPropertyChanged(nameof(JobTarget));
            }
        }

        /// <summary>
        /// Shows a folder browser dialog with the specified initial path.
        /// </summary>
        /// <param name="initialPath">The initial path for the folder browser dialog.</param>
        /// <returns>The selected path from the folder browser dialog.</returns>
        private string ShowFolderBrowserDialog(string initialPath)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = initialPath;

                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Edits the job with the details from the edit window.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        private void EditJob(object parameter)
        {
            var newName = JobName;
            try
            {
                if (!Directory.Exists(JobSource) || !Directory.Exists(JobTarget))
                {
                    throw new FileNotFoundException("Source file or target file doesn't exist.");
                }

                _job.SourceFilePath = JobSource;
                _job.TargetFilePath = JobTarget;
                _job.BackupType = JobTypeIdx == 0 ? BackupType.Full : BackupType.Diff;

                _logger.LogState(_job, newName);
                _job.Name = newName; // Search by Name => Update Name later on

                RequestClose?.Invoke();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Error : {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There is an error : {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}