using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Diary.Messages.Base
{
    public class ViewModelRequestDeleteMessage
    {
        public ObservableObject ViewModel { get; set; }

        public ViewModelRequestDeleteMessage(ObservableObject viewModelToDelete)
        {
            ViewModel = viewModelToDelete;
        }
    }

    public class ViewModelRequestDeleteMessage<T>
    {
        public ObservableObject ViewModel { get; set; }

        public ViewModelRequestDeleteMessage(ObservableObject viewModelToDelete)
        {
            ViewModel = viewModelToDelete;
        }
    }
}
