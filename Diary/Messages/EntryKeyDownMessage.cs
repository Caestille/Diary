using Diary.ViewModels.Base;
using System.Windows.Input;

namespace Diary.Messages.Base
{
    public class EntryKeyDownMessage
    {
        public ViewModelBase Sender { get; }

        public KeyEventArgs Args { get; }

        public EntryKeyDownMessage(ViewModelBase sender, KeyEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }
}
