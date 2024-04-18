using Diary.ViewModels.Views;
using ModernThemables.ViewModels;

namespace Diary.Messages
{
    public class SummaryChangedMessage
    {
        public DiaryDayViewModel Sender { get; }

        public SummaryChangedMessage(DiaryDayViewModel sender)
        {
            Sender = sender;
        }
    }
}
