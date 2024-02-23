using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EasySaveGUI;
using EasySaveGUI.Controller;
using EasySaveGUI.Model;
using Job = EasySaveLib.Model.Job;
using Logger = EasySaveLib.Model.Logger;
using ConfigManager = EasySaveLib.Model.ConfigManager;

namespace EasySaveGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JobsController _jobsController;
        private string _currentLanguageCode;
        private ResourceDictionary _languageDictionary;
        private AddJobWindow addJobWindow;
        public LangueSettingsViewModels _language = new();
        private Logger _logger;
        public ConfigManager _configManager;
        private MainViewModel _mainViewModel;
        public static bool IsPaused;
        
        public SolidColorBrush backgroundColor =
            new SolidColorBrush(Color.FromArgb(255, (byte)233, (byte)238, (byte)243));

        public MainWindow()
        {
            _configManager = new ConfigManager();
            InitializeComponent();
            btnRunJob.IsEnabled = false;
            btnRemoveJob.IsEnabled = false;
            _logger = new Logger();
            _jobsController = new JobsController(_logger);
            _mainViewModel = new MainViewModel(_jobsController);
            DataContext = _mainViewModel;
            string logsDirectoryPath = @"C:\Prosoft\EasySave\Logs";
            _languageDictionary = new ResourceDictionary();
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
            AddJobWindow addJobWindow = new AddJobWindow(_jobsController);
            addJobWindow.ShowDialog();
            RefreshJobList();
        }

        public void RefreshJobList()
        {
            var logger = new Logger();
            var j = new JobsController(logger);
            dgJobList.ItemsSource = null;
            dgJobList.ItemsSource = _jobsController.JobsCollection;
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            FormatLog optionWindow = new FormatLog();
            optionWindow.ShowDialog();
        }

        
        /* Language Management */

        //private void menuItemLang_Click(object sender, RoutedEventArgs e) { }


        /* ******************* */

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            List<int> selectedIndexes = new List<int>();
            foreach (var selectedItem in dgJobList.SelectedItems)
            {
                int index = dgJobList.Items.IndexOf(selectedItem);
                selectedIndexes.Add(index);
                Job selectedJob = (Job)dgJobList.Items[index];
                _jobsController.LaunchJobAsync(selectedJob);
            }
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobs = dgJobList?.SelectedItems.Cast<Job>().ToList();
            if (selectedJobs != null)
            {
                foreach (var job in selectedJobs)
                {
                    _jobsController.DeleteJob(job.Name);
                }
            }

            RefreshJobList();
        }

        private void btnEditJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPaused)
            {
                IsPaused = true;
                btnPlayPause.Background = Brushes.Red;
            }
            else
            {
                IsPaused = false;
                btnPlayPause.Background = backgroundColor;
            }
        }

        private void btnStopJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
        }

        private void dgJobList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Job job = (Job)(dgJobList.SelectedItem);
            if (job != null)
            {
                btnRunJob.IsEnabled = true;
                btnRemoveJob.IsEnabled = true;
            }
            else
            {
                btnRunJob.IsEnabled = false;
                btnRemoveJob.IsEnabled = false;
            }
        }
    }
}