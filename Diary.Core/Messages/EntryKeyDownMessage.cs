using Diary.Core.ViewModels.Views;
using System.Windows.Input;

namespace Diary.Core.Messages.Base
{
    public class EntryKeyDownMessage
    {
        public DiaryEntryViewModel Sender { get; }

        public KeyEventArgs Args { get; }

        public EntryKeyDownMessage(DiaryEntryViewModel sender, KeyEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }
}
