using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Caliburn.Tryouts
{
    public class AsyncVoidSayHelloResult : IResult
    {
        private CancellationToken token;

        public AsyncVoidSayHelloResult(CancellationToken token)
        {
            this.token = token;
        }
        public async void Execute(CoroutineExecutionContext context)
        {
            try
            {
                await Task.Delay(5000, token);
                Completed(this, new ResultCompletionEventArgs());
            }
            catch (OperationCanceledException)
            {
                Completed(this, new ResultCompletionEventArgs { WasCancelled = true});
            }
            catch (Exception ex)
            {
                Completed(this, new ResultCompletionEventArgs { Error = ex });
            }
        }

        public event EventHandler<ResultCompletionEventArgs> Completed;
    }
}