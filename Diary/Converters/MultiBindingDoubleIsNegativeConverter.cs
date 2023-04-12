using System;
using System.Globalization;
using System.Windows.Data;

namespace Diary.Converters
{
	public class MultiBindingDoubleIsNegativeConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			return new DoubleIsNegativeConverter().Convert(values[0], targetType, parameter, culture);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
