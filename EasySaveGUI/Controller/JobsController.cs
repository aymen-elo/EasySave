using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using EasySaveLib.Model;
using System.Threading.Tasks;
using EasySaveGUI.Model;
using Newtonsoft.Json;

namespace EasySaveGUI.Controller
{
    public class JobsController
    { 
        public List<Job> Jobs { get; private set; }
        public Logger Logger { get; set; }
        public event EventHandler<string> FileSaved;
        private CopyController _copyController;
        public ObservableCollection<Job> JobsCollection { get; set; }
        
        public JobsController(Logger logger)
        {
            Jobs = new List<Job>();
            JobsCollection = new ObservableCollection<Job>();
            Logger = logger;
            _copyController = new CopyController();
            Initialize();
        }

        public List<Job> GetJobs()
        {
            return Jobs;
        }

        private void Initialize()
        {
            FileSaved += HandleFileSaved;
            
            /* Handling the already existing backup jobs (if they exist) */
            // Checking if the State logging file exists
            var stateFile = Logger.LogsDirectoryPath + @"\state.json";
            if (File.Exists(stateFile) & new FileInfo(stateFile).Length != 0)
            {
                string jsonContent = File.ReadAllText(stateFile);
                var content = JsonConvert.DeserializeObject<List<Job>>(jsonContent);

                foreach (var jobInfo in content)
                {
                    // When the job is not taken account of -> we ignore it because it's not a Job anymore 
                    if (jobInfo.State == JobState.Retired) { continue; }
                    
                    // Construct a new Job with the available data in the json and append it to the Jobs List
                    var job = new Job(jobInfo.Name, jobInfo.State, jobInfo.SourceFilePath, jobInfo.TargetFilePath, 
                        jobInfo.TotalFilesToCopy, jobInfo.NbFilesLeftToDo, jobInfo.TotalFilesSize);
                    
                    Jobs.Add(job);
                    JobsCollection.Add(job);
                }
            }
        }

        private void HandleFileSaved(object sender, string fileName)
        {
            Logger.LogAction(fileName, "", "", 0, TimeSpan.Zero);
        }

        // Editing a job that exists -> used in another method EditJob(logger, translation)
        public void EditJob(int index, string name, string source, string destination, BackupType backupType)
        {
            Job job = Jobs[index];
            if (job != null)
            {
                job.SourceFilePath = source;
                job.TargetFilePath = destination;
                job.BackupType = backupType;
                
                UpdateJobData(name, job);                
                // Renaming for the current job object (LogState Constaints)
                job.Name = name;
            }
        }
        
        public void DeleteJob(int index)
        {
            var job = Jobs[index];

            job.State = JobState.Retired;
            Logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
            Jobs.Remove(job);
        }

        /* Variant deletion by Name method */
        public void DeleteJob(string name)
        {
            var job = Jobs.Find(j => j.Name == name);
            if (job != null)
            {
                job.State = JobState.Retired;
                Logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
                Jobs.Remove(job);
                JobsCollection.Remove(job);
            }
        }
        
        public void AddJob(string name, string source, string destination, BackupType backupType)
        {
            var job = new Job(name, backupType, source, destination);
            UpdateJobData(name, job);
            Jobs.Add(job);
            JobsCollection.Add(job);
        }

        private void LaunchJob(Job job, BackupProcess backupProcess)
        {
            if (job.State == JobState.Finished || job.State == JobState.Pending)
            {
                job.Progression = 0;
                job.State = JobState.Active;
                job.NbFilesLeftToDo = job.TotalFilesToCopy;
                
                backupProcess.StartBackup();

            }
            
            Logger.LogAction(job.Name, job.SourceFilePath, job.TargetFilePath, 0, TimeSpan.Zero);
            
            _copyController.CopyDirectory(job);
            
            if (job.Progression >= 100)
            {
                job.State = JobState.Finished;
            }

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

            Logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), newName);
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
            foreach (var job in Jobs)
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
