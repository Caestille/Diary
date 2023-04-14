using CoreUtilities.HelperClasses;
using Diary.Core.Messages.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Diary.Core.ViewModels.Base
{
	public class ViewModelBase : ObservableRecipient
	{
		private Func<ViewModelBase> createChildFunc;

		public ICommand SelectCommand => new RelayCommand(Select);
		public ICommand AddChildCommand => new RelayCommand(() => AddChild());
		public ICommand RequestDeleteCommand => new RelayCommand(OnDeleteRequested);

		private string name;
		public string Name
		{
			get => name;
			set => SetProperty(ref name, value.TrimStart());
		}

		private Color colour = Colors.Red;
		public Color Colour
		{
			get => colour;
			set
			{
				SetProperty(ref colour, value);
				OnCommitColourUpdate();
			}
		}

		private RangeObservableCollection<ViewModelBase> childViewModels = new();
		public RangeObservableCollection<ViewModelBase> ChildViewModels
		{
			get => childViewModels;
			set => SetProperty(ref childViewModels, value);
		}

		private bool isSelected;
		public bool IsSelected
		{
			get => isSelected;
			set => SetProperty(ref isSelected, value);
        }

		private bool showsInSearch = false;
        public bool ShowsInSearch
        {
            get => showsInSearch;
            set => SetProperty(ref showsInSearch, value);
        }

        private bool isCollapsed;
		public bool IsCollapsed
		{
			get => isCollapsed;
			set => SetProperty(ref isCollapsed, value);
		}

		public virtual bool SupportsAddingChildren => createChildFunc != null;

		private bool supportsDeleting;
		public bool SupportsDeleting
		{
			get => supportsDeleting;
			set => SetProperty(ref supportsDeleting, value);
		}

		private bool isShowingChildren;
		public bool IsShowingChildren
		{
			get => isShowingChildren;
			set => SetProperty(ref isShowingChildren, value);
        }

		private bool allowShowDropdownIndicator = true;
        public bool AllowShowDropdownIndicator
        {
            get => allowShowDropdownIndicator;
            set => SetProperty(ref allowShowDropdownIndicator, value);
        }

        private int level = 0;
		public int Level
		{
			get => level;
			set => SetProperty(ref level, value);
		}

		protected IMessenger BaseMessenger => Messenger;

		public ViewModelBase(string name, Func<ViewModelBase> createChild = null)
		{
			Name = name;
			createChildFunc = createChild;

			BindMessages();

			Messenger.Send(new RequestSyncTagsMessage());

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.ShutdownStarted += OnShutdownStart;
			}
		}

		protected virtual void BindMessages()
		{
			Messenger.Register<ViewModelRequestShowMessage>(this, async (sender, message) =>
			{
				//if (message.ViewModel == this)
				//	OnViewModelRequestShow(message);
				//else if (IsSelected)
				//	IsSelected = false;
			});

			Messenger.Register<ViewModelRequestDeleteMessage>(this, (sender, message) =>
			{
				OnViewModelRequestDelete(message);
			});
		}

		protected async virtual Task OnViewModelRequestShow(ViewModelRequestShowMessage message)
		{
			
		}

		protected virtual void OnDeleteRequested()
		{
			Messenger.Send(new ViewModelRequestDeleteMessage(this));
		}

		protected virtual void OnViewModelRequestDelete(ViewModelRequestDeleteMessage message)
		{
			if (ChildViewModels.Contains(message.ViewModel))
			{
				ChildViewModels.Remove((ViewModelBase)message.ViewModel);
				((ViewModelBase)message.ViewModel).OnDelete();

                message.ViewModel = null;

				if (IsShowingChildren && !ChildViewModels.Any())
				{
					IsShowingChildren = false;
				}

				OnPropertyChanged(nameof(ChildViewModels));
				OnChildrenChanged();
			}
		}

		protected virtual void OnDelete()
		{
			Application.Current.Dispatcher.ShutdownStarted -= OnShutdownStart;
		}

		protected virtual void Select()
		{
			if (ChildViewModels.Count != 0 && SupportsAddingChildren)
			{
				IsShowingChildren = !IsShowingChildren;
			}
			else if (!SupportsAddingChildren)
			{
				Messenger.Send(new ViewModelRequestShowMessage(this));
			}
		}

		public virtual void AddChild(ViewModelBase viewModelToAdd = null, string name = "", int? index = null)
		{
			var viewModel = viewModelToAdd ?? createChildFunc();
			if (name != string.Empty)
			{
				viewModel.Name = name;
			}

			if (index == null)
			{
				ChildViewModels.Add(viewModel);
			}
			else
			{
				ChildViewModels.Insert(index.Value, viewModel);
			}

			foreach (var vm in ChildViewModels)
			{
				vm.SetLevel(level + 1);
			}

			//if (SupportsAddingChildren)
			//{
			//	IsShowingChildren = true;
			//}

			OnPropertyChanged(nameof(ChildViewModels));
			OnChildrenChanged();
		}

		protected void SetLevel(int level)
		{
			Level = level;
			foreach (var vm in ChildViewModels)
			{
				vm.SetLevel(level + 1);
			}
		}

		protected void GetChildren(ref RangeObservableCollection<ViewModelBase> result, bool recurse)
		{
			result.AddRange(ChildViewModels);

			if (!recurse)
				return;

			foreach (var childVm in ChildViewModels)
			{
				childVm.GetChildren(ref result, true);
			}
		}

		protected async virtual Task OnSettingChange(string settingName, string settingGroup, object newValue) { }

		protected virtual void OnCommitColourUpdate() { }

		protected virtual void OnShutdownStart(object? sender, EventArgs e) { }

		protected virtual void OnChildrenChanged() { }
	}
}
