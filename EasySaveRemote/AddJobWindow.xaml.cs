using System.Windows;
using EasySaveRemote.ViewModel;
using EasySaveLib.Model;
using EasySaveRemote.Packets;
using Newtonsoft.Json;

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
            
            var jobData = new
            {
                JobName = txtJobName.Text,
                SourcePath = txtSourcePath.Text,
                DestinationPath = txtDestinationPath.Text,
                BackupType = cmbBackupType.SelectedItem.ToString(),
                TypeSave = (cmbBackupType.SelectedItem.ToString().Contains("Full")) ? "Full" : "Diff"
            };

            var jsonData = JsonConvert.SerializeObject(jobData);
            
            SendMessage.SendMessageTo("127.0.0.1", 13,jsonData, MessageType.NJ);
            
            this.Close();
        }
    }
}