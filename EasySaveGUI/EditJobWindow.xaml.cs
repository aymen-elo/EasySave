using System.Windows;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveGUI
{
    public partial class EditJobWindow : Window
    {
        private readonly EditJobViewModel _editJobViewModel;
        
        public EditJobWindow(Job job)
        {
            InitializeComponent();
            _editJobViewModel = new EditJobViewModel(job);
            DataContext = _editJobViewModel;
            
            _editJobViewModel.RequestClose += Close;
        }
    }
}