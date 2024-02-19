using System;
using System.Globalization;
using System.Windows;

using System.Windows.Controls;
using EasySaveGUI.Controller;
using EasySaveGUI.Model;
using EasySaveGUI.Model.Translation;
using Menu = EasySaveGUI.Model.Menu;

namespace EasySaveGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JobsController _jobsController;
        private readonly Menu _Menu;
        private Logger _logger;
        private TranslationController _translationController;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            btnRunJob.IsEnabled = false;
            SwitchLanguage("fr");
            _jobsController = new JobsController(_logger);
            string LogsDirectoryPath = @"C:\Prosoft\EasySave\Logs";
            // Définir la langue par défaut en fonction de la langue du PC
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;
        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e)
        {
            AddJobWindow addJobWindow = new AddJobWindow();
            addJobWindow.ShowDialog();
            RefreshJobList();
        }

        public void RefreshJobList()
        {
            var logger = new Logger();
            JobsController jobsController = new JobsController(logger);
            dgJobList.ItemsSource = jobsController.GetJobs();
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            OptionWindow optionWindow = new OptionWindow();
            optionWindow.ShowDialog();
        }
        
        
        /* Language Management */
        
        private void menuItemLang_Click(object sender, RoutedEventArgs e) { }
        private void menuItemLangFr_Click(object sender, RoutedEventArgs e) { SwitchLanguage("fr"); }
        private void menuItemLangEn_Click(object sender, RoutedEventArgs e) { SwitchLanguage("en"); }
        private void SwitchLanguage(string languageCode) 
        {
            ResourceDictionary dictionary = new ResourceDictionary();
            switch (languageCode)
            {
                case "en":
                    dictionary.Source = new Uri("..\\StringResources.en.xaml", UriKind.Relative);
                    break;

                case "fr":
                    dictionary.Source = new Uri("..\\StringResources.fr.xaml", UriKind.Relative);
                    break;
                default:
                    dictionary.Source = new Uri("..\\StringResources.en.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dictionary);
      
        }
        
        /* ******************* */

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
            Job selectedJob = dgJobList.SelectedItem as Job;
            if (selectedJob != null)
            {
                /*
                // Instancier le contrôleur de jobs
                JobsController jobsController = new JobsController();
        
                // Appeler la méthode LaunchJob avec le job sélectionné
                jobsController.LaunchJob(selectedJob, new Logger(), new TranslationModel());

                // Afficher un message ou effectuer toute autre action nécessaire
                */
                MessageBox.Show($"Job '{selectedJob.Name}' is running.");
            }
            else
            {
                // Afficher un message d'erreur si aucun job n'est sélectionné
                MessageBox.Show("Please select a job to run.");
            }
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
            var logger = new Logger();
            var translation = new TranslationModel();
            JobsController jobsController = new JobsController(logger);
            
            var job = dgJobList?.SelectedItem as Job;
            
            if (job != null)
            {
                jobsController.DeleteJob(dgJobList.Items.IndexOf(job));
            }
            
            RefreshJobList();
        }
        private void btnEditJob_Click(object sender, RoutedEventArgs e) { }
        private void btnPlayPause_Click(object sender, RoutedEventArgs e) { }
        private void btnStopJob_Click(object sender, RoutedEventArgs e) { }
        private void btnLogs_Click(object sender, RoutedEventArgs e) { }

        private void dgJobList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Job job = (Job)(dgJobList.SelectedItem);
            if (job != null)
            {
                btnRunJob.IsEnabled = true;
            }
            else
            {
                btnRunJob.IsEnabled = true;
            }
            // if several jobs are selected, the button is disabled
            if (dgJobList.SelectedItems.Count > 1 || dgJobList.SelectedItems.Count == 0)
            {
                btnRemoveJob.IsEnabled = false;
            }
            else
            {
                btnRemoveJob.IsEnabled = true;
            }
            
        }
    }
}
