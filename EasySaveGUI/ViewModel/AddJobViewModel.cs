using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using EasySaveGUI.Command;
using System.Windows.Forms;
using EasySaveLib.Model;


namespace EasySaveGUI.ViewModel
{
    public class AddJobViewModel : ViewModelBase
    {
        public event Action RequestClose;
        
        public ICommand OpenSourceCommand { get; }
        public ICommand OpenDestinationCommand { get; }
        public ICommand AddJobCommand { get; set; }
        private Logger _logger;
        
        private string _jobName;
        private ObservableCollection<Job> _jobs;
        
        public string JobName
        {
            get { return _jobName; }
            set
            {
                _jobName = value;
                OnPropertyChanged(nameof(JobName));
            }
        }

        private string _sourcePath;
        public string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                _sourcePath = value;
                OnPropertyChanged(nameof(SourcePath));
            }
        }

        private string _destinationPath;
        public string DestinationPath
        {
            get { return _destinationPath; }
            set
            {
                _destinationPath = value;
                OnPropertyChanged(nameof(DestinationPath));
            }
        }
        
        private BackupType _selectedBackupType = BackupType.Full;
        public BackupType SelectedBackupType
        {
            get { return _selectedBackupType; }
            set
            {
                _selectedBackupType = value;
                OnPropertyChanged(nameof(SelectedBackupType));
            }
        }
        
        public AddJobViewModel(ObservableCollection<Job> jobs)
        {
            _logger = Logger.GetInstance();
            _jobs = jobs;

            AddJobCommand = new RelayCommand(AddJob);
            OpenSourceCommand = new RelayCommand(OpenSource);
            OpenDestinationCommand = new RelayCommand(OpenDestination);
            
            /* Job details from the Edit Window */

        }
        
        private void AddJob(object parameter)
        {
            //TODO 
            //Job newJob = new Job(JobName, SourcePath, DestinationPath, SelectedBackupType);
            //_jobs.Add(newJob);
            
            RequestClose?.Invoke();
        }
        
        private void OpenSource(object parameter)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SourcePath = dialog.SelectedPath;
                }
            }
        }

        private void OpenDestination(object parameter)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    DestinationPath = dialog.SelectedPath;
                }
            }
        }
    }
}
