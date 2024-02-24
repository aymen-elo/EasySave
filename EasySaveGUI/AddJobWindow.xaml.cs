using System.Windows;
using EasySaveGUI.Controller;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveGUI
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
            BackupType typeSave = (backupType == "Full") ? BackupType.Full : BackupType.Diff;
            _jobsViewModel.AddJob( jobName, sourcePath, destinationPath, typeSave);
            
            this.Close();
        }
    }
}