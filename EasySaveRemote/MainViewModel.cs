using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveGUI.Controller;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveRemote
{
    public class MainViewModel : ViewModelBase
    {
        public JobsViewModel JobsViewModel { get; set; }
        private MainWindow mainWindow;
        public ICommand AddJobShowCommand { get; set; }
        public ICommand EditJobShowCommand { get; set; }

        public MainViewModel(JobsViewModel jobsViewModel, MainWindow _mainWindow)
        {
            JobsViewModel = jobsViewModel;
            mainWindow = _mainWindow;
            
            AddJobShowCommand = new RelayCommand(AddJobShow);

            EditJobShowCommand = new RelayCommand(EditJobShow);
        }

        private void AddJobShow(object param)
        {
            if (param is not ObservableCollection<Job> jobs) return;


            AddJobWindow addJobWindow = new AddJobWindow(jobs, mainWindow);
            addJobWindow.ShowDialog();
        }

        private void EditJobShow(object param)
        {
            if (param is not Job job) return;

            // Selected job -> Edit job (in new window)
            EditJobWindow editJobWindow = new EditJobWindow(job, mainWindow);
            editJobWindow.ShowDialog();
        }
    }
}