using Diary.Core.ViewModels.Base;
namespace Diary.Core.Models
{
    public class ToDoItem : ViewModelBase
    {
        public string Description { get; set; }

        public DateTime? Deadline { get; set; }

        public TimeSpan? RemainingTime { get; set; }

        public TimeSpan? WarningBeforeDeadline { get; set; }

        public bool IsWarning { get; set; }

        public bool IsDone { get; set; }

        public ToDoItem(string name, string description, DateTime? deadline = null, TimeSpan? warningBeforeDeadline = null)
            : base(name)
        {
            Description = description;
            Deadline = deadline;
            WarningBeforeDeadline = warningBeforeDeadline;
        }
    }
}
