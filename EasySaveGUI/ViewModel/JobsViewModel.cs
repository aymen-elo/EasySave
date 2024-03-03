using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using EasySaveLib.Model;
using System.Threading.Tasks;
using EasySaveGUI.Helper;
using Newtonsoft.Json;

namespace EasySaveGUI.ViewModel
{
    /// <summary>
    /// ViewModel for handling backup Jobs (Launch/Delete/Resume).
    /// This class contains the List of Jobs and some of the methods to handle them.
    /// </summary>
    public class JobsViewModel : ViewModelBase
    {
        /// <summary>
        /// the List representing backup Jobs.
        /// </summary>
        private readonly ObservableCollection<Job> _jobs;
        public ObservableCollection<Job> Jobs => _jobs;
        
        /// <summary>
        /// Logger for logging job states and actions.
        /// </summary>
        private Logger Logger { get; }
        
        /// <summary>
        /// Constructor for JobsViewModel.
        /// Initializes the jobs collection, logger and copy controller.
        /// </summary>
        public JobsViewModel(Logger logger)
        {
            _jobs = new ObservableCollection<Job>();
            Logger = logger;
            Initialize();
        }

        /// <summary>
        /// Initializes the jobs collection by retrieving existing backup jobs from the state file.
        /// </summary>
        private void Initialize()
        {
            var stateFile = Logger.LogsDirectoryPath + @"\state.json";
            if (File.Exists(stateFile) & new FileInfo(stateFile).Length != 0)
            {
                var jsonContent = File.ReadAllText(stateFile);
                var content = JsonConvert.DeserializeObject<ObservableCollection<Job>>(jsonContent);

                foreach (var jobInfo in content)
                {
                    var job = new Job(jobInfo.Name, jobInfo.BackupType, jobInfo.State, jobInfo.SourceFilePath, jobInfo.TargetFilePath, 
                        jobInfo.TotalFilesToCopy, jobInfo.NbFilesLeftToDo, jobInfo.TotalFilesSize);
                    
                    _jobs.Add(job);
                }
            }
        }

        /// <summary>
        /// Deletes a job by its name.
        /// </summary>
        /// <param name="name">The name of the job to delete.</param>
        public void DeleteJob(string name)
        {
            var job = _jobs.FirstOrDefault(j => j.Name == name);
            if (job != null)
            {
                job.State = JobState.Retired;
                Logger.LogState(job);
                _jobs.Remove(job);
                
                Logger.RemoveRetiredJobs();
                IdentityManager identityManager = new IdentityManager();
                identityManager.DeleteAllowedHashes(job.Name);
            }
        }

        /// <summary>
        /// Launches a job.
        /// </summary>
        /// <param name="job">The job to launch.</param>
        private void LaunchJob(Job job)
        {
            CopyController localCopyController = new CopyController();
            
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
            localCopyController.CopyDirectory(job);
        }

        /// <summary>
        /// Resumes a paused job.
        /// </summary>
        /// <param name="job">The job to resume.</param>
        private void ResumeJob(Job job)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously launches a job.
        /// </summary>
        /// <param name="job">The job to launch.</param>
        public async Task LaunchJobAsync(Job job)
        {
            await Task.Run(() => LaunchJob(job));
        }
    }
}