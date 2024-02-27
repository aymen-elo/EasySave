using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveGUI.Controller;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveGUI
{
    public class MainViewModel : ViewModelBase
    {
        public JobsViewModel JobsViewModel { get; set; }
        public ICommand EditJobShowCommand { get; set; }
        
        public MainViewModel(JobsViewModel jobsViewModel)
        {
            JobsViewModel = jobsViewModel;
            EditJobShowCommand = new RelayCommand(EditJobShow);
        }

        private void EditJobShow(object param)
        {
            if (param is not Job job) return;
            
            // Selected job -> Edit job (in new window)
            EditJobWindow editJobWindow = new EditJobWindow(job);
            editJobWindow.ShowDialog();
        }
    }
}