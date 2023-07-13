using Diary.Core.ViewModels.Base;
namespace Diary.Core.Models
{
    public class ToDoItem : ViewModelBase
    {
        private string description;
        private DateTime? deadline;
        private TimeSpan? remainingTime;
        private TimeSpan? warningBeforeDeadline;
        private bool isWarning;
        private bool isDone;

        public string Description
        {
            get => description;
            set => SetProperty(ref this.description, value);
        }

        public DateTime? Deadline
        {
            get => deadline;
            set => SetProperty(ref this.deadline, value);
        }

        public TimeSpan? RemainingTime
        {
            get => remainingTime;
            set => SetProperty(ref this.remainingTime, value);
        }

        public TimeSpan? WarningBeforeDeadline
        {
            get => warningBeforeDeadline;
            set => SetProperty(ref this.warningBeforeDeadline, value);
        }

        public bool IsWarning
        {
            get => isWarning;
            set => SetProperty(ref this.isWarning, value);
        }

        public bool IsDone
        {
            get => isDone;
            set => SetProperty(ref this.isDone, value);
        }

        public ToDoItem(string name, string description, DateTime? deadline = null, TimeSpan? warningBeforeDeadline = null)
            : base(name)
        {
            Description = description;
            Deadline = deadline;
            WarningBeforeDeadline = warningBeforeDeadline;
        }
    }
}
