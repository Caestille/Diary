using ModernThemables.ViewModels;
using Diary.Messages.Base;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Diary.ViewModels.Views
{
    public class TakeMeToTodayViewModel : ViewModelBase
    {
        public TakeMeToTodayViewModel()
            : base("Take me to today")
        {
            
        }

        protected override void Select()
        {
            Messenger.Send(new TakeMeToTodayMessage());
        }
    }
}
