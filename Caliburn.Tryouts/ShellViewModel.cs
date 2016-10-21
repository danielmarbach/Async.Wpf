using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;

namespace Caliburn.Tryouts
{
    public class ShellViewModel : Conductor<object>
    {
        public ShellViewModel()
        {
            ShowPageOne();
        }

        public void ShowPageOne()
        {
            ActivateItem(new PageOneViewModel());
        }

        public void ShowPageTwo()
        {
            ActivateItem(new PageTwoViewModel());
        }

        string name;
        private CancellationTokenSource tokenSource;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => CanSayHello);
            }
        }

        public bool CanSayHello
        {
            get { return !string.IsNullOrWhiteSpace(Name); }
        }

        //public async Task SayHelloAsync()
        //{
        //    await Task.Delay(5000);
        //    MessageBox.Show($"{Name}");
        //}

        public void Cancel()
        {
            tokenSource?.Cancel();
        }

        public IEnumerable<IResult> SayHello()
        {
            using (tokenSource = new CancellationTokenSource())
            {
                yield return Loader.Show("Loadding");
                yield return new AsyncVoidSayHelloResult(tokenSource.Token).WhenCancelled(Loader.Hide);
                yield return Loader.Hide();

                tokenSource = null;
            }
        }
    }

    public class SayHelloResult : IResult
    {
        public void Execute(CoroutineExecutionContext context)
        {
            Thread.Sleep(5000);
            Completed(this, new ResultCompletionEventArgs());
        }

        public event EventHandler<ResultCompletionEventArgs> Completed;
    }
}