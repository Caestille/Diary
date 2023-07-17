using Diary.Core.Messages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Diary.Core.Models
{
    public class ToDoItem : ObservableRecipient
    {
        private string name;
        private string description;
        private DateTime? deadline;
        private TimeSpan? remainingTime;
        private TimeSpan? warningBeforeDeadline;
        private bool isWarning;
        private bool isDone;
        private bool isDescriptionExpanded;

        public ICommand ToggleExpandDescriptionCommand => new RelayCommand(() =>
        {
            IsDescriptionExpanded = !IsDescriptionExpanded;
        });

        [JsonPropertyName("name")]
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        [JsonPropertyName("description")]
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        [JsonPropertyName("deadline")]
        public DateTime? Deadline
        {
            get => deadline;
            set => SetProperty(ref deadline, value);
        }

        [JsonIgnore]
        public TimeSpan? RemainingTime
        {
            get => remainingTime;
            set => SetProperty(ref remainingTime, value);
        }

        [JsonPropertyName("warningBeforeDeadline")]
        public TimeSpan? WarningBeforeDeadline
        {
            get => warningBeforeDeadline;
            set => SetProperty(ref warningBeforeDeadline, value);
        }

        [JsonIgnore]
        public bool IsWarning
        {
            get => isWarning;
            set => SetProperty(ref isWarning, value);
        }

        [JsonPropertyName("isDone")]
        public bool IsDone
        {
            get => isDone;
            set
            {
                SetProperty(ref isDone, value);
                Messenger.Send(new ToDoItemIsDoneChangedMessage());
            }
        }

        [JsonIgnore]
        public bool IsDescriptionExpanded
        {
            get => isDescriptionExpanded;
            set => SetProperty(ref isDescriptionExpanded, value);
        }

        public ToDoItem(string name, string description, DateTime? deadline = null, TimeSpan? warningBeforeDeadline = null)
        {
            Name = name;
            Description = description;
            Deadline = deadline;
            WarningBeforeDeadline = warningBeforeDeadline;
        }
    }
}
