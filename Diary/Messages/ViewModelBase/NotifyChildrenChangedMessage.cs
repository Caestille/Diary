using Diary.ViewModels.Base;

namespace Diary.Messages.Base
{
	public class NotifyChildrenChangedMessage
	{
		public ViewModelBase Sender { get; private set; }

		public NotifyChildrenChangedMessage(ViewModelBase sender)
		{
			Sender = sender;
		}
	}

	public class NotifyChildrenChangedMessage<T>
	{
		public ViewModelBase Sender { get; private set; }

		public NotifyChildrenChangedMessage(ViewModelBase sender)
		{
			Sender = sender;
		}
	}
}
