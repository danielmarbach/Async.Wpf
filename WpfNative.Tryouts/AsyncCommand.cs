using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfNative.Tryouts
{
    // https://msdn.microsoft.com/en-us/magazine/dn630647.aspx
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }

    public abstract class AsyncCommandBase : IAsyncCommand, INotifyPropertyChanged
    {
        public abstract bool CanExecute(object parameter);
        public abstract Task ExecuteAsync(object parameter);
        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AsyncCommand<TResult> : AsyncCommandBase
    {
        private readonly Func<object, CancellationToken, Task<TResult>> _command;
        private readonly CancelAsyncCommand _cancelCommand;
        private INotifyTaskCompletion<TResult> _execution;
        private readonly object _state;

        public AsyncCommand(Func<object, CancellationToken, Task<TResult>> command, object state)
        {
            _state = state;
            _command = command;
            _cancelCommand = new CancelAsyncCommand();
        }

        public override bool CanExecute(object parameter)
        {
            return Execution == null || Execution.IsCompleted;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();
            Execution = _command(_state, _cancelCommand.Token).AsTaskCompletion();
            RaiseCanExecuteChanged();
            await Execution.TaskCompleted;
            _cancelCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        public INotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            private set
            {
                _execution = value;
                OnPropertyChanged();
            }
        }

        sealed class CancelAsyncCommand : ICommand
        {
            private CancellationTokenSource _cts = new CancellationTokenSource();
            private bool _commandExecuting;

            public CancellationToken Token => _cts.Token;

            public void NotifyCommandStarting()
            {
                _commandExecuting = true;
                if (!_cts.IsCancellationRequested)
                    return;
                _cts = new CancellationTokenSource();
                RaiseCanExecuteChanged();
            }

            public void NotifyCommandFinished()
            {
                _commandExecuting = false;
                RaiseCanExecuteChanged();
                _cts.Dispose();
            }

            bool ICommand.CanExecute(object parameter)
            {
                return _commandExecuting && !_cts.IsCancellationRequested;
            }

            void ICommand.Execute(object parameter)
            {
                _cts.Cancel();
                RaiseCanExecuteChanged();
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            private void RaiseCanExecuteChanged()
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    // extended from original to avoid closure capturing
    public static class AsyncCommand
    {
        public static AsyncCommand<object> AsCommand(this Func<Task> command)
        {
            return new AsyncCommand<object>(async (state, _) =>
            {
                var cmd = (Func<Task>) state;
                await cmd();
                return null;
            }, command);
        }

        public static AsyncCommand<object> AsCommand(this Func<object, Task> command, object state)
        {
            return new AsyncCommand<object>(async (s, _) =>
            {
                var cmdAndState = (Tuple<Func<object, Task>, object>)s;
                await cmdAndState.Item1(cmdAndState.Item2);
                return null;
            }, Tuple.Create(command, state));
        }

        public static AsyncCommand<TResult> AsCommand<TResult>(this Func<Task<TResult>> command)
        {
            return new AsyncCommand<TResult>((s, _) =>
            {
                var cmd = (Func<Task<TResult>>)s;
                return cmd();
            }, command);
        }

        public static AsyncCommand<TResult> AsCommand<TResult>(this Func<object, Task<TResult>> command, object state)
        {
            return new AsyncCommand<TResult>((s, _) =>
            {
                var cmdAndState = (Tuple<Func<object, Task<TResult>>, object>)s;
                return cmdAndState.Item1(cmdAndState.Item2);
            }, Tuple.Create(command, state));
        }

        public static AsyncCommand<object> AsCommand(this Func<CancellationToken, Task> command)
        {
            return new AsyncCommand<object>(async (state, token) =>
            {
                var cmd = (Func<CancellationToken, Task>) state;
                await cmd(token);
                return null;
            }, command);
        }

        public static AsyncCommand<object> AsCommand(this Func<object, CancellationToken, Task> command, object state)
        {
            return new AsyncCommand<object>(async (s, token) =>
            {
                var cmdAndState = (Tuple<Func<object, CancellationToken, Task>, object>)s;
                await cmdAndState.Item1(cmdAndState.Item2, token);
                return null;
            }, Tuple.Create(command, state));
        }

        public static AsyncCommand<TResult> AsCommand<TResult>(this Func<CancellationToken, Task<TResult>> command)
        {
            return new AsyncCommand<TResult>((s, token) =>
            {
                var cmd = (Func<CancellationToken, Task<TResult>>)s;
                return cmd(token);
            }, command);
        }

        public static AsyncCommand<TResult> AsCommand<TResult>(this Func<object, CancellationToken, Task<TResult>> command, object state)
        {
            return new AsyncCommand<TResult>(command, state);
        }
    }
}