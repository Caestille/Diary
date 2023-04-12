using System;
using System.Globalization;
using System.Windows.Data;

namespace Diary.Converters
{
	public class DoubleIsNegativeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not double) return true;

			double currencyValue = (double)value;
			return currencyValue < 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
