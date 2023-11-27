using Diary.Models.RepoBrowser;
using System.Globalization;
using System.Windows.Data;

namespace Diary.Converters
{
	internal class ActionIconConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not RepoAction action) return null;

			switch (action)
			{
				case RepoAction.VisualStudio2022:
					return new Uri(@$"..\..\..\Resources\Images\VS2022.png", UriKind.Relative);
				case RepoAction.VisualStudioCode:
					return new Uri(@$"..\..\..\Resources\Images\VSCode.png", UriKind.Relative);
				case RepoAction.NotePadPlusPlus:
					return new Uri(@$"..\..\..\Resources\Images\NP++.png", UriKind.Relative);
				case RepoAction.Fork:
					return new Uri(@$"..\..\..\Resources\Images\Fork.png", UriKind.Relative);
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}
	}
}
