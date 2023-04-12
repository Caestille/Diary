using Diary.Core.ViewModels.Base;

namespace Diary.Core.Messages
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
