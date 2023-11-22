using Diary.Models;

namespace Diary.Messages
{
    public class SyncTagsMessage
    {
        public IEnumerable<CustomTag> Tags { get; private set; }

        public SyncTagsMessage(IEnumerable<CustomTag> tags)
        {
            Tags = tags;
        }
    }
}
