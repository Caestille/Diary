using Diary.ViewModels.Views;

namespace Diary.Messages.Base
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
