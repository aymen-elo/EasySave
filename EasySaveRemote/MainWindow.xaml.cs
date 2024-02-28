﻿using System;
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
        public FormatLog _formatLog = new FormatLog();


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
            _jobsViewModel = new JobsViewModel(_logger);
            _mainViewModel = new MainViewModel(_jobsViewModel);
            string logsDirectoryPath = @"C:\Prosoft\EasySave\Logs";
            _languageDictionary = new ResourceDictionary();
            _sendMessageInstance = new SendMessage();
            DataContext = this;


        }

        public async void firstConnection(string ipAddress, int port)
        {
            IPEndPoint ipEndPoint = new(IPAddress.Parse(ipAddress), port);
            client = new Socket(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            await client.ConnectAsync(ipEndPoint);
            
            /* Reception 1ere partie data */
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.GAJ, this);
            
            /* Reception 2e partie data */
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.GAJ, this);

            
        }


        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
            AddJobWindow addJobWindow = new AddJobWindow(_jobsViewModel);
            addJobWindow.ShowDialog();
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            FormatLog optionWindow = new FormatLog(logFormat, encryptionKey, cipherList, priorityList, bigFileSize, this);
            optionWindow.ShowDialog();
        }

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Get selected job list -> message
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.RJ);
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Get selected job.name -> message
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.DJ);
        }

        private void btnEditJob_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Open edit

        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.PP);
        }

        private void btnStopJob_Click(object sender, RoutedEventArgs e)
        {
            SendMessage.SendMessageTo(ipAddress, port,"", MessageType.SJ);
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