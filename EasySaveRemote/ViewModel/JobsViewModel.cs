using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EasySaveLib.Model;
using System.Linq;
using System.Threading.Tasks;
using EasySaveLib.Model;
using Newtonsoft.Json;

namespace EasySaveRemote.ViewModel
{
    public class JobsViewModel : ViewModelBase
    {
        /* Instead of a Jobs list for data binding purposes */
        private readonly ObservableCollection<Job> _jobs;
        public ObservableCollection<Job> Jobs => _jobs;
        
        private Logger Logger { get; }
        
        public JobsViewModel(Logger logger)
        {
            _jobs = new ObservableCollection<Job>();
            Logger = logger;
            Initialize();
        }

        private void Initialize()
        {
        }

        // Editing a job that exists -> used in another method EditJob(logger, translation)
        public void EditJob(int index, string name, string source, string destination, BackupType backupType)
        {
            
        }

        /* Job Deletion by Name method */
        public void DeleteJob(string name)
        {
            
        }
        
        public void AddJob(string name, string source, string destination, BackupType backupType)
        {
           
        }
        
        
        
        private void UpdateJobData(string newName, Job job)
        {
        }
        
    }
}
