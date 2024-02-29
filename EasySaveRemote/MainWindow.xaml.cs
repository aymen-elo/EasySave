using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using EasySaveGUI.ViewModel;
using EasySaveRemote.ViewModel;
using Newtonsoft.Json;
using Job = EasySaveLib.Model.Job;
using Logger = EasySaveLib.Model.Logger;
using ConfigManager = EasySaveLib.Model.ConfigManager;
using EditJobViewModel = EasySaveRemote.ViewModel.EditJobViewModel;
using JobsViewModel = EasySaveRemote.ViewModel.JobsViewModel;

namespace EasySaveRemote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly EasySaveGUI.ViewModel.JobsViewModel _jobsViewModel;
        private string _currentLanguageCode;
        private ResourceDictionary _languageDictionary;
        private AddJobWindow addJobWindow;
        public LangueSettingsViewModels _language = new();
        private Logger _logger;
        public ConfigManager _configManager;
        private MainViewModel _mainViewModel;
        public Option Option = new Option();


        public SolidColorBrush backgroundColor =
            new SolidColorBrush(Color.FromArgb(255, (byte)233, (byte)238, (byte)243));

        private SendMessage _sendMessageInstance;
        public string ipAddress = "127.0.0.1";
        public int port = 13;
        
        public string language { get; set; }
        public string logFormat { get; set; }
        public string encryptionKey { get; set; }
        public string cipherList { get; set; }
        public string priorityList { get; set; }
        public string bigFileSize { get; set; }
        public Socket client;
        

        public MainWindow()
        {
            _configManager = new ConfigManager();
            InitializeComponent();
            btnRunJob.IsEnabled = false;
            btnRemoveJob.IsEnabled = false;
            _logger = new Logger();
            _jobsViewModel = new EasySaveGUI.ViewModel.JobsViewModel(_logger);
            _mainViewModel = new MainViewModel(_jobsViewModel, this);
            string logsDirectoryPath = @"C:\Prosoft\EasySave\Logs";
            _languageDictionary = new ResourceDictionary();
            _sendMessageInstance = new SendMessage();
            DataContext = this;
            SendMessage.JobsUpdated += JobsUpdatedHandler;
            
        }

        public async void firstConnection(string ipAddress, int port)
        {
            IPEndPoint ipEndPoint = new(IPAddress.Parse(ipAddress), port);
            client = new Socket(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            await client.ConnectAsync(ipEndPoint);

            refreshJO();
        }

        public void refreshJO()
        {
            /* Reception 1ere partie data */
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.GAJ, this);
            
            /* Reception 2e partie data */
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.GAJ, this);
        }

        private void JobsUpdatedHandler(ObservableCollection<Job> jobs)
        {
            // Mettez à jour la DataGrid avec la nouvelle collection de jobs
            dgJobList.ItemsSource = jobs;
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
         {
             ObservableCollection<Job> jobList = new ObservableCollection<Job>();
             foreach (Job j in dgJobList.SelectedItems)
             {
                 jobList.Add(j);
             }
             AddJobWindow addJobWindow = new AddJobWindow(jobList, this);
             addJobWindow.ShowDialog();
         }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            Option optionWindow = new Option(logFormat, encryptionKey, cipherList, priorityList, bigFileSize, this);
            optionWindow.ShowDialog();
        }

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Get selected job list -> message
            List<Job> jobList = new List<Job>();
            foreach (Job j in dgJobList.SelectedItems)
            {
                jobList.Add(j);
            }
            string jobListJson = JsonConvert.SerializeObject(jobList);
            
            SendMessage.SendMessageTo(ipAddress, port,jobListJson, MessageType.RJ, this);
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobs = dgJobList?.SelectedItems.Cast<Job>().ToList();
            string listJobToDelete = String.Empty;
            if (selectedJobs != null)
            {
                foreach (var job in selectedJobs)
                {
                    if (listJobToDelete == String.Empty)
                    {
                        listJobToDelete = job.Name;
                    }
                    else
                    {
                        listJobToDelete = string.Join(";", listJobToDelete, job.Name);
                    }

                }
            }
            SendMessage.SendMessageTo(ipAddress, port,listJobToDelete, MessageType.DJ, this);
        }

        private void btnEditJob_Click(object sender, RoutedEventArgs e)
        {
            List<Job> jobList = new List<Job>();
            foreach (Job j in dgJobList.SelectedItems)
            {
                jobList.Add(j);
            }
            EditJobWindow editJobWindow = new EditJobWindow(jobList.First(), this);
            editJobWindow.ShowDialog();
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.PP, this);
        }

        private void btnStopJob_Click(object sender, RoutedEventArgs e)
        {
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.SJ, this);
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