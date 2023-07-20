namespace Diary.Core.Models
{
    public class GroupedItems
    {
        public string Group { get; }

		public IEnumerable<ToDoItem> Items { get; }

        public GroupedItems(string group, IEnumerable<ToDoItem> items)
        {
            Group = group;
			Items = items;
        }
    }
}
