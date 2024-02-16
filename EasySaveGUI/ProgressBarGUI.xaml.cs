using System;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EasySave.Controllers;
using EasySave.Models;

namespace EasySave_2._0
{
    public partial class ProgressBarGUI : Window
    {
        private Job _selectedJob;
        public static readonly Logger logger = Logger.GetInstance();
        private JobsController _jobsController = new JobsController(logger);
        private TranslationModel translation = new TranslationModel();
        private BackgroundWorker _worker;

        public ProgressBarGUI()
        {
            InitializeComponent();
        }
        
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
    
            // Utilisez Dispatcher.Invoke pour accéder à MainWindow depuis le thread de l'interface utilisateur
            Dispatcher.Invoke(() => { worker.RunWorkerAsync(); });
        }
        
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            Job selectedJob = (Job)mainWindow.dgJobList.SelectedItem;

            List<int> selectedIndices = new List<int>();
            foreach (var selectedItem in mainWindow.dgJobList.SelectedItems)
            {
                int index = mainWindow.dgJobList.Items.IndexOf(selectedItem);
                selectedIndices.Add(index);
            }

            foreach (int index in selectedIndices)
            {
                selectedJob = (Job)mainWindow.dgJobList.Items[index];

                // Utilisez Dispatcher.Invoke pour exécuter le code sur le thread STA
                this.Dispatcher.Invoke(() =>
                {
                    _jobsController.LaunchJob(selectedJob, logger, translation, _worker);
                });
            }
        }


        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }
        
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbCopy.Value = e.ProgressPercentage;
        }
    }
}