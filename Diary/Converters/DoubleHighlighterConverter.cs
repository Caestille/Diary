using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Diary.Converters
{
	public class DoubleHighlighterConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool invert = parameter == null ? false : bool.Parse((string)parameter);
			double currencyValue = (double)value;
			if (invert)
			{
				return Math.Round(currencyValue, 2) > 0 ? ((SolidColorBrush)Application.Current.Resources["FinanceRedBrush"]) : ((SolidColorBrush)Application.Current.Resources["FinanceGreenBrush"]);
			}
			else
			{
				return Math.Round(currencyValue, 2) < 0 ? ((SolidColorBrush)Application.Current.Resources["FinanceRedBrush"]) : ((SolidColorBrush)Application.Current.Resources["FinanceGreenBrush"]);
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
