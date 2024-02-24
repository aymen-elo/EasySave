using System.Collections.Generic;
using System.Collections.ObjectModel;
using EasySaveGUI.Controller;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveGUI
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