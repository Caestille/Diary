using Diary.Core.Models;
using Diary.Core.ViewModels.Base;

namespace Diary.Core.Messages
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
