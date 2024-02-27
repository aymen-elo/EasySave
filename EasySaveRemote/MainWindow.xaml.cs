using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Media;
using EasySaveRemote.Model;
using EasySaveRemote.Packets;
using EasySaveRemote.ViewModel;
using Job = EasySaveLib.Model.Job;
using Logger = EasySaveLib.Model.Logger;
using ConfigManager = EasySaveLib.Model.ConfigManager;

namespace EasySaveRemote
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
        public SolidColorBrush backgroundColor = new SolidColorBrush(Color.FromArgb(255, (byte)233, (byte)238, (byte)243));
        private SendMessage _sendMessageInstance;
        
        public MainWindow()
        {
            _configManager = new ConfigManager();
            InitializeComponent();
            btnRunJob.IsEnabled = false;
            btnRemoveJob.IsEnabled = false;
            _logger = new Logger();
            _jobsViewModel = new JobsViewModel(_logger);
            _mainViewModel = new MainViewModel(_jobsViewModel);
            string logsDirectoryPath = @"C:\Prosoft\EasySave\Logs";
            _languageDictionary = new ResourceDictionary();
            _sendMessageInstance = new SendMessage();
            DataContext = this;

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
            
            FormatLog optionWindow = new FormatLog();
            optionWindow.ShowDialog();
        }

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnEditJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            SendMessage.SendMessageTo("127.0.0.1", 13, "hello mi friend", MessageType.GAJ);
             
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
