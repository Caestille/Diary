using Diary.Core.ViewModels.Views;

namespace Diary.Core.Messages.Base
{
    public class EntryDateChangedMessage
    {
        public DiaryEntryViewModel Sender { get; }

        public DateTime OldValue { get; }

        public DateTime NewValue { get; }

        public EntryDateChangedMessage(DiaryEntryViewModel sender, DateTime oldValue, DateTime newValue)
        {
            Sender = sender;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
