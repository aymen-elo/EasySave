using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

using System.Windows.Controls;
using EasySaveGUI;
using EasySaveGUI.Controller;
using EasySaveGUI.Model;
using EasySaveGUI.Model.Translation;
using Menu = EasySaveGUI.Model.Menu;

namespace EasySaveGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JobsController _jobsController;
        private readonly Menu _menu;
        private readonly TranslationController _translationController;
        private string _currentLanguageCode;
        private ResourceDictionary _languageDictionary;
        private AddJobWindow addJobWindow;
        public LangueSettingsViewModels _language = new();
        Logger _logger = new Logger();
        public ConfigManager _configManager;
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            _configManager = new ConfigManager();
            InitializeComponent();
            btnRunJob.IsEnabled = false;
            btnRemoveJob.IsEnabled = false;
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
                _jobsController.LaunchJob(selectedJob);
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
        private void btnEditJob_Click(object sender, RoutedEventArgs e) { }
        private void btnPlayPause_Click(object sender, RoutedEventArgs e) { }
        private void btnStopJob_Click(object sender, RoutedEventArgs e) { }
        private void btnLogs_Click(object sender, RoutedEventArgs e) { }

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
