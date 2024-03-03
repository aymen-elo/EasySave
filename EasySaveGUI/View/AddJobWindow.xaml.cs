using System.Collections.ObjectModel;
using System.Windows;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveGUI.View
{
    public partial class AddJobWindow : Window
    {
        private readonly AddJobViewModel _addJobViewModel;

        public AddJobWindow(ObservableCollection<Job> jobs)
        {
            InitializeComponent();
            _addJobViewModel = new AddJobViewModel(jobs);            
            DataContext = _addJobViewModel;
            _addJobViewModel.RequestClose += Close;
        }

    }
}