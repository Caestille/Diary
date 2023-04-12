using System;
using System.Globalization;
using System.Windows.Data;

namespace Diary.Converters
{
	public class MenuLeftPaddingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = (int)value;
			var ret = (val - 1) * Double.Parse((string)parameter);
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
