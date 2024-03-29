﻿using CoreUtilities.HelperClasses;
using CoreUtilities.Interfaces.RegistryInteraction;
using CoreUtilities.Services;
using Diary.Messages.Base;
using Diary.ViewModels.Base;
using Diary.ViewModels.Views;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Globalization;
using System.Windows.Input;
using Diary.Extensions;
using System.Windows;

namespace Diary.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private IRegistryService registryService;
		private RefreshTrigger trigger;

		private const string MenuPinnedSettingName = "MenuPinned";

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
				trigger.Refresh();
			}
		}

		public RangeObservableCollection<ViewModelBase> AllViewModels
		{
			get
			{
				RangeObservableCollection<ViewModelBase> result = new RangeObservableCollection<ViewModelBase>();
				this.GetChildren(ref result, true);
				return new RangeObservableCollection<ViewModelBase>(result.Where(x => x is not DiaryEntryViewModel));
			}
		}

		public RangeObservableCollection<ViewModelBase> FilteredViewModels
		{
			get
			{
				return string.IsNullOrWhiteSpace(SearchText)
					? new RangeObservableCollection<ViewModelBase>()
					: new RangeObservableCollection<ViewModelBase>(
						AllViewModels.Where(x => x.ShowsInSearch).Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
			}
		}

		public MainViewModel(List<ViewModelBase> viewModels, IRegistryService registryService) : base(string.Empty)
		{
			this.registryService = registryService;
			ChildViewModels.AddRange(viewModels);
			SetLevel(0);
			trigger = new RefreshTrigger(() => { if (SearchText != "") OnPropertyChanged(nameof(FilteredViewModels)); }, 100);

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
				if (SearchText != "") SearchText = "";
			});

			Messenger.Register<ViewModelRequestShowMessage<DiaryDayViewModel>>(this, (sender, message) =>
			{
				if (searchText == string.Empty) return;

				SelectDay(DateTime.ParseExact(
					(message.ViewModel as DiaryDayViewModel).Name,
					"dd/MM/yyyy",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None));
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

			Messenger.Register<TakeMeToTodayMessage>(this, (sender, message) =>
			{
				if (!SelectDay(DateTime.Now, true))
				{
					if (MessageBox.Show("No week added for today, create one?", "No week to select", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						var currentDate = DateTime.Now;
						var calendar = AllViewModels.First(x => x is CalendarViewModel) as CalendarViewModel;
						var years = calendar.ChildViewModels.Cast<DiaryYearViewModel>();
						if (!years.Any(x => x.Name == currentDate.Year.ToString()))
						{
							calendar.AddChild();
						}
						var year = calendar.ChildViewModels.First(x => x.Name == currentDate.Year.ToString());
						var months = year.ChildViewModels.Cast<DiaryMonthViewModel>();
						if (!months.Any(x => x.Name == currentDate.ToString("MMMM")))
						{
							year.AddChild();
						}
						var month = year.ChildViewModels.First(x => x.Name == currentDate.ToString("MMMM"));
						if (!month.ChildViewModels.Any(x => x.Name == $"Week {currentDate.FirstDayOfWeek().ToString("dd/MM/yyyy")}"))
						{
							month.AddChild();
						}
						SelectDay(DateTime.Now, true);
						if (!IsMenuOpen)
						{
							year.IsCollapsed = true;
							month.IsCollapsed = true;
							month.ChildViewModels.First(x => x.IsSelected).IsCollapsed = true;
						}
					}
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

		protected override void OnShutdownStart(object? sender, EventArgs e)
		{
			trigger.Stop();
			base.OnShutdownStart(sender, e);
		}

		private bool SelectDay(DateTime day, bool includeDay = false)
		{
			try
			{
				var weekDate = day.FirstDayOfWeek();
				var weekVm = AllViewModels
					.Where(x => x is DiaryWeekViewModel)
					.First(x => (x as DiaryWeekViewModel).WeekStart.ToString("dd/MM/yyyy") == weekDate.ToString("dd/MM/yyyy"));
				var calendarVm = ChildViewModels.First(x => x is CalendarViewModel);
				var yearVm = calendarVm.ChildViewModels
					.Where(x => x is DiaryYearViewModel)
					.First(x => x.Name == weekDate.Year.ToString());
				var monthVm = yearVm.ChildViewModels
					.Where(x => x is DiaryMonthViewModel)
					.First(x => x.Name == weekDate.ToString("MMMM"));
				AllViewModels
					.Where(x => (x is DiaryYearViewModel || x is DiaryMonthViewModel || x is DiaryWeekViewModel) && x.IsShowingChildren)
					.ToList()
					.ForEach(x => { x.SelectCommand.Execute(null); });

				if (!calendarVm.IsShowingChildren) calendarVm.SelectCommand.Execute(null);
				if (!yearVm.IsShowingChildren) yearVm.SelectCommand.Execute(null);
				if (!monthVm.IsShowingChildren) monthVm.SelectCommand.Execute(null);
				if (!weekVm.IsSelected) weekVm.SelectCommand.Execute(null);

				if (includeDay)
				{
					var dayVm = weekVm.ChildViewModels
						.First(x => (x as DiaryDayViewModel).Name == day.ToString("dd/MM/yyyy"));
					if (!dayVm.IsSelected) dayVm.SelectCommand.Execute(null);
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		private async void ToggleMenuOpen()
		{
			IsMenuOpen = !IsMenuOpen;
			if (!IsMenuOpen && !string.IsNullOrEmpty(searchText))
			{
				SearchText = "";
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

			SelectDay(DateTime.Now, true);
		}
	}
}
