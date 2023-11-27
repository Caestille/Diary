using CoreUtilities.HelperClasses;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Diary.Models.RepoBrowser
{
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
}
