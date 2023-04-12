using System.Threading.Tasks;
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
    }
}
