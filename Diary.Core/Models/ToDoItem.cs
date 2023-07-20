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
        private string? group;
        private string description;
        private DateTime? deadline;
        private TimeSpan? remainingTime;
        private TimeSpan? warningBeforeDeadline;
        private string? warningBeforeDeadlineDays;
        private bool isWarning;
        private bool isDone;
        private bool isDescriptionExpanded;

        [JsonIgnore]
        public ICommand ToggleExpandDescriptionCommand => new RelayCommand(() =>
        {
            IsDescriptionExpanded = !IsDescriptionExpanded;
        });

        [JsonPropertyName("group")]
        public string? Group
        {
            get => group;
            set
            {
                SetProperty(ref group, value);
                Messenger.Send(new ToDoItemIsDoneChangedMessage());
            }
        }

        [JsonPropertyName("name")]
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
                Messenger.Send(new ToDoItemIsDoneChangedMessage());
            }
        }

        [JsonPropertyName("description")]
        public string Description
        {
            get => description;
            set
            {
                SetProperty(ref description, value);
                Messenger.Send(new ToDoItemIsDoneChangedMessage());
            }
        }

        [JsonPropertyName("deadline")]
        public DateTime? Deadline
        {
            get => deadline;
            set
            {
                SetProperty(ref deadline, value);
                Messenger.Send(new ToDoItemIsDoneChangedMessage());
            }
        }

        [JsonPropertyName("warningBeforeDeadline")]
        public string? WarningBeforeDeadlineDays
        {
            get => warningBeforeDeadlineDays;
            set
            {
                SetProperty(ref warningBeforeDeadlineDays, value);
                if (double.TryParse(value, out var days))
                {
                    WarningBeforeDeadline = TimeSpan.FromDays(days);
                }
                else
                {
                    WarningBeforeDeadline = null;
                }
                Messenger.Send(new ToDoItemIsDoneChangedMessage());
            }
        }

        [JsonIgnore]
        public TimeSpan? RemainingTime
        {
            get => remainingTime;
            set => SetProperty(ref remainingTime, value);
        }

        [JsonIgnore]
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

        public ToDoItem(string name)
        {
            Name = name;
        }
    }
}
