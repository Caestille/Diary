using Diary.ViewModels.Views;

namespace Diary.Messages.Base
{
    public class EntryDateChangedMessage
    {
        public DiaryEntryViewModel Sender { get; }

        public bool IsStartDate { get; }

        public DateTime OldValue { get; }

        public DateTime NewValue { get; }

        public EntryDateChangedMessage(DiaryEntryViewModel sender, bool isStartDate, DateTime oldValue, DateTime newValue)
        {
            Sender = sender;
            IsStartDate = isStartDate;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
