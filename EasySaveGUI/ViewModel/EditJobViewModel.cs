using System;
using System.Windows.Controls;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveLib.Model;

namespace EasySaveGUI.ViewModel
{
    public class EditJobViewModel : ViewModelBase
    {
        public ICommand EditJobCommand { get; set; }
        
        public string JobName { get; set; }
        public string JobSource { get; set; }
        public string JobTarget { get; set; }
        public int JobTypeIdx { get; set; }
        
        public EditJobViewModel(Job job)
        {
            //EditJobCommand = new RelayCommand(EditJob);
            JobName = job.Name;
            JobSource = job.SourceFilePath;
            JobTarget = job.TargetFilePath;
            
            JobTypeIdx = job.BackupType == BackupType.Full ? 0 : 1;
        }
        
        private void EditJob(object parameter)
        {
            
        }
        
        
        ////////////////////////////
        ///  INSTRUCTIONS
        /// ///////////////////////
        /*public void EditJob(int index, string name, string source, string destination, BackupType backupType)
        {
            Job job = _jobs[index];
            if (job != null)
            {
                job.SourceFilePath = source;
                job.TargetFilePath = destination;
                job.BackupType = backupType;

                UpdateJobData(name, job);
                // Renaming for the current job object (LogState Constaints)
                job.Name = name;
            }
        }*/
    }
}