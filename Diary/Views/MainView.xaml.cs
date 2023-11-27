using System.Windows.Controls;
using System.Windows.Data;
using ModernThemables.Controls;

namespace Diary.Views
{
	/// <summary>
	/// Interaction logic for MainView.xaml
	/// </summary>
	public partial class MainView : UserControl
	{
		private const string DefaultSearchText = "Search...";
		public MainView()
		{
			InitializeComponent();
			this.Loaded += MainView_Loaded;
		}

		private async void MainView_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			await System.Threading.Tasks.Task.Run(() => Thread.Sleep(300));
			BindingExpression binding = Blurrer.GetBindingExpression(BlurHost.BlurEnabledProperty);
			Binding parentBinding = binding.ParentBinding;
			Blurrer.BlurEnabled = true;
			Blurrer.DrawBlurredElementBackground();
			Blurrer.SetBinding(BlurHost.BlurEnabledProperty, parentBinding);
			this.Loaded -= MainView_Loaded;
		}
    }
}
