namespace Diary.Models
{
    public class GroupedItems
    {
        public string Group { get; }

        public bool ItemsAreDone { get; }

        public IEnumerable<ToDoItem> Items { get; }

        public GroupedItems(string group, IEnumerable<ToDoItem> items, bool doneIfEmpty)
        {
            Group = group;
			Items = items;
            ItemsAreDone = items.Any() ? items.All(x => x.IsDone) : doneIfEmpty;
        }
    }
}
