using CoreUtilities.HelperClasses;
using CoreUtilities.Services;
using Diary.Models.RepoBrowser;
using ModernThemables.ViewModels;
using Microsoft.Toolkit.Mvvm.Input;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace Diary.ViewModels.Views
{
	public class RepoBrowserViewModel : ViewModelBase
	{
		private string workingDirectory;
		private RefreshTrigger searchThrottler;

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
				searchThrottler.Refresh();
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

		public ICommand FavouriteRepoCommand => new RelayCommand<RepoModel>((repo) =>
		{
			repo.IsFavourited = !repo.IsFavourited;
			if (repo.IsFavourited) FavouriteRepos.Add(repo.Name);
			else FavouriteRepos.Remove(repo.Name);
			OrderByFavourites();
		});

		public ICommand FavouriteRootCommand => new RelayCommand(() =>
		{
			IsRootFavourited = !IsRootFavourited;
			if (IsRootFavourited) FavouriteRoots.Add(RootDirectory);
			else FavouriteRoots.Remove(RootDirectory);
		});

		public ICommand LoadFavouriteRootCommand => new RelayCommand<string>((root) => RootDirectory = root);

		private bool isRootFavourited;
		public bool IsRootFavourited
		{
			get => isRootFavourited;
			set => SetProperty(ref isRootFavourited, value);
		}

		public RepoBrowserViewModel(string workingDirectory) : base("Repositories")
		{
			this.workingDirectory = workingDirectory;
			searchThrottler = new RefreshTrigger(() => _ = ApplyFilters(), 300);
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
			searchThrottler.Stop();
			File.WriteAllText(WritePath, JsonSerializer.Serialize(new RepoBrowserSettings(RootDirectory, FavouriteRoots, FavouriteRepos)));
		}
	}
}
