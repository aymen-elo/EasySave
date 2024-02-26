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
using EasySaveGUI.ViewModel;
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
        private readonly JobsViewModel _jobsViewModel;
        private string _currentLanguageCode;
        private ResourceDictionary _languageDictionary;
        private AddJobWindow addJobWindow;
        public LangueSettingsViewModels _language = new();
        private Logger _logger;
        public ConfigManager _configManager;
        private MainViewModel _mainViewModel;
        public static bool IsPaused;
        public SolidColorBrush backgroundColor = new SolidColorBrush(Color.FromArgb(255, (byte)233, (byte)238, (byte)243));
        public MainWindow()
        {
            _configManager = new ConfigManager();
            InitializeComponent();
            btnRunJob.IsEnabled = false;
            btnRemoveJob.IsEnabled = false;
            _logger = new Logger();
            _jobsViewModel = new JobsViewModel(_logger);
            _mainViewModel = new MainViewModel(_jobsViewModel);
            DataContext = _mainViewModel;
            string logsDirectoryPath = @"C:\Prosoft\EasySave\Logs";
            _languageDictionary = new ResourceDictionary();
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
            AddJobWindow addJobWindow = new AddJobWindow(_jobsViewModel);
            addJobWindow.ShowDialog();
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            foreach (var j in _jobsViewModel.Jobs)
            {
                j.NbSavedFiles = 100;
                j.TotalFilesToCopy = 100;
            }
            
        
            Option optionWindow = new Option();
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
                
                BackupProcess backupProcess = new BackupProcess(selectedJob);
                _jobsViewModel.LaunchJobAsync(selectedJob, backupProcess);
            }
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobs = dgJobList?.SelectedItems.Cast<Job>().ToList();
            if (selectedJobs != null)
            {
                foreach (var job in selectedJobs)
                {
                    _jobsViewModel.DeleteJob(job.Name);
                }
            }
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

        public void cipherCryptoSoft(string sourcePath, string targetPath, string key)
        {
            string arguments = sourcePath + " " + targetPath + " " + key;
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "../../../../CryptoSoft/bin/Debug/net5.0/CryptoSoft.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
            process.Close();
        }
    }
}
