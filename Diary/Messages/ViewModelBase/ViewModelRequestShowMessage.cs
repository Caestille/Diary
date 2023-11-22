using Diary.ViewModels.Base;

namespace Diary.Messages.Base
{
	public class ViewModelRequestShowMessage
	{
		public ViewModelBase ViewModel { get; protected set; }

		public ViewModelRequestShowMessage(ViewModelBase viewModelToShow)
		{
			ViewModel = viewModelToShow;
		}
	}

	public class ViewModelRequestShowMessage<T>
	{
		public ViewModelBase ViewModel { get; protected set; }

		public ViewModelRequestShowMessage(ViewModelBase viewModelToShow)
		{
			ViewModel = viewModelToShow;
		}
	}
}
