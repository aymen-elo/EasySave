using System.Collections.Generic;
using System.Collections.ObjectModel;
using EasySaveRemote.ViewModel;
using EasySaveLib.Model;

namespace EasySaveRemote
{
    public class MainViewModel
    {
        public JobsViewModel JobsViewModel { get; set; }

        public MainViewModel(JobsViewModel jobsViewModel)
        {
            JobsViewModel = jobsViewModel;
        }
    }
}