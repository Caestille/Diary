using Diary.Models.Tagging;

namespace Diary.Dtos
{
	public class TaggingRuleDto
    {
        public TaggingRuleDto() { }

        public TaggingRuleDto(TaggingRule rule)
        {
            Tag = rule.Tag.Tag;
            Text = rule.Text;
        }

        public string Tag { get; set; }

        public string Text { get; set; }
    }
}
