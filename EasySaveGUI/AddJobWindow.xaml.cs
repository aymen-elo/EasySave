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
            // Récupérer les valeurs saisies par l'utilisateur
            string jobName = txtJobName.Text;
            string sourcePath = txtSourcePath.Text;
            string destinationPath = txtDestinationPath.Text;
            string backupType = cmbBackupType.SelectedItem.ToString(); // Récupérer le type de sauvegarde sélectionné

            // Transmettre les valeurs à votre JobsController pour l'ajout du job à la liste des jobs
            JobsController jobsController = new JobsController(_logger);

            // Convertir la chaîne de type de sauvegarde en BackupType
            BackupType typeSave = (backupType == "Full") ? BackupType.Full : BackupType.Diff;

            // Ajouter le nouveau job en utilisant la méthode AddJob du JobsController
            jobsController.AddJob(_logger, _translation, jobName, sourcePath, destinationPath, typeSave);

            // Fermer la fenêtre
            this.Close();
        }
    }
}