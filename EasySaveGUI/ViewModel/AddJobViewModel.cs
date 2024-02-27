using System.ComponentModel;
using System.Windows.Input;
using EasySaveGUI.Command;
using System.Windows.Forms;

namespace EasySaveGUI.ViewModel
{
    public class AddJobViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _sourcePath;
        public string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                _sourcePath = value;
                OnPropertyChanged(nameof(SourcePath));
            }
        }

        private string _destinationPath;
        public string DestinationPath
        {
            get { return _destinationPath; }
            set
            {
                _destinationPath = value;
                OnPropertyChanged(nameof(DestinationPath));
            }
        }

        public ICommand OpenSourceCommand { get; }
        public ICommand OpenDestinationCommand { get; }

        public AddJobViewModel()
        {
            OpenSourceCommand = new RelayCommand(OpenSource);
            OpenDestinationCommand = new RelayCommand(OpenDestination);
        }

        private void OpenSource(object parameter)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SourcePath = dialog.SelectedPath;
                }
            }
        }

        private void OpenDestination(object parameter)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    DestinationPath = dialog.SelectedPath;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
