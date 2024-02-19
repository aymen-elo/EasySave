using System;
using System.IO;
using System.Windows;
using EasySave.Controllers;
using EasySave.Library;
using EasySave.Models;

namespace EasySave_2._0
{
    public partial class AddJobWindow : Window
    {
        Logger _logger = new Logger();
        private TranslationModel _translation;

        public AddJobWindow()
        {
            InitializeComponent();
            _translation = new TranslationModel(); // Utilisez la variable de niveau de classe
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string jobName = txtJobName.Text;
                string sourcePath = txtSourcePath.Text;
                string destinationPath = txtDestinationPath.Text;
                string backupType =
                    cmbBackupType.SelectedItem?.ToString(); 

                // Vérifier si toutes les entrées sont valides
                if (string.IsNullOrWhiteSpace(jobName) || string.IsNullOrWhiteSpace(sourcePath) ||
                    string.IsNullOrWhiteSpace(destinationPath) || string.IsNullOrWhiteSpace(backupType))
                {
                    throw new ArgumentException("All fields must be completed.");
                }
                
                if (!Directory.Exists(sourcePath) || !Directory.Exists(destinationPath))
                {
                    throw new ArgumentException("Les chemins source et/ou destination sont invalides.");
                }

                JobsController jobsController = new JobsController(_logger);

                BackupType typeSave = (backupType == "Full") ? BackupType.Full : BackupType.Diff;

                jobsController.AddJob(_logger, _translation, jobName, sourcePath, destinationPath, typeSave);

                this.Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Error : {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There is an error : {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}