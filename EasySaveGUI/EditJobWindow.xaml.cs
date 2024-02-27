using System;
using System.Windows;
using System.Windows.Input;
using EasySaveGUI.Command;
using EasySaveGUI.ViewModel;
using EasySaveLib.Model;

namespace EasySaveGUI
{
    public partial class EditJobWindow : Window
    {
        private EditJobViewModel _editJobViewModel;
        
        public EditJobWindow(Job job)
        {
            InitializeComponent();
            _editJobViewModel = new EditJobViewModel(job);
            DataContext = _editJobViewModel;
        }
    }
}