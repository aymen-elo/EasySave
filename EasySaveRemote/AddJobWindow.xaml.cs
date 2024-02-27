using System.Windows;
using EasySaveRemote.ViewModel;
using EasySaveLib.Model;

namespace EasySaveRemote
{
    public partial class AddJobWindow : Window
    {
        private readonly JobsViewModel _jobsViewModel;
        public AddJobWindow(JobsViewModel jobsViewModel)
        {
            InitializeComponent();
            _jobsViewModel = jobsViewModel;
        }

        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string jobName = txtJobName.Text;
            string sourcePath = txtSourcePath.Text;
            string destinationPath = txtDestinationPath.Text;
            string backupType = cmbBackupType.SelectedItem.ToString();
            BackupType typeSave = (backupType.Contains("Full")) ? BackupType.Full : BackupType.Diff;
            _jobsViewModel.AddJob( jobName, sourcePath, destinationPath, typeSave);
            
            this.Close();
        }
    }
}