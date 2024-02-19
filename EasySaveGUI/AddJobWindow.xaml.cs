using System;
using System.Windows;
using EasySave.Controllers;
using EasySave.Library;
using EasySave.Models;
using EasySave_2._0;

namespace EasySave_2._0
{
    public partial class AddJobWindow : Window
    {
        Logger _logger = new Logger();
        private TranslationModel _translation;


        public AddJobWindow()
        {
            InitializeComponent();
            _translation = new TranslationModel();

        }

        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string jobName = txtJobName.Text;
            string sourcePath = txtSourcePath.Text;
            string destinationPath = txtDestinationPath.Text;
            string backupType = cmbBackupType.SelectedItem.ToString();
            JobsController jobsController = new JobsController(_logger);
            BackupType typeSave = (backupType == "Full") ? BackupType.Full : BackupType.Diff;
            jobsController.AddJob(_logger, _translation, jobName, sourcePath, destinationPath, typeSave);
            
            this.Close();
        }
    }
}