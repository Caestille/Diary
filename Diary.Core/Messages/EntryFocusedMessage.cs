using Diary.Core.ViewModels.Views;

namespace FinanceTracker.Core.Messages.Base
{
    public class EntryFocusedMessage
    {
        public DiaryEntryViewModel Sender { get; }

        public EntryFocusedMessage(DiaryEntryViewModel sender)
        {
            Sender = sender;
        }
    }
}
