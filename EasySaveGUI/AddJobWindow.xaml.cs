using System.Windows;
using EasySaveGUI.Controller;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;
using System.Windows.Forms;

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
        private void btnOpenSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderBrowserDialog(txtSourcePath);
        }

        private void btnOpenDestination_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderBrowserDialog(txtDestinationPath);
        }

        private void OpenFolderBrowserDialog(System.Windows.Controls.TextBox textBox)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    textBox.Text = dialog.SelectedPath;
                }
            }
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