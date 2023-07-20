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
        private bool canAddItem;
        private bool showItemEditPanel;
        private ToDoItem? editItem;

        private bool allowUpdate = true;

        private bool hasLoadedItems;

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

        public ToDoItem? EditItem
        {
            get => editItem;
            set => SetProperty(ref editItem, value);
        }

        public bool CanAddItem
        {
            get => canAddItem;
            set => SetProperty(ref canAddItem, value);
        }

        private ObservableCollection<ToDoItem> items = new();
        public new ObservableCollection<ToDoItem> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        public bool ShowItemEditPanel
        {
            get => showItemEditPanel;
            set => SetProperty(ref showItemEditPanel, value);
        }

        public IEnumerable<GroupedItems> GroupedToDoItems =>
            this.Items.Where(x => !x.IsDone && x.Group != null)
                .Select(x => x.Group)
                .Order()
                .Distinct()
                .Select(x => new GroupedItems(x, this.Items.Where(y => !y.IsDone && y.Group == x)));

        public IEnumerable<ToDoItem> UnGroupedToDoItems => this.Items.Where(x => !x.IsDone && x.Group == null);

        public IEnumerable<GroupedItems> GroupedDoneItems =>
            this.Items.Where(x => x.IsDone && x.Group != null)
                .Select(x => x.Group)
                .Order()
                .Distinct()
                .Select(x => new GroupedItems(x, this.Items.Where(y => y.IsDone && y.Group == x)));

        public IEnumerable<ToDoItem> UnGroupedDoneItems => this.Items.Where(x => x.IsDone && x.Group == null);

        public new ICommand AddItemCommand => new RelayCommand(() =>
        {
            var group = ProposedName.Contains(":") ? ProposedName.Split(':').First() : null;
            if (string.IsNullOrEmpty(group)) group = null;
            var match = group != null ? GroupedToDoItems.Select(x => x.Group).FirstOrDefault(x => x.IndexOf(group, StringComparison.OrdinalIgnoreCase) != -1) : null;
            if (match != null) group = match;
            group = group != null ? group.Trim() : null;

            var name = (ProposedName.Contains(":") ? ProposedName.Split(':').Last() : ProposedName).Trim();

            Items.Add(new ToDoItem(name) { Group = group });
            Notify();
            ProposedName = "";
        });

        public ICommand DeleteItemCommand => new RelayCommand<ToDoItem>((item) =>
        {
            Items.Remove(item);
            Notify();
        });

        public ICommand EditItemCommand => new RelayCommand<ToDoItem>((item) =>
        {
            EditItem = item;
            ShowItemEditPanel = true;
        });

        public ICommand CloseEditPanelCommand => new RelayCommand(() =>
        {
            ShowItemEditPanel = false;
            EditItem = null;
        });

        public ICommand IncreaseIndexCommand => new RelayCommand<ToDoItem>((item) =>
        {
            allowUpdate = false;

            var isGrouped = item.Group != null;
            Func<IEnumerable<ToDoItem>> source = isGrouped
                ? (() => GroupedToDoItems.First(x => x.Group == item.Group).Items)
                : (() => UnGroupedToDoItems);

            var currentIndex = source().ToList().IndexOf(item);
            if (currentIndex == source().Count() - 1) return;

            var lastIndex = currentIndex;
            var maxIterations = Items.Count;
            int i = 0;
            while (source().ToList().IndexOf(item) == currentIndex && i < maxIterations)
            {
                Items.Remove(item);
                Items.Insert(Math.Min(Items.Count, lastIndex + 1), item);
                lastIndex++;
                Notify(false);
                i++;
            }
            Notify();
            allowUpdate = true;
        });

        public ICommand DecreaseIndexCommand => new RelayCommand<ToDoItem>((item) =>
        {
            allowUpdate = false;

            var isGrouped = item.Group != null;
            Func<IEnumerable<ToDoItem>> source = isGrouped
                ? (() => GroupedToDoItems.First(x => x.Group == item.Group).Items)
                : (() => UnGroupedToDoItems);

            var currentIndex = source().ToList().IndexOf(item);
            if (currentIndex == 0) return;

            var lastIndex = currentIndex;
            var maxIterations = Items.Count;
            int i = 0;
            while (source().ToList().IndexOf(item) == currentIndex && i < maxIterations)
            {
                Items.Remove(item);
                Items.Insert(Math.Max(0, lastIndex - 1), item);
                lastIndex--;
                Notify(false);
                i++;
            }
            Notify();
            allowUpdate = true;
        });

        public ICommand ToDoItemEditorKeyDownCommand => new RelayCommand<object>(CustomTagEditorKeyDown);

        public ToDoListViewModel(string workingDirectory)
            : base("To Do List")
        {
            this.workingDirectory = workingDirectory;

            if (File.Exists(toDoListWriteDirectory))
            {
                Items = new ObservableCollection<ToDoItem>(
                    JsonSerializer.Deserialize<List<ToDoItem>>(File.ReadAllText(toDoListWriteDirectory)));
            }

            cardUpdateTimer = new System.Timers.Timer(1000);
            cardUpdateTimer.AutoReset = true;
            cardUpdateTimer.Elapsed += CardUpdateTimer_Elapsed;
            cardUpdateTimer.Start();

            hasLoadedItems = true;

            Application.Current.Dispatcher.ShutdownStarted += (sender, e) =>
            {
                cardUpdateTimer.Elapsed -= CardUpdateTimer_Elapsed;
                cardUpdateTimer.Stop();
            };
        }

        private void CardUpdateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!allowUpdate) return;

            foreach (var item in Items)
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
                Notify(false);
            });
            base.BindMessages();
        }

        private void SetCanAddItem()
        {
            CanAddItem = !string.IsNullOrWhiteSpace(ProposedName);
        }

        private void Notify(bool force = true)
        {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(GroupedToDoItems));
            OnPropertyChanged(nameof(UnGroupedToDoItems));
            OnPropertyChanged(nameof(GroupedDoneItems));
            OnPropertyChanged(nameof(UnGroupedDoneItems));
            if (hasLoadedItems || force)
            {
                File.WriteAllText(toDoListWriteDirectory, JsonSerializer.Serialize(Items));
            }
        }

        private void CustomTagEditorKeyDown(object args)
        {
            if (CanAddItem && args is KeyEventArgs e && (e.Key == Key.Enter || e.Key == Key.Escape) && e.Key == Key.Enter)
            {
                AddItemCommand.Execute(null);
            }
        }
    }
}
