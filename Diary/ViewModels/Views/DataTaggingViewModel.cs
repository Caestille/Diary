using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using CoreUtilities.HelperClasses;
using System.Text.Json;
using System.Collections.ObjectModel;
using Diary.ViewModels.Base;
using Diary.Messages;
using Diary.Messages.Base;
using Diary.Models;
using Diary.Dtos;
using System.IO;

namespace Diary.ViewModels.Views
{
	public class DataTaggingViewModel : ViewModelBase
	{
		private string workingDirectory;
		private string tagsWritePath => Path.Combine(workingDirectory, "CustomTags.json");
		private string rulesWritePath => Path.Combine(workingDirectory, "TaggingRules.json");

		public ICommand AddCustomTagCommand => new RelayCommand(() =>
		{
			CustomTags.Add(ProposedTag);
			ProposedTag = new CustomTag();
			OnPropertyChanged(nameof(CustomTags));
			SynchroniseTags();
			File.WriteAllText(tagsWritePath, JsonSerializer.Serialize(CustomTags));
		});

		public ICommand DeleteCustomTagCommand => new RelayCommand<CustomTag>((tag) =>
		{
			CustomTags.Remove(tag);
			OnPropertyChanged(nameof(CustomTags));
			SynchroniseTags();
		});

		public ICommand AddRuleCommand => new RelayCommand(() =>
		{
			TaggingRules.Add(ProposedRule);
			ProposedRule.TagChanged -= ProposedRule_TagChanged;
			ProposedRule = new TaggingRule();
			ProposedRule.TagChanged += ProposedRule_TagChanged;
			OnPropertyChanged(nameof(TaggingRules));
			SynchroniseRules();
			File.WriteAllText(rulesWritePath, JsonSerializer.Serialize(TaggingRules.Select(x => new TaggingRuleDto(x))));
		});

		public ICommand DeleteRuleCommand => new RelayCommand<TaggingRule>((rule) =>
		{
			TaggingRules.Remove(rule);
			OnPropertyChanged(nameof(TaggingRules));
			SynchroniseRules();
		});

		public ICommand IncreaseTagIndexCommand => new RelayCommand<CustomTag>((tag) =>
		{
			var lastIndex = CustomTags.IndexOf(tag);
			CustomTags.Remove(tag);
			CustomTags.Insert(Math.Min(CustomTags.Count, lastIndex + 1), tag);
			OnPropertyChanged(nameof(CustomTags));
			SynchroniseTags();
		});

		public ICommand DecreaseTagIndexCommand => new RelayCommand<CustomTag>((tag) =>
		{
			var lastIndex = CustomTags.IndexOf(tag);
			CustomTags.Remove(tag);
			CustomTags.Insert(Math.Max(0, lastIndex - 1), tag);
			OnPropertyChanged(nameof(CustomTags));
			SynchroniseTags();
		});

		public ICommand CustomTagEditorKeyDownCommand => new RelayCommand<object>(CustomTagEditorKeyDown);

		public ICommand RuleEditorKeyDownCommand => new RelayCommand<object>(RuleEditorKeyDown);

		private CustomTag proposedTag;
		public CustomTag ProposedTag
		{
			get => proposedTag;
			set => SetProperty(ref proposedTag, value);
		}

		private TaggingRule proposedRule;
		public TaggingRule ProposedRule
		{
			get => proposedRule;
			set => SetProperty(ref proposedRule, value);
		}


		private ObservableCollection<CustomTag> customTags = new();
		public ObservableCollection<CustomTag> CustomTags
		{
			get => customTags;
			set => SetProperty(ref customTags, value);
		}


		private ObservableCollection<TaggingRule> taggingRules = new();
		public ObservableCollection<TaggingRule> TaggingRules
		{
			get => taggingRules;
			set => SetProperty(ref taggingRules, value);
		}

		private bool canAddTag;
		public bool CanAddTag
		{
			get => canAddTag;
			set => SetProperty(ref canAddTag, value);
		}

		private bool canAddRule;
		public bool CanAddRule
		{
			get => canAddRule;
			set => SetProperty(ref canAddRule, value);
		}

		public DataTaggingViewModel(string workingDirectory)
			: base("Tagging")
		{
			this.workingDirectory = workingDirectory;
			ProposedTag = new CustomTag();
			ProposedRule = new TaggingRule();
			ProposedRule.TagChanged += ProposedRule_TagChanged;

			if (File.Exists(tagsWritePath))
			{
				CustomTags = new RangeObservableCollection<CustomTag>(
					JsonSerializer.Deserialize<List<CustomTag>>(File.ReadAllText(tagsWritePath)));
			}

			if (File.Exists(rulesWritePath))
			{
				TaggingRules = new RangeObservableCollection<TaggingRule>(
					JsonSerializer.Deserialize<List<TaggingRuleDto>>(File.ReadAllText(rulesWritePath))
						.Where(x => CustomTags.Any(y => y.Tag == x.Tag))
						.Select(x => new TaggingRule(x, CustomTags)));
			}

			SynchroniseRules();
		}

		protected override void BindMessages()
		{
			Messenger.Register<RequestSyncTagsMessage>(this, (sender, message) =>
			{
				SynchroniseTags();
			});
			Messenger.Register<RequestSyncRulesMessage>(this, (sender, message) =>
			{
				SynchroniseRules();
			});
			base.BindMessages();
		}

		protected override void OnShutdownStart(object? sender, EventArgs e)
		{
			File.WriteAllText(tagsWritePath, JsonSerializer.Serialize(CustomTags));
			File.WriteAllText(rulesWritePath, JsonSerializer.Serialize(TaggingRules.Select(x => new TaggingRuleDto(x))));
			base.OnShutdownStart(sender, e);
		}

		private void CustomTagEditorKeyDown(object args)
		{
			CanAddTag = !CustomTags.Select(x => x.Tag).Contains(proposedTag.Tag)
				&& !string.IsNullOrWhiteSpace(proposedTag.Tag)
				&& !string.IsNullOrEmpty(proposedTag.Tag);

			if (CanAddTag && args is KeyEventArgs e && (e.Key == Key.Enter || e.Key == Key.Escape) && e.Key == Key.Enter)
			{
				AddCustomTagCommand.Execute(null);
				File.WriteAllText(tagsWritePath, JsonSerializer.Serialize(CustomTags));
			}
		}

		private void RuleEditorKeyDown(object args)
		{
			CanAddRule = !string.IsNullOrWhiteSpace(proposedRule.Text)
				&& proposedRule.Tag != null;

			if (CanAddRule && args is KeyEventArgs e && (e.Key == Key.Enter || e.Key == Key.Escape) && e.Key == Key.Enter)
			{
				AddRuleCommand.Execute(null);
				File.WriteAllText(rulesWritePath, JsonSerializer.Serialize(TaggingRules.Select(x => new TaggingRuleDto(x))));
			}
		}

		private void SynchroniseTags()
		{
			Messenger.Send(new SyncTagsMessage(CustomTags));
		}

		private void SynchroniseRules()
		{
			Messenger.Send(new SyncRulesMessage(TaggingRules));
		}

		private void ProposedRule_TagChanged(object? sender, EventArgs e)
		{
			RuleEditorKeyDown(null);
		}
	}
}