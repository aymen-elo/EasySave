using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EasySave.Controller;
using EasySave.Model;
using EasySaveGUI;
using EasySaveGUI.Controller;
using EasySaveLib.Model;
using EasySaveGUI.Model;
using EasySaveGUI.Packets;
using EasySaveGUI.ViewModel;
using Newtonsoft.Json;
using BackupType = EasySaveLib.Model.BackupType;
using Job = EasySaveLib.Model.Job;
using Logger = EasySaveLib.Model.Logger;
using ConfigManager = EasySaveLib.Model.ConfigManager;
using JobState = EasySaveLib.Model.JobState;

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
        public static bool IsStopped;
        public SolidColorBrush backgroundColor = new SolidColorBrush(Color.FromArgb(255, (byte)221, (byte)221, (byte)221));
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
            LaunchServerAsync();
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
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
            if (!IsStopped)
            {
                IsStopped = true;
                btnStopJob.Background = Brushes.Red;
            }
            else
            {
                IsStopped = false;
                btnStopJob.Background = backgroundColor;
            }
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
        
        public async void LaunchServerAsync()
        {
            await Task.Run(() => server());
        }

        private async void server()
        {
            ExecuteCommand executeCommand = new ExecuteCommand();
            try
            {
                IPEndPoint ipEndPoint = new(IPAddress.Parse("127.0.0.1"), 13);
                using Socket listener = new(
                    ipEndPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                List<string> prefixes = new List<string>
                    { "<|GAJ|>", "<|RJ|>", "<|EJ|>", "<|DJ|>", "<|NJ|>", "<|PP|>", "<|SJ|>", "<|MO|>" };


                listener.Bind(ipEndPoint);
                listener.Listen(100);

                var handler = await listener.AcceptAsync();
                string responseNoPrefix;

                while (true)
                {
                    var buffer = new byte[4_096];
                    // Receive message.
                    var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    while (received == 0)
                    {
                        received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    }
                    
                    var response = Encoding.UTF8.GetString(buffer, 0, received);

                    var eom = prefixes.FirstOrDefault(prefix => response.StartsWith(prefix));
                    switch (eom)
                    {
                        case var str when str == "<|GAJ|>":
                            /* Get All Jobs */
                            string statePath = @"C:\Prosoft\EasySave\Logs\state.json";
                            string stateContent = File.ReadAllText(statePath);
                            stateContent = "<|GAJ|>" + stateContent;
                            
                            var echoBytes = Encoding.UTF8.GetBytes(stateContent);
                            await handler.SendAsync(echoBytes, 0);

                            /* Wait until client ack */
                            await handler.ReceiveAsync(buffer, SocketFlags.None);
                            string conf =
                                $"{ConfigManager.GetSavedLanguage()};{ConfigManager.GetLogFormat()};{ConfigManager.GetEncryptionKey()};{ConfigManager.GetCipherList()};{ConfigManager.GetPriorityList()};{ConfigManager.GetBigFileSize()}";
                            conf = "<|Opt|>" + conf;
                            echoBytes = Encoding.UTF8.GetBytes(conf);
                            await handler.SendAsync(echoBytes, 0);
                            
                            break;

                        case var str when str == "<|RJ|>":
                            /* Run Jobs */
                            responseNoPrefix = response.Replace("<|RJ|>", "");
                            executeCommand.RJExecute(responseNoPrefix);
                            await handler.ReceiveAsync(buffer, SocketFlags.None);
                            break;


                        case var str when str == "<|EJ|>":
                            /* Edit Jobs */
                            responseNoPrefix = response.Replace("<|EJ|>", "");
                            executeCommand.EJExecute(responseNoPrefix);
                            break;


                        case var str when str == "<|DJ|>":
                            /* Delete Jobs */
                            responseNoPrefix = response.Replace("<|DJ|>", "");
                            _jobsViewModel.DeleteJob(responseNoPrefix);
                            break;

                        case var str when str == "<|NJ|>":
                            /* New Job */
                            responseNoPrefix = response.Replace("<|NJ|>", "");
                            executeCommand.NJExecute(responseNoPrefix);
                            break;


                        case var str when str == "<|PP|>":
                            /* Play/Pause a Job */
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
                            break;
                        case var str when str == "<|SJ|>":
                            /* Stop Running Jobs */
                            if (!IsStopped)
                            {
                                IsStopped = true;
                                btnStopJob.Background = Brushes.Red;
                            }
                            else
                            {
                                IsStopped = false;
                                btnStopJob.Background = backgroundColor;
                            }
                            break;
                        case var str when str == "<|MO|>":
                            /* Modify Option */
                            responseNoPrefix = response.Replace("<|MO|>", "");
                            executeCommand.MOExecute(responseNoPrefix);
                            break;
                        default:
                            /* Type not recognized */
                            break;
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
