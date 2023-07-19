﻿using CoreUtilities.HelperClasses;
using CoreUtilities.HelperClasses.Extensions;
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

        public IEnumerable<ToDoItem> ToDoItems => this.Items.Where(x => !x.IsDone);

        public IEnumerable<ToDoItem> DoneItems => this.Items.Where(x => x.IsDone);

        public new ICommand AddItemCommand => new RelayCommand(() =>
        {
            Items.Insert(0, new ToDoItem(ProposedName));
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
            var currentIndex = ToDoItems.ToList().IndexOf(item);
            if (currentIndex == ToDoItems.Count() - 1) return;

            var lastIndex = currentIndex;
            while (ToDoItems.ToList().IndexOf(item) == currentIndex)
            {
                Items.Remove(item);
                Items.Insert(Math.Min(Items.Count, lastIndex + 1), item);
            }
            Notify();
            allowUpdate = true;
        });

        public ICommand DecreaseIndexCommand => new RelayCommand<ToDoItem>((item) =>
        {
            allowUpdate = false;
            var currentIndex = ToDoItems.ToList().IndexOf(item);
            if (currentIndex == 0) return;

            var lastIndex = currentIndex;
            while (ToDoItems.ToList().IndexOf(item) == currentIndex)
            {
                Items.Remove(item);
                Items.Insert(Math.Max(0, lastIndex - 1), item);
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
                OnPropertyChanged(nameof(ToDoItems));
                OnPropertyChanged(nameof(DoneItems));
                if (hasLoadedItems)
                {
                    File.WriteAllText(toDoListWriteDirectory, JsonSerializer.Serialize(Items));
                }
            });
            base.BindMessages();
        }

        private void SetCanAddItem()
        {
            CanAddItem = !string.IsNullOrWhiteSpace(ProposedName);
        }

        private void Notify()
        {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(ToDoItems));
            OnPropertyChanged(nameof(DoneItems));
            File.WriteAllText(toDoListWriteDirectory, JsonSerializer.Serialize(Items));
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
