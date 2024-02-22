using System.Collections.ObjectModel;
using EasySaveGUI.Controller;
using EasySaveGUI.Model;

namespace EasySaveGUI
{
    public class MainViewModel
    {
        private JobsController _jobsController;
        public ObservableCollection<Job> JobsCollection { get; set; }

        public MainViewModel(JobsController jobsController)
        {
            _jobsController = jobsController;
            JobsCollection = jobsController.JobsCollection;
        }
    }
}