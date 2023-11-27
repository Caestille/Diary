using System.Globalization;
using System.Windows.Data;

namespace Diary.Converters
{
	public class IntMonthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value is int month)
			{
				return new DateTime(DateTime.Now.Year - 1, month, 1).ToString("MMM", CultureInfo.InvariantCulture);;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
