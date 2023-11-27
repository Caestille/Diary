using CoreUtilities.HelperClasses;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Diary.Models.RepoBrowser
{
	public class RepoModel : ObservableObject
	{
		private Dictionary<RepoAction, IEnumerable<string>> discoverFilter = new Dictionary<RepoAction, IEnumerable<string>>()
		{
			{ RepoAction.VisualStudio2022, new List<string>() { "sln" } },
			{ RepoAction.VisualStudioCode, new List<string>() { "md", "txt", "json" } },
			{ RepoAction.NotePadPlusPlus, new List<string>() { "txt", "json", "m" } },
		};

		public string Name { get; private set; }

		public string RepoDirectory { get; private set; }

		private bool isFavourited;
		public bool IsFavourited
		{
			get => isFavourited;
			set => SetProperty(ref isFavourited, value);
		}

		private RangeObservableCollection<GroupedRepoActionModels> actions = new();
		public RangeObservableCollection<GroupedRepoActionModels> Actions
		{
			get => actions;
			set => SetProperty(ref actions, value);
		}

		public ICommand OpenDirectoryCommand => new RelayCommand(() => Process.Start("explorer.exe", RepoDirectory));

		public RepoModel(string directory)
		{
			RepoDirectory = directory;
			Name = Path.GetFileName(directory);
			DiscoverActions();
		}

		private void DiscoverActions()
		{
			IEnumerable<T> InsertAndReturn<T>(IList<T> inList, T add) { inList.Insert(0, add); return inList; }
			Actions = new(
				discoverFilter.SelectMany(x => x.Value.SelectMany(y => Directory.EnumerateFiles(RepoDirectory, $"*.{y}", SearchOption.AllDirectories)
					.Select(z => new RepoActionModel(x.Key, z))))
					.GroupBy(x => x.Action)
					.Select(x => new GroupedRepoActionModels(x.Key, x.Key == RepoAction.VisualStudioCode ? InsertAndReturn(x.ToList(), new RepoActionModel(RepoAction.VisualStudioCode, RepoDirectory)) : x)))
			{ new GroupedRepoActionModels(RepoAction.Fork, new List<RepoActionModel>() { new RepoActionModel(RepoAction.Fork, RepoDirectory) }) };
		}
	}
}
