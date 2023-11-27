namespace Diary.Models.RepoBrowser
{
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
