using System;
using System.Collections.ObjectModel;
using EasySave.Models;
using Newtonsoft.Json;
using EasySave.Controllers;

namespace EasySave_2._0
{
    public class MainViewModel
    {
        static Logger logger = Logger.GetInstance();
        JobsController jobsController = new JobsController(logger);
        TranslationController translationController = new TranslationController();
        TranslationManager translationManager = new TranslationManager();
        
        private ObservableCollection<Job> _jobs;
        public ObservableCollection<Job> Jobs
        {
            get { return _jobs; }
            set
            {
                _jobs = value;
            }
        }

        public MainViewModel()
        {
            Jobs = new ObservableCollection<Job>();
            foreach (var travail in jobsController.GetJobs())
            {
                Jobs.Add(new Job(travail.Name, travail.BackupType, travail.SourceFilePath, travail.TargetFilePath));
            }
        }

        public MainViewModel refreshGrid()
        {
            MainViewModel mainViewModel = new MainViewModel();
            return mainViewModel;
        }
        
        public class DataModel
        {
            public string Name { get; set; }
            public string FileSource { get; set; }
            public string FileTarget { get; set; }
            public int FileSize { get; set; }
            public TimeSpan FileTransferTime { get; set; }
            public DateTime Time { get; set; }
        }
    }
}