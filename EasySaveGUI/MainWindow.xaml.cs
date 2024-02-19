
using EasySave.Controllers;
using EasySave.Models;

using System;
using System.Globalization;
using System.Windows;

using System.Windows.Controls;
using Menu = EasySave.Library.Menu;

namespace EasySave_2._0
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

            bool? result = addJobWindow.ShowDialog();

            if (result == true)
            {
                RefreshJobList();
            }
        }

        private void RefreshJobList()
        {
            // Vous pouvez mettre à jour la liste des jobs ici en fonction de la logique de votre application
            // Par exemple, si vous avez une liste ListBox nommée "lstJobs" dans votre interface utilisateur,
            // vous pouvez mettre à jour cette liste avec les nouveaux jobs ajoutés.
        }

        private void btnRunJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnRemoveJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnEditJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnStopJob_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            OptionWindow optionWindow = new OptionWindow();
            optionWindow.ShowDialog();
        }



        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("en");

        }

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

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("fr");

        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
