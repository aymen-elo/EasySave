using System.Windows;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;
using EasySaveRemote.Packets;
using EasySaveRemote.ViewModel;
using EditJobViewModel = EasySaveRemote.ViewModel.EditJobViewModel;

namespace EasySaveRemote
{
    public partial class EditJobWindow : Window
    {
        private readonly EditJobViewModel _editJobViewModel;
        
        public EditJobWindow(Job job, MainWindow _mainWindow)
        {
            InitializeComponent();
            _editJobViewModel = new EditJobViewModel(job, _mainWindow);
            DataContext = _editJobViewModel;
            
            _editJobViewModel.RequestClose += Close;
        }
    }
}