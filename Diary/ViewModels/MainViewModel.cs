using CoreUtilities.HelperClasses;
using CoreUtilities.Interfaces.RegistryInteraction;
using Diary.Core.Messages.Base;
using Diary.Core.ViewModels.Base;
using Diary.Core.ViewModels.Views;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Diary.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private IRegistryService registryService;

		private const string MenuPinnedSettingName = "MenuPinned";
		private const string DefaultSearchText = "Search";

		public ICommand ToggleMenuOpenCommand => new RelayCommand(ToggleMenuOpen);
		public ICommand ToggleMenuPinCommand => new RelayCommand(ToggleMenuPin);
		public ICommand FormLoadedCommand => new RelayCommand(Loaded);

		private ViewModelBase visibleViewModel;
		public ViewModelBase VisibleViewModel
		{
			get => visibleViewModel;
			set => SetProperty(ref visibleViewModel, value);
		}

		private bool isMenuPinned;
		public bool IsMenuPinned
		{
			get => isMenuPinned;
			set
			{
				SetProperty(ref isMenuPinned, value);
				registryService.SetSetting(MenuPinnedSettingName, value.ToString());
			}
		}

		private bool isMenuOpen = App.MenuStartOpen;
		public bool IsMenuOpen
		{
			get => isMenuOpen;
			set => SetProperty(ref isMenuOpen, value);
		}

		private string searchText = string.Empty;
		public string SearchText
		{
			get => searchText;
			set
			{
				SetProperty(ref searchText, value);
				OnPropertyChanged(nameof(FilteredViewModels));
			}
		}

		public RangeObservableCollection<ViewModelBase> AllViewModels
		{
			get
			{
				RangeObservableCollection<ViewModelBase> result = new RangeObservableCollection<ViewModelBase>();
				this.GetChildren(ref result, true);
				return result;
			}
		}

		public RangeObservableCollection<ViewModelBase> FilteredViewModels
		{
			get
			{
				return (SearchText == DefaultSearchText || string.IsNullOrWhiteSpace(SearchText)) 
					? SearchText == DefaultSearchText ? AllViewModels : new RangeObservableCollection<ViewModelBase>() 
					: new RangeObservableCollection<ViewModelBase>(
						AllViewModels.Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
			}
		}

		public MainViewModel(List<ViewModelBase> viewModels, IRegistryService registryService) : base(string.Empty)
		{
			this.registryService = registryService;
			ChildViewModels.AddRange(viewModels);
			SetLevel(0);

			DiaryWeekViewModel? weekVm = ChildViewModels.First().ChildViewModels.Any() 
				? ChildViewModels.First().ChildViewModels.First() as DiaryWeekViewModel 
				: null;

            if (weekVm != null)
			{
				weekVm.IsSelected = true;
				VisibleViewModel = weekVm;
				DiaryDayViewModel? dayVm = weekVm.ChildViewModels.Any()
					? (weekVm.ChildViewModels.FirstOrDefault(x => (x as DiaryDayViewModel).DayOfWeek == DateTime.Now.DayOfWeek.ToString()) ?? weekVm.ChildViewModels.First()) as DiaryDayViewModel
					: null;
				if (dayVm != null)
				{
					dayVm.IsSelected = true;
					weekVm.SelectedDay = dayVm;
				}
			}

			foreach (var vm in AllViewModels)
			{
				vm.IsCollapsed = !App.MenuStartOpen;
			}
		}

		protected override void BindMessages()
		{
			Messenger.Register<ViewModelRequestShowMessage>(this, (sender, message) => 
			{
				foreach (var vm in AllViewModels.Where(x => x is not DiaryDayViewModel))
				{
					if (vm.IsSelected) vm.IsSelected = false;
				}
				message.ViewModel.IsSelected = true;
				VisibleViewModel = message.ViewModel;
				SearchText = DefaultSearchText;
			});

			Messenger.Register<ViewModelRequestDeleteMessage>(this, (sender, message) =>
			{
				if (message.ViewModel is DiaryWeekViewModel)
				{
					VisibleViewModel = ChildViewModels.First().ChildViewModels
						.FirstOrDefault(x => x.GetType() == typeof(DiaryWeekViewModel)) ?? null;
					if (VisibleViewModel != null) VisibleViewModel.IsSelected = true;
				}
			});

			Messenger.Register<NotifyChildrenChangedMessage>(this, (sender, message) =>
			{
				OnPropertyChanged(nameof(AllViewModels));
			});
		}

		protected override void OnChildrenChanged()
		{
			OnPropertyChanged(nameof(AllViewModels));
		}

		private async void ToggleMenuOpen()
		{
			IsMenuOpen = !IsMenuOpen;
			if (IsMenuOpen)
			{
				await Task.Delay(50);
			}
			else
			{
				SearchText = "Search";
			}
			foreach (var vm in AllViewModels)
			{
				vm.IsCollapsed = !IsMenuOpen;
			}
		}

		private void ToggleMenuPin()
		{
			IsMenuPinned = !IsMenuPinned;
		}

		private void Loaded()
		{
			registryService.TryGetSetting(MenuPinnedSettingName, false, out var menuPinned);
			if (menuPinned && !IsMenuOpen)
			{
				ToggleMenuOpen();
			}
			IsMenuPinned = menuPinned;
		}
	}
}
