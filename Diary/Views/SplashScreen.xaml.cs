using ModernThemables.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Diary.Views
{
	/// <summary>
	/// Interaction logic for SplashScreen.xaml
	/// </summary>
	public partial class SplashScreen : Window
	{
		public SplashScreen()
		{
			var themeVm = new ThemingControlViewModel();
			InitializeComponent();
			themeVm.Dispose();
		}

		public void SetProgressPercent(double progressPercent)
		{
			Application.Current.Dispatcher.Invoke(() => ProgressBar.Value = progressPercent);
		}

		public void SetMessage(string message)
		{
			Application.Current.Dispatcher.Invoke(() => MessageTextBlock.Text = message);
		}
	}
}
