using System.Windows;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveGUI
{
    public partial class AddJobWindow : Window
    {
        private readonly JobsViewModel _jobsViewModel;
        private readonly AddJobViewModel _viewModel;

        public AddJobWindow(JobsViewModel jobsViewModel)
        {
            InitializeComponent();
            _jobsViewModel = jobsViewModel;
            _viewModel = new AddJobViewModel();
            DataContext = _viewModel;
        }

        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string jobName = txtJobName.Text;
            string sourcePath = _viewModel.SourcePath;
            string destinationPath = _viewModel.DestinationPath;
            string backupType = cmbBackupType.SelectedItem?.ToString(); // Check for null
            BackupType typeSave = (backupType?.Contains("Full") == true) ? BackupType.Full : BackupType.Diff;
            _jobsViewModel.AddJob(jobName, sourcePath, destinationPath, typeSave);

            this.Close();
        }
    }
}