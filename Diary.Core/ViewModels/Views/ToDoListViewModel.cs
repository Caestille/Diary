using CoreUtilities.HelperClasses;
using Diary.Core.Messages;
using Diary.Core.Models;
using Diary.Core.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
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

        private string workingDirectory;

        private System.Timers.Timer cardUpdateTimer;

        private string toDoListWriteDirectory => Path.Combine(workingDirectory, "ToDoList.json");

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

        private ObservableCollection<ToDoItem> childViewModels = new();
        public new ObservableCollection<ToDoItem> ChildViewModels
        {
            get => childViewModels;
            set => SetProperty(ref childViewModels, value);
        }

        public new ICommand AddChildCommand => new RelayCommand(() =>
        {
            ChildViewModels.Add(new ToDoItem(
                ProposedName,
                ProposedDescription,
                ProposedDeadline,
                ProposedWarningDays != null ? TimeSpan.FromDays(ProposedWarningDays.Value) : null));
            Notify();
            ProposedName = "";
            ProposedDescription = "";
            ProposedDeadline = null;
            ProposedWarningDays = null;
        });

        public ICommand DeleteItemCommand => new RelayCommand<ToDoItem>((item) =>
        {
            ChildViewModels.Remove(item);
            Notify();
        });

        public IEnumerable<ToDoItem> ToDoItems => this.ChildViewModels.Where(x => !x.IsDone);

        public IEnumerable<ToDoItem> DoneItems => this.ChildViewModels.Where(x => x.IsDone);

        public ToDoListViewModel(string workingDirectory)
            : base("To Do List")
        {
            this.workingDirectory = workingDirectory;

            if (File.Exists(toDoListWriteDirectory))
            {
                ChildViewModels = new ObservableCollection<ToDoItem>(
                    JsonSerializer.Deserialize<List<ToDoItem>>(File.ReadAllText(toDoListWriteDirectory)));
            }

            cardUpdateTimer = new System.Timers.Timer(1000);
            cardUpdateTimer.AutoReset = true;
            cardUpdateTimer.Elapsed += CardUpdateTimer_Elapsed;
            cardUpdateTimer.Start();

            Application.Current.Dispatcher.ShutdownStarted += (sender, e) =>
            {
                cardUpdateTimer.Elapsed -= CardUpdateTimer_Elapsed;
                cardUpdateTimer.Stop();
            };
        }

        private void CardUpdateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var item in ChildViewModels)
            {
                item.RemainingTime = item.Deadline != null && !item.IsDone
                    ? DateTime.Now > item.Deadline
                        ? TimeSpan.FromSeconds(0)
                        : item.Deadline - DateTime.Now
                    : null;
                item.IsWarning = item.RemainingTime != null && !item.IsDone
                    ? item.RemainingTime == TimeSpan.FromSeconds(0)
                        ? true
                        : item.RemainingTime < item.WarningBeforeDeadline
                    : false;
            }
        }

        protected override void BindMessages()
        {
            Messenger.Register<ToDoItemIsDoneChangedMessage>(this, (recipient, sender) =>
            {
                OnPropertyChanged(nameof(ToDoItems));
                OnPropertyChanged(nameof(DoneItems));
                File.WriteAllText(toDoListWriteDirectory, JsonSerializer.Serialize(ChildViewModels));
            });
            base.BindMessages();
        }

        private void SetCanAddItem()
        {
            CanAddItem = !string.IsNullOrWhiteSpace(ProposedName) || !string.IsNullOrWhiteSpace(ProposedDescription);
        }

        private void Notify()
        {
            OnPropertyChanged(nameof(ChildViewModels));
            OnPropertyChanged(nameof(ToDoItems));
            OnPropertyChanged(nameof(DoneItems));
            File.WriteAllText(toDoListWriteDirectory, JsonSerializer.Serialize(ChildViewModels));
        }
    }
}
