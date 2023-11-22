using Diary.Models;

namespace Diary.Messages
{
    public class SyncRulesMessage
    {
        public IEnumerable<TaggingRule> Rules { get; private set; }

        public SyncRulesMessage(IEnumerable<TaggingRule> rules)
        {
            Rules = rules;
        }
    }
}
