using ModernThemables.Controls;
using System.Windows;

namespace Diary
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ThemableWindow2
	{
		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= MainWindow_Loaded;
		}
	}
}
