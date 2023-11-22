using Diary.ViewModels.Base;

namespace Diary.Messages
{
    public class SummaryChangedMessage
    {
        public ViewModelBase Sender { get; }

        public SummaryChangedMessage(ViewModelBase sender)
        {
            Sender = sender;
        }
    }
}
