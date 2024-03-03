using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveGUI.View;
using EasySaveLib.Model;

namespace EasySaveGUI.ViewModel
{
    /// <summary>
    /// Class following the MVVM through Add and Edit Job commands
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public JobsViewModel JobsViewModel { get; set; }
        public ICommand AddJobShowCommand { get; set; }
        public ICommand EditJobShowCommand { get; set; }
        
        public MainViewModel(JobsViewModel jobsViewModel)
        {
            JobsViewModel = jobsViewModel;
            
            AddJobShowCommand = new RelayCommand(AddJobShow);
            EditJobShowCommand = new RelayCommand(EditJobShow);
        }

        /// <summary>
        /// Display the AddJobWindow
        /// </summary>
        /// <param name="param"></param>
        private void AddJobShow(object param)
        {
            if (param is not ObservableCollection<Job> jobs) return;
            
            AddJobWindow addJobWindow = new AddJobWindow(jobs);
            addJobWindow.ShowDialog();
        }
        
        /// <summary>
        /// Display the EditJobWindow
        /// </summary>
        /// <param name="param"></param>
        private void EditJobShow(object param)
        {
            if (param is not Job job) return;
            
            // Selected job -> Edit job (in new window)
            EditJobWindow editJobWindow = new EditJobWindow(job);
            editJobWindow.ShowDialog();
        }
    }
}