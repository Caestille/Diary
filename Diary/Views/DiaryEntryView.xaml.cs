using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diary.Views
{
	/// <summary>
	/// Interaction logic for DiaryEntryView.xaml
	/// </summary>
	public partial class DiaryEntryView : UserControl
    {
        public DiaryEntryView()
        {
            InitializeComponent();
            this.Loaded += DiaryEntryView_Loaded;
        }

        private async void DiaryEntryView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= DiaryEntryView_Loaded;
            EntryTextbox.Focusable = true;
            EntryTextbox.Focusable = false;
            EntryTextbox.Focusable = true;
            await Task.Delay(100);
            Keyboard.Focus(EntryTextbox);
        }

        private void EntryTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            // Budget 'AcceptsReturn' which works nicely with pressing enter to add new entry
            if (e.Key == Key.Enter && !((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    || (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))))
            {
                EntryTextbox.Text = EntryTextbox.Text.Insert(EntryTextbox.SelectionStart, "\r\n");
                EntryTextbox.SelectionStart = EntryTextbox.Text.Length;
            }
        }
    }
}
