using System;
using System.Windows.Input;

namespace EasySaveRemote.Command
{
    public class RelayCommand : ICommand
    {
        private readonly Action<string> _execute;
        private readonly Predicate<string> _canExecute;

        public RelayCommand(Action<string> execute, Predicate<string> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter as string) ?? true;

        public void Execute(object parameter) => _execute(parameter as string);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}