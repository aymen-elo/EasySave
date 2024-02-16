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
        private Job _selectedJob;
        public static readonly Logger logger = Logger.GetInstance();
        private JobsController _jobsController = new JobsController(logger);
        private TranslationModel translation = new TranslationModel();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            btnRunJob.IsEnabled = false;

        }
        public Job SelectedJob
        {
            get { return _selectedJob; }
            set
            {
                _selectedJob = value;
                btnRunJob.IsEnabled = (_selectedJob != null);
            }
        }


        private void btnNewJob_Click(object sender, RoutedEventArgs e) { }

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            // Utilisez Dispatcher.Invoke pour créer ProgressBarGUI sur le thread STA
            this.Dispatcher.Invoke(() =>
            {
                ProgressBarGUI progressBar = new ProgressBarGUI();
                progressBar.ShowDialog();
            });
        }


        private void btnRemoveJob_Click(object sender, RoutedEventArgs e) { }
        private void btnEditJob_Click(object sender, RoutedEventArgs e) { }
        private void btnPlayPause_Click(object sender, RoutedEventArgs e) { }
        private void btnStopJob_Click(object sender, RoutedEventArgs e) { }
        private void btnLogs_Click(object sender, RoutedEventArgs e) { }

        private void dgJobList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgJobList.SelectedItem != null)
            {
                SelectedJob = (Job)dgJobList.SelectedItem;
            }
        }
    }
}
