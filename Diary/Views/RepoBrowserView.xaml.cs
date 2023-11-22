using Diary.ViewModels.Views;
using System.Windows.Controls;

namespace Diary.Views
{
	/// <summary>
	/// Interaction logic for RepoBrowserView.xaml
	/// </summary>
	public partial class RepoBrowserView : UserControl
	{
		public RepoBrowserView()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (((Button)sender).DataContext is not GroupedRepoActionModels action) return;

			if (action.FilteredActions.Count == 1)
			{
				action.FilteredActions.First().OpenCommand.Execute(null);
			}
			else if (action.FilteredActions.Any())
			{
				((Button)sender).ContextMenu.DataContext = ((Button)sender).DataContext;
				((Button)sender).ContextMenu.IsOpen = true;
			}
		}
	}
}
