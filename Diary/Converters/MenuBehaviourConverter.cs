using Diary.ViewModels.Views;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Diary.Converters
{
	public class MenuBehaviourConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter is string type)
			{
				if (type == "CanAddChild")
				{
					return ResolveCanAddChild(value);
				}
				else if (type == "CanDelete")
				{
					return ResolveCanDelete(value);
				}
				else if (type == "CanOpen")
				{
					return ResolveCanOpen(value);
				}
				else if (type == "ShowOpenIndicator")
				{
					return ResolveShowOpenIndicator(value);
				}
				else if (type == "Margin")
				{
					return ResolveMargin(value);
				}
			}

			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}

		private bool ResolveCanAddChild(object value)
		{
			switch (value)
			{
				case CalendarViewModel:
				case DiaryYearViewModel:
				case DiaryMonthViewModel:
					return true;
				default:
					return false;

			}
		}

		private bool ResolveCanDelete(object value)
		{
			switch (value)
			{
				case DiaryYearViewModel:
				case DiaryMonthViewModel:
				case DiaryWeekViewModel:
					return true;
				default:
					return false;

			}
		}

		private bool ResolveCanOpen(object value)
		{
			switch (value)
			{
				case CalendarViewModel:
				case DiaryYearViewModel:
				case DiaryMonthViewModel:
					return true;
				default:
					return false;

			}
		}

		private bool ResolveShowOpenIndicator(object value)
		{
			switch (value)
			{
				case CalendarViewModel:
					return true;
				default:
					return false;

			}
		}

		private Thickness ResolveMargin(object value)
		{
			switch (value)
			{
				case DiaryWeekViewModel week:
				case DiaryMonthViewModel month:
					return new Thickness(10, 0, 0, 0);
				default:
					return new Thickness(0);

			}
		}
	}
}
