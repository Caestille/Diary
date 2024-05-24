using ModernThemables.ViewModels;
using System.Windows.Input;

namespace Diary.Messages.Base
{
    public class EntryKeyDownMessage
    {
        public GenericViewModelBase Sender { get; }

        public KeyEventArgs? Args { get; }

        public EntryKeyDownMessage(GenericViewModelBase sender, KeyEventArgs? args = null)
        {
            Sender = sender;
            Args = args;
        }
    }
}
