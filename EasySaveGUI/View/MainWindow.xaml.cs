using System.Linq;
using System.Windows;
using System.Windows.Media;
using EasySaveGUI.ViewModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using Job = EasySaveLib.Model.Job;
using Logger = EasySaveLib.Model.Logger;

namespace EasySaveGUI.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        ///     Indicate the Job states.
        /// </summary>
        public static bool IsPaused;
        public static bool IsStopped;

        /// <summary>
        ///     ViewModel for managing Jobs list.
        /// </summary>
        public readonly JobsViewModel JobsViewModel;

        public readonly SolidColorBrush BackgroundColor = new(Color.FromArgb(255, 221, 221, 221));

        /// <summary>
        ///     Server instance for handling network requests.
        /// </summary>
        private readonly Server.Server _server;

        /// <summary>
        ///     Constructor for MainWindow.
        ///     Initializes the window and starts the server.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            btnRunJob.IsEnabled = false;
            btnRemoveJob.IsEnabled = false;
            
            var logger = new Logger();
            JobsViewModel = new JobsViewModel(logger);
            var mainViewModel = new MainViewModel(JobsViewModel);
            DataContext = mainViewModel;
            
            _server = new Server.Server(this);
            LaunchServerAsync();
        }

        /// <summary>
        ///     Event handler for the options button click event.
        ///     Opens the options window.
        /// </summary>
        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            var optionWindow = new OptionWindow();
            optionWindow.ShowDialog();
        }

        /// <summary>
        ///     Event handler for the run job button click event.
        ///     Runs the selected jobs.
        /// </summary>
        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndexes = new List<int>();
            foreach (var selectedItem in dgJobList.SelectedItems)
            {
                var index = dgJobList.Items.IndexOf(selectedItem);
                selectedIndexes.Add(index);
                var selectedJob = (Job)dgJobList.Items[index];

                JobsViewModel.LaunchJobAsync(selectedJob);
            }
        }

        /// <summary>
        ///     Event handler for the remove job button click event.
        ///     Removes the selected jobs.
        /// </summary>
        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobs = dgJobList?.SelectedItems.Cast<Job>().ToList();
            if (selectedJobs != null)
                foreach (var job in selectedJobs)
                    JobsViewModel.DeleteJob(job.Name);
        }

        /// <summary>
        ///     Event handler for the play/pause button click event.
        ///     Toggles the paused state of the application.
        /// </summary>
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
                btnPlayPause.Background = BackgroundColor;
            }
        }

        /// <summary>
        ///     Event handler for the stop job button click event.
        ///     Toggles the stopped state of the application.
        /// </summary>
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
                btnStopJob.Background = BackgroundColor;
            }
        }

        /// <summary>
        ///     Event handler for the job list selection changed event.
        ///     Enables or disables the run and remove job buttons based on the selection.
        /// </summary>
        private void dgJobList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var job = (Job)dgJobList.SelectedItem;
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

        /// <summary>
        ///     Launches the server asynchronously.
        /// </summary>
        public async void LaunchServerAsync()
        {
            await Task.Run(() => _server.Serve());
        }
    }
}