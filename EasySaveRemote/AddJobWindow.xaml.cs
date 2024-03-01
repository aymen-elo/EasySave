using System.Collections.ObjectModel;
using System.Windows;
using EasySaveRemote.ViewModel;
using EasySaveLib.Model;
using EasySaveRemote.Packets;
using Newtonsoft.Json;

namespace EasySaveRemote
{
    public partial class AddJobWindow : Window
    {
        private readonly AddJobViewModel _addJobViewModel;

        public AddJobWindow(ObservableCollection<Job> jobs, MainWindow mainWindow)
        {
            InitializeComponent();
            _addJobViewModel = new AddJobViewModel(jobs, mainWindow);            
            DataContext = _addJobViewModel;
            _addJobViewModel.RequestClose += Close;
        }
    }
}