using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EasySaveLib.Model;
using System.Linq;
using System.Threading.Tasks;
using EasySaveGUI.Model;
using EasySaveGUI.Controller;
using EasySaveLib.Model;
using Newtonsoft.Json;

namespace EasySaveGUI.ViewModel
{
    public class JobsViewModel : ViewModelBase
    {
        /* Instead of a Jobs list for data binding purposes */
        private readonly ObservableCollection<Job> _jobs;
        public ObservableCollection<Job> Jobs => _jobs;
        
        private Logger Logger { get; }
        private readonly CopyController _copyController;
        
        public JobsViewModel(Logger logger)
        {
            _jobs = new ObservableCollection<Job>();
            Logger = logger;
            _copyController = new CopyController();
            Initialize();
        }

        private void Initialize()
        {
            /* Handling the already existing backup jobs (if they exist) */
            // Checking if the State logging file exists
            var stateFile = Logger.LogsDirectoryPath + @"\state.json";
            if (File.Exists(stateFile) & new FileInfo(stateFile).Length != 0)
            {
                string jsonContent = File.ReadAllText(stateFile);
                var content = JsonConvert.DeserializeObject<ObservableCollection<Job>>(jsonContent);

                foreach (var jobInfo in content)
                {
                    // When the job is not taken account of -> we ignore it because it's not a Job anymore 
                    if (jobInfo.State == JobState.Retired) { continue; }
                    
                    // Construct a new Job with the available data in the json and append it to the Jobs List
                    var job = new Job(jobInfo.Name, jobInfo.BackupType, jobInfo.State, jobInfo.SourceFilePath, jobInfo.TargetFilePath, 
                        jobInfo.TotalFilesToCopy, jobInfo.NbFilesLeftToDo, jobInfo.TotalFilesSize);
                    
                    _jobs.Add(job);
                }
            }
        }

        /* Job Deletion by Name method */
        public void DeleteJob(string name)
        {
            var job = _jobs.FirstOrDefault(j => j.Name == name);
            if (job != null)
            {
                job.State = JobState.Retired;
                Logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
                _jobs.Remove(job);
            }
        }
        
        public void AddJob(string name, string source, string destination, BackupType backupType)
        {
            var job = new Job(name, backupType, source, destination);
            UpdateJobData(name, job);
            _jobs.Add(job);
        }

        private void LaunchJob(Job job, BackupProcess backupProcess)
        {
            if (job.State == JobState.Paused)
            {
                ResumeJob(job);
                return;
            }
            
            if (job.State == JobState.Finished)
            {
                job.Progression = 0;
                job.NbSavedFiles = 0;
                job.NbFilesLeftToDo = job.TotalFilesToCopy;
            }
            
            job.State = JobState.Active;
            
            Logger.LogAction(job.Name, job.SourceFilePath, job.TargetFilePath, 0, TimeSpan.Zero);
            Logger.LogState(job);
            _copyController.CopyDirectory(job);
        }

        private void ResumeJob(Job job)
        {
            throw new NotImplementedException();
        }

        public async void LaunchJobAsync(Job job, BackupProcess backupProcess)
        {
            await Task.Run(() => LaunchJob(job, backupProcess));
        }
        
        
        /* Update Job data in state.json -> Add()/Edit() */
        // TOFIX: Redundancy => Search for job by id instead of name
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

            Logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), newName);
        }

        bool ContainsDuplicates(Array array)
        {
            HashSet<object> set = new HashSet<object>();
            foreach (var item in array)
            {
                if (!set.Add(item))
                {
                    return true; 
                }
            }
            return false;
        }
        private bool JobExists(string name)
        {
            foreach (var job in _jobs)
            {
                if (job.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
        
    }
}
