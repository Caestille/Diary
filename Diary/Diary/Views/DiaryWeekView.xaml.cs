using System.Windows;
using System.Windows.Controls;

namespace Diary.Views
{
    /// <summary>
    /// Interaction logic for DiaryWeekView.xaml
    /// </summary>
    public partial class DiaryWeekView : UserControl
	{
		public DiaryWeekView()
		{
			InitializeComponent();
		}

		private void NameTextbox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var textbox = sender as TextBox;
			if (textbox != null && (textbox.Visibility == Visibility.Visible))
			{
				textbox.Width = 150;
				textbox.Focus();
				textbox.SelectAll();
			}
		}
	}
}
