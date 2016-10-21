using System;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;

namespace Caliburn.Tryouts
{
    public class Loader : IResult
    {
        readonly string message;
        readonly bool hide;

        public Loader(string message)
        {
            this.message = message;
        }

        public Loader(bool hide)
        {
            this.hide = hide;
        }

        // Doesn't work yet just for demo purposes
        public void Execute(CoroutineExecutionContext context)
        {
            var view = context.View as FrameworkElement;
            while (view != null)
            {
                var textBox = (TextBox) view.FindName("statusTextBox");
                if (textBox != null)
                {
                    if (!string.IsNullOrEmpty(message))
                        textBox.Text = message;
                    textBox.Visibility = hide ? Visibility.Hidden : Visibility.Visible;
                    break;
                }

                view = view.Parent as FrameworkElement;
            }

            Completed(this, new ResultCompletionEventArgs());
        }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public static IResult Show(string message = null)
        {
            return new Loader(message);
        }

        public static IResult Hide()
        {
            return new Loader(true);
        }
    }
}