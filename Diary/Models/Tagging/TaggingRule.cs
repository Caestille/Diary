﻿using Diary.Dtos;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Diary.Models.Tagging
{
	public class TaggingRule : ObservableObject
	{
		private CustomTag tag;
		private string text;

		public event EventHandler TagChanged;

		public TaggingRule()
		{

		}

		public TaggingRule(TaggingRuleDto dto, IEnumerable<CustomTag> tags)
		{
			Tag = tags.First(x => x.Tag == dto.Tag);
			Text = dto.Text;
		}

		public CustomTag Tag
		{
			get => tag;
			set
			{
				SetProperty(ref tag, value);
				TagChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public string Text
		{
			get => text;
			set => SetProperty(ref text, value);
		}
	}
}
