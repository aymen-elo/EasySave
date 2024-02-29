using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveLib.Model;
using EasySaveRemote.Packets;
using Newtonsoft.Json;

namespace EasySaveRemote.ViewModel
{
    public class EditJobViewModel : ViewModelBase
    {
        public event Action RequestClose;

        public ICommand EditJobCommand { get; set; }
        private Job _job;
        private Logger _logger;
        private MainWindow _mainWindow;

        public string JobName { get; set; }
        public string JobSource { get; set; }
        public string JobTarget { get; set; }
        public int JobTypeIdx { get; set; }

        public EditJobViewModel(Job job, MainWindow mainWindow)
        {
            _job = job;
            _logger = Logger.GetInstance();
            _mainWindow = mainWindow;

            /* Job details from the Edit Window */
            JobName = job.Name;
            JobSource = job.SourceFilePath;
            JobTarget = job.TargetFilePath;
            JobTypeIdx = job.BackupType == BackupType.Full ? 0 : 1;

            EditJobCommand = new RelayCommand(EditJob);
        }

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
                string oldName = _job.Name;
                _job.Name = newName; // Search by Name => Update Name later on

                string oldNameJson = JsonConvert.SerializeObject(oldName);
                string jobJson = JsonConvert.SerializeObject(_job);
                
                string combinedJson = "[{\"OldName\":" + oldNameJson + "}," + jobJson + "]";
                
                
                SendMessage.SendMessageTo("127.0.0.1", 13,combinedJson , MessageType.EJ, _mainWindow); 

                
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