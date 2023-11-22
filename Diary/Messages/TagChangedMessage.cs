using Diary.Models;
using Diary.ViewModels.Base;

namespace Diary.Messages
{
    public class TagChangedMessage
    {
        public ViewModelBase Sender { get; }

        public CustomTag Tag { get; }

        public TagChangedMessage(ViewModelBase sender, CustomTag tag)
        {
            Sender = sender;
            Tag = tag;
        }
    }
}
