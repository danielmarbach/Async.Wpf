using System.Collections.Generic;
using System.Threading;
using Caliburn.Micro;

namespace Caliburn.Tryouts
{
    public class PageOneViewModel : Screen
    {
        protected override void OnActivate()
        {
            var results = new List<IResult>();
            results.Add(Loader.Show("Loading"));
            results.Add(new AsyncVoidSayHelloResult(CancellationToken.None).WhenCancelled(Loader.Hide));
            results.Add(Loader.Hide());

            Coroutine.BeginExecute(results.GetEnumerator());
        }
    }
}