using System;
using System.Windows.Input;

namespace WpfNative.Tryouts
{
    public sealed class DelegateCommand : ICommand
    {
        private readonly Action _command;
        private readonly Action<object> _commandWithState;

        public DelegateCommand(Action<object> command)
        {
            _commandWithState = command;
        }

        public DelegateCommand(Action command)
        {
            _command = command;
        }

        public void Execute(object parameter)
        {
            if (_command != null)
            {
                _command();
            }
            else
            {
                _commandWithState(parameter);
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}