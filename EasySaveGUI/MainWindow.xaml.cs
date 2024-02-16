
using EasySave.Controllers;
using EasySave.Models;

using System;
using System.Windows;
using EasySave.Library;

namespace EasySave_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JobsController _jobsController;
        private readonly Menu _Menu;
        private Logger _logger;

        public MainWindow()
        {
            InitializeComponent();
            _jobsController = new JobsController(_logger);

            string LogsDirectoryPath = @"C:\Prosoft\EasySave\Logs";

            
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
            AddJobWindow addJobWindow = new AddJobWindow();

            bool? result = addJobWindow.ShowDialog();

            if (result == true)
            {
                RefreshJobList();
            }
        }

        private void RefreshJobList()
        {
            // Vous pouvez mettre à jour la liste des jobs ici en fonction de la logique de votre application
            // Par exemple, si vous avez une liste ListBox nommée "lstJobs" dans votre interface utilisateur,
            // vous pouvez mettre à jour cette liste avec les nouveaux jobs ajoutés.
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
        }
        private void btnStopJob_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
