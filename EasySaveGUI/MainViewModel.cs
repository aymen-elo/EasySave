using System.Collections.ObjectModel;
using EasySaveGUI.Controller;
using EasySaveGUI.Model;
using EasySaveGUI.Model.Translation;

namespace EasySaveGUI
{
    public class MainViewModel
    {
        static Logger logger = Logger.GetInstance();
        TranslationController translationController = new TranslationController();
        TranslationManager translationManager = new TranslationManager();

        private JobsController _jobsController;
        public ObservableCollection<Job> JobsCollection { get; set; }

        public MainViewModel(JobsController jobsController)
        {
            _jobsController = jobsController;
            JobsCollection = jobsController.JobsCollection;
        }
    }
}