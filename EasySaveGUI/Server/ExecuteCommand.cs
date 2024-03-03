using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasySaveGUI.View;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;
using Newtonsoft.Json;

namespace EasySaveGUI.Server
{
    /// <summary>
    /// Methods to execute Job commands received from the server
    /// </summary>
    public class ExecuteCommand
    {
        private static Logger _logger = new Logger();
        private JobsViewModel _jobsViewModel = new JobsViewModel(_logger);
        private OptionWindow _option;
        private EditJobViewModel _editJobViewModel;
        private AddJobViewModel _addJobWindow;

        /// <summary>
        /// New Job => Send Job data to AddJobViewModel
        /// </summary>
        /// <param name="response"></param>
        public void NJExecute(string response)
        {
            string name = null;
            string sourcePath = null;
            string destinationPath = null;
            BackupType jobType = BackupType.Full;
            
            var job = JsonConvert.DeserializeObject<Job>(response);
            name = job.Name;
            sourcePath = job.SourceFilePath;
            destinationPath = job.TargetFilePath;
            jobType = job.BackupType;

            Job newJob = new Job(name, jobType, sourcePath, destinationPath);
            ObservableCollection<Job> jobs = new ObservableCollection<Job>();

            jobs.Add(job);

            _addJobWindow = new AddJobViewModel(jobs);
        }

        /// <summary>
        /// Edit Job => Send Job data to EditJobViewModel
        /// </summary>
        /// <param name="response"></param>
        public void EJExecute(string response)
        {
            List<object>? deserializedData = JsonConvert.DeserializeObject<List<object>>(response);
            string jobJson = JsonConvert.SerializeObject(deserializedData?[1]);
            Job? job = JsonConvert.DeserializeObject<Job>(jobJson);

            List<Dictionary<string, object>>? dataList =
                JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
            Dictionary<string, object> firstEntry = dataList.FirstOrDefault();
            string? name = firstEntry?["OldName"]?.ToString();
            
            _editJobViewModel = new EditJobViewModel(job);
        }

        /// <summary>
        /// Run Job => Launch selected Job
        /// </summary>
        /// <param name="response"></param>
        public async void RJExecute(string response)
        {
            var jobs = JsonConvert.DeserializeObject<ObservableCollection<Job>>(response);

            foreach (var j in jobs)
            {
                await _jobsViewModel.LaunchJobAsync(j);
            }
        }

        /// <summary>
        /// Modify Options => Save new Options
        /// </summary>
        /// <param name="response"></param>
        public void MOExecute(string response)
        {
            string[] dataArray = response.Split(';');

            ConfigManager.SaveLanguage(dataArray[0]);
            ConfigManager.SaveLogFormat(dataArray[1]);
            ConfigManager.SaveEncryptionKey(dataArray[2]);
            ConfigManager.SaveCipherList(dataArray[3]);
            ConfigManager.SavePriorityList(dataArray[4]);
            ConfigManager.SaveBigFileSize(dataArray[5]);
        }
    }
}