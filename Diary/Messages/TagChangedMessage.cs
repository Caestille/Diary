using Diary.Models.Tagging;
using ModernThemables.ViewModels;

namespace Diary.Messages
{
	public class TagChangedMessage
    {
        public GenericViewModelBase Sender { get; }

        public CustomTag Tag { get; }

        public TagChangedMessage(GenericViewModelBase sender, CustomTag tag)
        {
            Sender = sender;
            Tag = tag;
        }
    }
}
