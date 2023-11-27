using Diary.Messages;
using Diary.Models.ToDoList;
using Diary.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace Diary.ViewModels.Views
{
	public class ToDoListViewModel : ViewModelBase
	{
		private string proposedName;
		private bool canAddItem;
		private bool showItemEditPanel;
        private bool isListHorizontal;
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

        public bool IsListHorizontal
        {
            get => isListHorizontal;
            set => SetProperty(ref isListHorizontal, value);
        }

        public IEnumerable<GroupedItems> GroupedToDoItems =>
			this.Items
				.Select(x => x.Group)
				.Order()
				.Distinct()
				//.Where(x => this.Items.Where(y => y.Group == x && !y.IsDone).Any())
				.Select(x => new GroupedItems(x, this.Items.Where(y => !y.IsDone && y.Group == x), false));

		public IEnumerable<GroupedItems> GroupedDoneItems =>
			this.Items
				.Select(x => x.Group)
				.Order()
				.Distinct()
				//.Where(x => this.Items.Where(y => y.Group == x && y.IsDone).Any())
				.Select(x => new GroupedItems(x, this.Items.Where(y => y.IsDone && y.Group == x), true));

		private ObservableCollection<string> autofillOptions = new();
		public ObservableCollection<string> AutofillOptions
		{
            get => autofillOptions;
            set => SetProperty(ref autofillOptions, value);
        }

		public new ICommand AddItemCommand => new RelayCommand(() =>
		{
			var group = ProposedName.Contains(":") ? ProposedName.Split(':').First() : null;
			if (string.IsNullOrEmpty(group)) group = null;
			var match = group != null ? Items.Select(x => x.Group).FirstOrDefault(x => x.IndexOf(group, StringComparison.OrdinalIgnoreCase) != -1) : null;
			if (match != null) group = match;
			group = group != null ? group.Trim() : null;

			var name = (ProposedName.Contains(":") ? string.Join(":", ProposedName.Split(':').Skip(1)) : ProposedName).Trim();

			Items.Add(new ToDoItem(name) { Group = group });
			Notify();
			ProposedName = "";

			AutofillOptions = new ObservableCollection<string>(Items.Select(x => $"{x.Group}: ").Distinct());
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
			EditItem = null;
			ShowItemEditPanel = false;
		});

        public ICommand ToggleListStyleCommand => new RelayCommand(() =>
        {
			IsListHorizontal = !IsListHorizontal;
        });

        public ICommand IncreaseIndexCommand => new RelayCommand<ToDoItem>((item) =>
		{
			allowUpdate = false;

			var source = GroupedToDoItems.First(x => x.Group == item.Group).Items.ToList();

			var currentIndex = source.IndexOf(item);
			if (currentIndex == source.Count() - 1) return;

			var nextItem = source[currentIndex + 1];

            Items.Remove(item);
            var mainIndex = Items.IndexOf(nextItem);
			Items.Insert(mainIndex + 1, item);

            Notify();
			allowUpdate = true;
		});

		public ICommand DecreaseIndexCommand => new RelayCommand<ToDoItem>((item) =>
		{
			allowUpdate = false;

			var source = GroupedToDoItems.First(x => x.Group == item.Group).Items.ToList();

			var currentIndex = source.IndexOf(item);
			if (currentIndex == 0) return;

			var nextItem = source[currentIndex - 1];

            Items.Remove(item);
            var mainIndex = Items.IndexOf(nextItem);
			Items.Insert(Math.Clamp(mainIndex - 1, 0, Items.Count), item);

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

            AutofillOptions = new ObservableCollection<string>(Items.Select(x => $"{x.Group}: ").Distinct());
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
			OnPropertyChanged(nameof(GroupedDoneItems));
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
