using CoreUtilities.HelperClasses;
using Diary.Messages.Base;
using Diary.ViewModels.Views;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Globalization;
using Diary.Extensions;
using System.Windows;
using ModernThemables.ViewModels;
using ModernThemables.Messages;

namespace Diary.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private GenericViewModelBase visibleViewModel;
		public GenericViewModelBase VisibleViewModel
		{
			get => visibleViewModel;
			set => SetProperty(ref visibleViewModel, value);
		}

		private bool isMenuOpen;
		public bool IsMenuOpen
		{
			get => isMenuOpen;
			set => SetProperty(ref isMenuOpen, value);
		}

		public RangeObservableCollection<GenericViewModelBase> AllViewModels
		{
			get
			{
				var result = GetChildren(true);
				var result2 = new RangeObservableCollection<GenericViewModelBase>(result.Cast<GenericViewModelBase>().Where(x => x is not DiaryEntryViewModel));
				return result2;
			}
		}

		public MainViewModel(List<GenericViewModelBase> viewModels) : base(string.Empty)
		{
			ChildViewModels.AddRange(viewModels);
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
			});

			Messenger.Register<ViewModelRequestShowMessage<DiaryDayViewModel>>(this, (sender, message) =>
			{
				SelectDay(DateTime.ParseExact(
					message.ViewModel.Name,
					"dd/MM/yyyy",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None));
			});

			Messenger.Register<ViewModelRequestDeleteMessage>(this, (sender, message) =>
			{
				if (message.ViewModel is DiaryWeekViewModel)
				{
					VisibleViewModel = ChildViewModels.First().GetChildren()
						.FirstOrDefault(x => x is DiaryWeekViewModel) as GenericViewModelBase ?? null;
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

		private bool SelectDay(DateTime day, bool includeDay = false)
		{
			try
			{
				var weekDate = day.FirstDayOfWeek();
				var weekVm = AllViewModels
					.Where(x => x is DiaryWeekViewModel)
					.First(x => (x as DiaryWeekViewModel).WeekStart.ToString("dd/MM/yyyy") == weekDate.ToString("dd/MM/yyyy"));
				var calendarVm = ChildViewModels.First(x => x is CalendarViewModel) as CalendarViewModel;
				var yearVm = calendarVm.ChildViewModels
					.Where(x => x is DiaryYearViewModel)
					.First(x => x.Name == weekDate.Year.ToString());
				var monthVm = yearVm.ChildViewModels
					.Where(x => x is DiaryMonthViewModel)
					.First(x => x.Name == weekDate.ToString("MMMM"));
				//AllViewModels
				//	.Where(x => (x is DiaryYearViewModel || x is DiaryMonthViewModel || x is DiaryWeekViewModel))
				//	.ToList()
				//	.ForEach(x => x.SelectCommand.Execute(null));

				if (!calendarVm.IsExpanded) calendarVm.IsExpanded = true;
				if (!yearVm.IsExpanded) yearVm.IsExpanded = true;
				if (!monthVm.IsExpanded) monthVm.IsExpanded = true;
				if (!weekVm.IsSelected) weekVm.SelectCommand.Execute(null);

				if (includeDay)
				{
					var dayVm = weekVm.GetChildren()
						.First(x => (x as DiaryDayViewModel).Name == day.ToString("dd/MM/yyyy")) as DiaryDayViewModel;
					if (!dayVm.IsSelected) dayVm.SelectCommand.Execute(null);
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
