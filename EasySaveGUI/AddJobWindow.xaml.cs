using System.Windows;
using EasySaveGUI.Controller;
using EasySaveGUI.Model;
using EasySaveGUI.Model.Translation;

namespace EasySaveGUI
{
    public partial class AddJobWindow : Window
    {
        Logger _logger = new Logger();
        private TranslationModel _translation;
        private JobsController _jobsController;
        public AddJobWindow(JobsController jobsController)
        {
            InitializeComponent();
            _translation = new TranslationModel();
            _jobsController = jobsController;
        }

        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string jobName = txtJobName.Text;
            string sourcePath = txtSourcePath.Text;
            string destinationPath = txtDestinationPath.Text;
            string backupType = cmbBackupType.SelectedItem.ToString();
            BackupType typeSave = (backupType == "Full") ? BackupType.Full : BackupType.Diff;
            _jobsController.AddJob( jobName, sourcePath, destinationPath, typeSave);
            
            this.Close();
        }
    }
}