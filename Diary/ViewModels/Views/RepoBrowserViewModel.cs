using CoreUtilities.HelperClasses;
using Diary.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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
		private Dictionary<RepoAction, (string, string)> executionTemplate = new()
		{
			{ RepoAction.VisualStudio2022, ("\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Professional\\Common7\\IDE\\devenv.exe\"", "\"{PATH}\"") },
			{ RepoAction.VisualStudioCode, ("code", "\"{PATH}\"") },
			{ RepoAction.NotePadPlusPlus, ("notepad++", "\"{PATH}\"") },
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
					.Select(x => new GroupedRepoActionModels(x.Key, x.Key == RepoAction.VisualStudioCode ? InsertAndReturn(x.ToList(), new RepoActionModel(RepoAction.VisualStudioCode, RepoDirectory)) : x)));
		}
	}

	public class RepoBrowserViewModel : ViewModelBase
	{
		private string workingDirectory;

		private string WritePath => Path.Combine(workingDirectory, "RepoBrowserSettings.json");

		private string rootDirectory;
		public string RootDirectory
		{
			get => rootDirectory;
			set
			{
				SetProperty(ref rootDirectory, value);
				IsRootFavourited = FavouriteRoots.Contains(value);
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

		private bool isLoading;
		public bool IsLoading
		{
			get => isLoading;
			set => SetProperty(ref isLoading, value);
		}

		private RangeObservableCollection<string> availableTags = new();
		public RangeObservableCollection<string> AvailableTags
		{
			get => availableTags;
			set => SetProperty(ref availableTags, value);
		}

		private RangeObservableCollection<string> selectedTags = new();
		public RangeObservableCollection<string> SelectedTags
		{
			get => selectedTags;
			set
			{
				SetProperty(ref selectedTags, value);
				assembledTags = string.Join('.', selectedTags);
				_ = GenerateAvailableTags();
				_ = ApplyFilters();
			}
		}

		private RangeObservableCollection<string> favouriteRoots = new();
		public RangeObservableCollection <string> FavouriteRoots
		{
			get => favouriteRoots;
			set => SetProperty(ref favouriteRoots, value);
		}

		private RangeObservableCollection<string> favouriteRepos = new();
		public RangeObservableCollection<string> FavouriteRepos
		{
			get => favouriteRepos;
			set => SetProperty(ref favouriteRepos, value);
		}

		private string assembledTags = "";

		public ICommand SelectTagCommand => new RelayCommand<string>((tag) => SelectedTags = new(new List<string>(SelectedTags) { tag }));

		public ICommand ClearTagsCommand => new RelayCommand(() => SelectedTags = new());

		public ICommand FavouriteRepoCommand => new RelayCommand<RepoModel>((repo) => { repo.IsFavourited = !repo.IsFavourited; if (repo.IsFavourited) FavouriteRepos.Add(repo.Name); else FavouriteRepos.Remove(repo.Name); OrderByFavourites(); });

		public ICommand FavouriteRootCommand => new RelayCommand(() => { IsRootFavourited = !IsRootFavourited; if (IsRootFavourited) FavouriteRoots.Add(RootDirectory); else FavouriteRoots.Remove(RootDirectory); });

		public ICommand LoadFavouriteRootCommand => new RelayCommand<string>((root) => RootDirectory = root);

		private bool isRootFavourited;
		public bool IsRootFavourited
		{
			get => isRootFavourited;
			set => SetProperty(ref isRootFavourited, value);
		}

		public RepoBrowserViewModel(string workingDirectory) : base("Repo Browser")
		{
			this.workingDirectory = workingDirectory;
			if (File.Exists(WritePath))
			{
				var settings = JsonSerializer.Deserialize<RepoBrowserSettings>(File.ReadAllText(WritePath));
				FavouriteRoots = new(settings.FavouriteRoots);
				RootDirectory = settings.CurrentRoot;
				FavouriteRepos = new(settings.FavouriteRepos);
			}

			_ = GenerateAvailableTags();
		}

		private async Task LoadReposAsync()
		{
			if (Directory.Exists(RootDirectory))
			{
				IsLoading = true;
				var repositories = await Task.Run(() => Directory.EnumerateDirectories(RootDirectory)
					.Select(x => new RepoModel(x))
					.ToList());
				repos = new(repositories);
				IsLoading = false;
			}
			else
			{
				repos = new();
			}

			foreach (var repo in repos)
			{
				repo.IsFavourited = FavouriteRepos.Contains(repo.Name);
			}

			_ = GenerateAvailableTags();
			_ = ApplyFilters();
		}

		private async Task GenerateAvailableTags()
		{
			if (!repos.Any(x => x.Name.StartsWith(assembledTags)) && SelectedTags.Any()) SelectedTags = new();

			var tags = await Task.Run(() =>
				repos.Where(x => x.Name.StartsWith(assembledTags))
					.Select(x => x.Name.Split(".")
					.Skip(SelectedTags.Count))
					.Where(x => x.Count() > 1)
					.Select(x => x.FirstOrDefault())
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Distinct()
					.ToList());
			AvailableTags = new(tags);
		}

		private async Task ApplyFilters()
		{
			if (string.IsNullOrWhiteSpace(SearchText) && !SelectedTags.Any())
			{
				FilteredRepos = new(repos.OrderByDescending(x => x.IsFavourited));
			}
			else
			{
				var filtered = await Task.Run(() => repos
					.Where(x => x.Name.StartsWith(assembledTags))
					.Where(x => x.Name.Contains(SearchText ?? "", StringComparison.OrdinalIgnoreCase)).ToList());
				FilteredRepos = new(filtered.OrderByDescending(x => x.IsFavourited));
			}
		}

		private void OrderByFavourites()
		{
			FilteredRepos = new(FilteredRepos.OrderByDescending(x => x.IsFavourited));
		}

		protected override void OnShutdownStart(object? sender, EventArgs e)
		{
			File.WriteAllText(WritePath, JsonSerializer.Serialize(new RepoBrowserSettings(RootDirectory, FavouriteRoots, FavouriteRepos)));
		}
	}

	public class RepoBrowserSettings
	{
		public string CurrentRoot { get; set; }

		public List<string> FavouriteRoots { get; set; }

		public List<string> FavouriteRepos { get; set; }

		public RepoBrowserSettings() { }

		public RepoBrowserSettings(string currentRoot, IEnumerable<string> favouriteRoots, IEnumerable<string> favouriteRepos)
		{
			CurrentRoot = currentRoot;
			FavouriteRoots = new(favouriteRoots);
			FavouriteRepos = new(favouriteRepos);
		}
	}
}
