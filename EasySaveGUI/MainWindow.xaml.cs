using EasySave.Controllers;
using EasySave.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static EasySave_2._0.MainViewModel;

namespace EasySave_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Job _selectedJob;
        public static readonly Logger logger = Logger.GetInstance();
        private JobsController _jobsController = new JobsController(logger);
        private TranslationModel translation = new TranslationModel();
        private DataModel _dataModel = new DataModel();
        MainViewModel mainViewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;
            btnRunJob.IsEnabled = false;

        }

        public Job SelectedJob
        {
            get { return _selectedJob; }
            set
            {
                _selectedJob = value;
                btnRunJob.IsEnabled = (_selectedJob != null);
            }
        }
        private void dgJobList_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            if (dgJobList.SelectedItem != null)
            {
                SelectedJob = (Job)dgJobList.SelectedItem;
            }
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
            dgJobList.ItemsSource = _jobsController.GetJobs();

        }

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            List<int> selectedIndices = new List<int>();
            foreach (var selectedItem in dgJobList.SelectedItems)
            {
                int index = dgJobList.Items.IndexOf(selectedItem);
                selectedIndices.Add(index);
                Job selectedJob = (Job)dgJobList.Items[index];
                string message = string.Format($"La sauvegarde {selectedJob.Name} va commencer");
                eventList.ItemsSource = mainViewModel.PrintEvents(message);
                
                _jobsController.LaunchJob(selectedJob, logger, translation);
                
                message = string.Format($"La sauvegarde {0} viens de se finir", selectedJob.Name);
                eventList.ItemsSource = mainViewModel.PrintEvents(message);
            }
        }
        
        
        
        private void btnRemoveJob_Click(object sender, RoutedEventArgs e){}
        private void btnEditJob_Click(object sender, RoutedEventArgs e){}
        private void btnPlayPause_Click(object sender, RoutedEventArgs e){}
        private void btnStopJob_Click(object sender, RoutedEventArgs e) {}
        private void btnLogs_Click(object sender, RoutedEventArgs e) {}
        
    }
}
