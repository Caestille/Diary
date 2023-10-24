using Diary.Core.Models;

namespace Diary.Core.Messages
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
