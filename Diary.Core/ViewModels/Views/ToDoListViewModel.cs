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
        private int? proposedWarningDays;
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

        public int? ProposedWarningDays
        {
            get => proposedWarningDays;
            set => SetProperty(ref proposedWarningDays, value);
        }

        public bool CanAddItem
        {
            get => canAddItem;
            set => SetProperty(ref canAddItem, value);
        }

        public new ICommand AddChildCommand => new RelayCommand(() => ChildViewModels.Add(
            new ToDoItem(ProposedName, ProposedDescription, ProposedDeadline, ProposedWarningDays != null ? TimeSpan.FromDays(ProposedWarningDays.Value) : null)));

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
