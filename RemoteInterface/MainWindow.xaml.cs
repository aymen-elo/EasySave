using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EasySaveLib.Model;

// using EasySave.Model.Remote;
// using EasySave_RemoteClient.src;

namespace RemoteInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int progress;
        private ResourceDictionary _languageDictionary;
        
        private async Task StartAsync()
        {
            
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clientSide();
        }

        private async Task clientSide()
        {
            Server server = new Server(); 
            server.StartAsync(progress);
            
            Client client = new Client();
            await client.ConnectAsync();
        }
        
        
        public MainWindow()
        {
            InitializeComponent();
            btnRunJob.IsEnabled = false;
            btnRemoveJob.IsEnabled = false;
            _languageDictionary = new ResourceDictionary();
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
            AddJobWindow addJobWindow = new AddJobWindow();
            addJobWindow.ShowDialog();
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            // foreach (var j in _jobsViewModel.Jobs)
            // {
            //     j.NbSavedFiles = 100;
            //     j.TotalFilesToCopy = 100;
            // }
            
            FormatLog optionWindow = new FormatLog();
            optionWindow.ShowDialog();
        }
        
        
        /* Language Management */
        
        //private void menuItemLang_Click(object sender, RoutedEventArgs e) { }
        
        
        /* ******************* */

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
