using System.Collections.ObjectModel;
using EasySaveGUI.Controller;
using EasySaveGUI.Model;
using EasySaveGUI.Model.Translation;

namespace EasySaveGUI
{
    public class MainViewModel
    {
        static Logger logger = Logger.GetInstance();
        JobsController jobsController = new JobsController(logger);
        TranslationController translationController = new TranslationController();
        TranslationManager translationManager = new TranslationManager();

        public ObservableCollection<Job> JobsCollection { get; set; }

        public MainViewModel()
        {
            JobsCollection = new ObservableCollection<Job>();
            foreach (var travail in jobsController.GetJobs())
            {
                JobsCollection.Add(new Job(travail.Name, travail.BackupType, travail.SourceFilePath, travail.TargetFilePath));
            }
        }
    }
}