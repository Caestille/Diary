using Diary.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diary.Core.Dtos
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
