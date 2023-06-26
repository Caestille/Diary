using Diary.Core.ViewModels.Base;
using System.Windows.Input;

namespace Diary.Core.Messages.Base
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
