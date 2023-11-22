using CoreUtilities.HelperClasses;
using Diary.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Diary.ViewModels.Views
{
	public enum RepoAction
	{
		VisualStudio2022,
		VisualStudioCode,
		NotePadPlusPlus,
	}

	public class GroupedRepoActionModels : ObservableObject
	{
		private string searchText;
		public string SearchText
		{
			get => searchText;
			set
			{
				SetProperty(ref searchText, value);
				_ = ApplyFilters();
			}
		}

		public RepoAction Action { get; private set; }

		private List<RepoActionModel> actions;

		private RangeObservableCollection<RepoActionModel> filteredActions = new();
		public RangeObservableCollection<RepoActionModel> FilteredActions
		{
			get => filteredActions;
			set => SetProperty(ref filteredActions, value);
		}

		public GroupedRepoActionModels(RepoAction action, IEnumerable<RepoActionModel> actions)
		{
			Action = action;
			this.actions = new(actions);
			_ = ApplyFilters();
		}

		private async Task ApplyFilters()
		{
			if (string.IsNullOrWhiteSpace(SearchText)) { FilteredActions = new(actions); return; }
			var filtered = await Task.Run(() => actions.Where(x => x.ItemPath.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList());
			FilteredActions = new(filtered);
		}
	}

	public class RepoActionModel : ObservableObject
	{
		private Dictionary<RepoAction, string> executionTemplate = new Dictionary<RepoAction, string>()
		{
			{ RepoAction.VisualStudio2022, "{PATH}" },
			{ RepoAction.VisualStudioCode, "{PATH}" },
			{ RepoAction.NotePadPlusPlus, "{PATH}" },
		};

		private string openPath;

		public string ItemPath { get; private set; }

		public RepoAction Action { get; private set; }

		public ICommand OpenCommand => new RelayCommand(() => Process.Start(openPath));

		public RepoActionModel(RepoAction action, string path)
		{
			Action = action;
			ItemPath = path;
			openPath = executionTemplate[action].Replace("{PATH}", path);
		}
	}

	public class RepoModel : ObservableObject
	{
		private Dictionary<RepoAction, IEnumerable<string>> discoverFilter = new Dictionary<RepoAction, IEnumerable<string>>()
		{
			{ RepoAction.VisualStudio2022, new List<string>() { "sln" } },
			{ RepoAction.VisualStudioCode, new List<string>() { "sln", "md" } },
			{ RepoAction.NotePadPlusPlus, new List<string>() { "txt", "json" } },
		};

		public string Name { get; private set; }

		public string RepoDirectory { get; private set; }

		private RangeObservableCollection<GroupedRepoActionModels> actions = new();
		public RangeObservableCollection<GroupedRepoActionModels> Actions
		{
			get => actions;
			set => SetProperty(ref actions, value);
		}

		public ICommand OpenDirectoryCommand => new RelayCommand(() => Process.Start(RepoDirectory));

		public RepoModel(string directory)
		{
			RepoDirectory = directory;
			Name = Path.GetFileName(directory);
			DiscoverActions();
		}

		private void DiscoverActions()
		{
			Actions = new(
				discoverFilter.SelectMany(x => x.Value.SelectMany(y => Directory.EnumerateFiles(RepoDirectory, $"*.{y}", SearchOption.AllDirectories)
					.Select(z => new RepoActionModel(x.Key, z))))
					.GroupBy(x => x.Action)
					.Select(x => new GroupedRepoActionModels(x.Key, x))
				);
		}
	}

	public class RepoBrowserViewModel : ViewModelBase
	{
		private string rootDirectory;
		public string RootDirectory
		{
			get => rootDirectory;
			set
			{
				SetProperty(ref rootDirectory, value);
				_ = LoadReposAsync();
			}
		}

		private List<RepoModel> repos = new();

		private RangeObservableCollection<RepoModel> filteredRepos = new();
		public RangeObservableCollection<RepoModel> FilteredRepos
		{
			get => filteredRepos;
			set => SetProperty(ref filteredRepos, value);
		}

		private string searchText;
		public string SearchText
		{
			get => searchText;
			set
			{
				SetProperty(ref searchText, value);
				_ = ApplyFilters();
			}
		}

		public RepoBrowserViewModel() : base("Repo Browser")
		{
			
		}

		private async Task LoadReposAsync()
		{
			if (!Directory.Exists(RootDirectory)) { repos = new(); _ =ApplyFilters(); return; }
			var repositories = await Task.Run(() => Directory.EnumerateDirectories(RootDirectory)
				.Select(x => new RepoModel(x))
				.ToList());
			repos = new(repositories);
			_ = ApplyFilters();
		}

		private async Task ApplyFilters()
		{
			if (string.IsNullOrWhiteSpace(SearchText)) { FilteredRepos = new(repos); return; }
			var filtered = await Task.Run(() => repos.Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList());
			FilteredRepos = new(filtered);
		}
	}
}
