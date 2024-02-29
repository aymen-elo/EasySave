using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasySaveGUI.Model;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;
using Newtonsoft.Json;

namespace EasySaveGUI.Packets
{
    public class ExecuteCommand
    {
        private static Logger _logger = new Logger();
        private JobsViewModel _jobsViewModel = new JobsViewModel(_logger);
        private Option _option;
        private EditJobViewModel _editJobViewModel;
        private AddJobViewModel _addJobWindow;

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

        public async void RJExecute(string response)
        {
            var jobs = JsonConvert.DeserializeObject<ObservableCollection<Job>>(response);

            foreach (var j in jobs)
            {
                BackupProcess backupProcess = new BackupProcess(j);
                await _jobsViewModel.LaunchJobAsync(j, backupProcess);
            }
        }

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