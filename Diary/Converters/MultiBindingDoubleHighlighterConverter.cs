using System.Globalization;
using System.Windows.Data;

namespace Diary.Converters
{
	public class MultiBindingDoubleHighlighterConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] is double && bool.TryParse((string)values[1], out var _))
			{
				bool invert = bool.Parse((string)values[1]);
				return new DoubleHighlighterConverter().Convert(values[0], targetType, invert.ToString(), culture);
			}

			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
