using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace WpfNative.Tryouts
{
    public sealed class CountUrlBytesViewModel
    {
        public CountUrlBytesViewModel(MainWindowViewModel parent, string url, IAsyncCommand command)
        {
            LoadingMessage = "Loading (" + url + ")...";
            Command = command;
            RemoveCommand = new DelegateCommand(() => parent.Operations.Remove(this));
        }

        public string LoadingMessage { get; private set; }

        public IAsyncCommand Command { get; private set; }

        public ICommand RemoveCommand { get; private set; }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            Url = "http://www.example.com/";
            Operations = new ObservableCollection<CountUrlBytesViewModel>();
            CountUrlBytesCommand = new DelegateCommand(() =>
            {
                var countBytes = AsyncCommand.AsCommand(token => MyService.DownloadAndCountBytesAsync(Url, token));
                countBytes.Execute(null);
                Operations.Add(new CountUrlBytesViewModel(this, Url, countBytes));
            });
        }

        private string _url;
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CountUrlBytesViewModel> Operations { get; private set; }

        public ICommand CountUrlBytesCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
