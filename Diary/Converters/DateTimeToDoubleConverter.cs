using System.Globalization;
using System.Windows.Data;

namespace Diary.Converters
{
	internal class DateTimeToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var ts = (DateTime)value - DateTime.MinValue;
			return ts.TotalMilliseconds;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((double)value >= 0)
			{
				var ts = TimeSpan.FromMilliseconds((double)value);
				try { return DateTime.MinValue + ts; }
				catch { return DateTime.MinValue; }
			}
			else
				return DateTime.MinValue;
		}
	}
}
