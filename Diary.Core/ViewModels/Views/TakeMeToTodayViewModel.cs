using Diary.Core.ViewModels.Base;
using Diary.Core.Messages.Base;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Diary.Core.ViewModels.Views
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
