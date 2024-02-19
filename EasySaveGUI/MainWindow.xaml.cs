using EasySave.Controllers;
using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasySave_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            btnRunJob.IsEnabled = false;

        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e) { }

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            Job selectedJob = dgJobList.SelectedItem as Job;
            if (selectedJob != null)
            {
                /*
                // Instancier le contrôleur de jobs
                JobsController jobsController = new JobsController();
        
                // Appeler la méthode LaunchJob avec le job sélectionné
                jobsController.LaunchJob(selectedJob, new Logger(), new TranslationModel());

                // Afficher un message ou effectuer toute autre action nécessaire
                */
                MessageBox.Show($"Job '{selectedJob.Name}' is running.");
            }
            else
            {
                // Afficher un message d'erreur si aucun job n'est sélectionné
                MessageBox.Show("Please select a job to run.");
            }
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
            var logger = new Logger();
            var translation = new TranslationModel();
            JobsController jobsController = new JobsController(logger);
            
            var job = dgJobList?.SelectedItem as Job;
            
            if (job != null)
            {
                jobsController.DeleteJob(dgJobList.Items.IndexOf(job), translation, logger);
            }
            
            dgJobList.ItemsSource = jobsController.GetJobs();
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
            }
            else
            {
                btnRunJob.IsEnabled = true;
            }
            // if several jobs are selected, the button is disabled
            if (dgJobList.SelectedItems.Count > 1 || dgJobList.SelectedItems.Count == 0)
            {
                btnRemoveJob.IsEnabled = false;
            }
            else
            {
                btnRemoveJob.IsEnabled = true;
            }
            
        }
    }
}
