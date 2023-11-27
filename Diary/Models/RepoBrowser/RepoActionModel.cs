using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;

namespace Diary.Models.RepoBrowser
{
	public class RepoActionModel : ObservableObject
	{
		private Dictionary<RepoAction, (string, string)> executionTemplate = new()
		{
			{ RepoAction.VisualStudio2022, ("\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Professional\\Common7\\IDE\\devenv.exe\"", "\"{PATH}\"") },
			{ RepoAction.VisualStudioCode, ("code", "\"{PATH}\"") },
			{ RepoAction.NotePadPlusPlus, ("notepad++", "\"{PATH}\"") },
			{ RepoAction.Fork, ("\"C:\\Users\\jward\\AppData\\Local\\Fork\\Fork.exe\"", "\"{PATH}\"") },
		};

		private string openPath;

		public string ItemPath { get; private set; }

		public RepoAction Action { get; private set; }

		public ICommand OpenCommand => new RelayCommand(
			() => Process.Start(
				new ProcessStartInfo(
					executionTemplate[Action].Item1,
					executionTemplate[Action].Item2.Replace("{PATH}", ItemPath))
				{
					UseShellExecute = true,
					CreateNoWindow = true
				}));

		public RepoActionModel(RepoAction action, string path)
		{
			Action = action;
			ItemPath = path;
		}
	}
}
