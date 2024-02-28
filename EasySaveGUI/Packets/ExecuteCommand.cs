﻿using System;
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
        private JobsViewModel _jobsViewModel;
        private FormatLog formatLog;

        public void NJExecute(string response)
        {
            dynamic jobData = JsonConvert.DeserializeObject(response);

            string jobName = jobData.JobName;
            string sourcePath = jobData.SourcePath;
            string destinationPath = jobData.DestinationPath;
            BackupType typeSave = (jobData.TypeSave == "Full") ? BackupType.Full : BackupType.Diff;
            
            _jobsViewModel.AddJob(jobName, sourcePath, destinationPath, typeSave);
        }
        
        public void DJExecute(string response)
        {
                        
            List<Dictionary<string, object>>? dataList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
            Dictionary<string, object> firstEntry = dataList.FirstOrDefault();
            string? name = firstEntry?["Name"]?.ToString();
            if (dataList.Count > 1)
            {
                var jobData = dataList[1];
                            
                Job job = new Job
                {
                    Name = jobData["Name"]?.ToString(),
                    BackupType = (BackupType)Enum.Parse(typeof(BackupType), jobData["BackupType"]?.ToString()),
                    SourceFilePath = jobData["SourceFilePath"]?.ToString(),
                    TargetFilePath = jobData["TargetFilePath"]?.ToString(),
                    State = (JobState)Enum.Parse(typeof(JobState), jobData["State"]?.ToString()),
                    TotalFilesToCopy = Convert.ToInt32(jobData["TotalFilesToCopy"]),
                    TotalFilesSize = Convert.ToInt64(jobData["TotalFilesSize"]),
                    NbFilesLeftToDo = Convert.ToInt64(jobData["NbFilesLeftToDo"]),
                    Progression = Convert.ToInt32(jobData["Progression"])
                };
            }

            // TODO : Edit Jobs
            // _jobsViewModel.EditJob(job);
        }

        public void RJExecute(string response)
        {
            var jobs = JsonConvert.DeserializeObject<ObservableCollection<Job>>(response);
                        
            foreach (var j in jobs)
            {
                BackupProcess backupProcess = new BackupProcess(j);
                _jobsViewModel.LaunchJobAsync(j, backupProcess);
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