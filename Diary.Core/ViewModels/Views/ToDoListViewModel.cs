using Diary.Core.Models;
using Diary.Core.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

namespace Diary.Core.ViewModels.Views
{
    public class ToDoListViewModel : ViewModelBase
    {
        private string proposedName;
        private string proposedDescription;
        private DateTime? proposedDeadline;
        private TimeSpan? proposedWarning;
        private bool canAddItem;

        public string ProposedName
        {
            get => proposedName;
            set
            {
                SetProperty(ref proposedName, value);
                SetCanAddItem();
            }
        }

        public string ProposedDescription
        {
            get => proposedDescription;
            set
            {
                SetProperty(ref proposedDescription, value);
                SetCanAddItem();
            }
        }

        public DateTime? ProposedDeadline
        {
            get => proposedDeadline;
            set => SetProperty(ref proposedDeadline, value);
        }

        public TimeSpan? ProposedWarning
        {
            get => proposedWarning;
            set => SetProperty(ref proposedWarning, value);
        }

        public bool CanAddItem
        {
            get => canAddItem;
            set => SetProperty(ref canAddItem, value);
        }

        public new ICommand AddChildCommand => new RelayCommand(() => ChildViewModels.Add(new ToDoItem(ProposedName, ProposedDescription, ProposedDeadline, ProposedWarning)));

        public ICommand DeleteItemCommand => new RelayCommand<ToDoItem>((item) => ChildViewModels.Remove(item));

        public ToDoListViewModel()
            : base("To Do List")
        {

        }

        private void SetCanAddItem()
        {
            CanAddItem = !string.IsNullOrWhiteSpace(ProposedName) && !string.IsNullOrWhiteSpace(ProposedDescription);
        }
    }
}
