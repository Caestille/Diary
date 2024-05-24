using CoreUtilities.HelperClasses;
using Diary.Dtos;
using Diary.Messages;
using Diary.Messages.Base;
using ModernThemables.ViewModels;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;
using System.Windows;
using CoreUtilities.Services;
using Diary.Models.Tagging;

namespace Diary.ViewModels.Views
{
	public class DiaryEntryViewModel : ViewModelBase
    {
        private IEnumerable<TaggingRule> autoTags;
        private RefreshTrigger autoTagTrigger;

        public ICommand EntryGotFocusCommand => new RelayCommand(EntryGotFocus);
        public ICommand EntryKeyDownCommand => new RelayCommand<object>(EntryKeyDown);
        public ICommand EntryDateChangedCommand => new RelayCommand<object>(EntryDateChanged);

        private DateTime startTime;
        public DateTime StartTime
        {
            get => startTime;
            set
            {
                SetProperty(ref startTime, value);
                Messenger.Send(new TagChangedMessage(this, tag));
            }
        }

        private bool showFullDates;
        public bool ShowFullDates
        {
            get => showFullDates;
            set
            {
                SetProperty(ref showFullDates, value);
                Format = showFullDates ? "dd/MM/yyyy HH:mm" : "HH:mm";
            }
        }

        private string format = "HH:mm";
        public string Format
        {
            get => format;
            set => SetProperty(ref format, value);
        }

        private DateTime endTime;
        public DateTime EndTime
        {
            get => endTime;
            set
            {
                SetProperty(ref endTime, value);
                Messenger.Send(new TagChangedMessage(this, tag));
            }
        }

        private string entryText;
        public string EntryText
        {
            get => entryText;
            set
            {
                SetProperty(ref entryText, value);
                autoTagTrigger.Refresh();
            }
        }

        private bool isFocused;
        public bool IsFocused
        {
            get => isFocused;
            set => SetProperty(ref isFocused, value);
        }

        private CustomTag tag;
        public CustomTag Tag
        {
            get => tag;
            set
            {
                SetProperty(ref tag, value);
                Messenger.Send(new TagChangedMessage(this, value));
            }
        }

        private string starterTag = "";

        private RangeObservableCollection<CustomTag> selectableTags;
        public RangeObservableCollection<CustomTag> SelectableTags
        {
            get => selectableTags;
            set => SetProperty(ref selectableTags, value);
        }

		public TimeSpan Span => EndTime - StartTime;

        public DiaryEntryViewModel(string starterTag = "") : base("")
        {
            this.starterTag = starterTag;
            autoTagTrigger = new RefreshTrigger(AutoTag, 300);
            Messenger.Send(new RequestSyncTagsMessage());
            Messenger.Send(new RequestSyncRulesMessage()); 
			Messenger.Send(new EntryDateChangedMessage(this, true, StartTime, StartTime));
			Messenger.Send(new EntryDateChangedMessage(this, false, EndTime, EndTime));

		}

		public void Stop()
		{
			this.autoTagTrigger.Stop();
		}

		public override void OnDelete()
		{
			Stop();
			base.OnDelete();
		}

		internal static DiaryEntryViewModel FromDto(DiaryEntryDto dto)
        {
            return new DiaryEntryViewModel(dto.Tag) { EntryText = dto.Entry, StartTime = dto.Start, EndTime = dto.End };
        }

        protected override void BindMessages()
        {
            Messenger.Register<SyncTagsMessage>(this, (sender, message) =>
            {
                SelectableTags = new RangeObservableCollection<CustomTag>(
                    message.Tags);
                if (!SelectableTags.Contains(Tag))
                {
                    Tag = null;
                }
                if (starterTag != "" && Tag == null)
                {
                    Tag = SelectableTags.First(x => x.Tag == starterTag);
                    starterTag = "";
                }
            });

            Messenger.Register<SyncRulesMessage>(this, (sender, message) =>
            {
                autoTags = message.Rules;
            });

            base.BindMessages();
        }

		private void EntryGotFocus()
        {
            this.IsFocused = true;
            Messenger.Send(new EntryFocusedMessage(this));
        }

        private void EntryKeyDown(object args)
        {
            if (args is KeyEventArgs keyArgs)
            {
                Messenger.Send(new EntryKeyDownMessage(this, keyArgs));
            }
        }

        private void EntryDateChanged(object args)
        {
            if (args is RoutedPropertyChangedEventArgs<DateTime?> propertyArgs && propertyArgs.OldValue is DateTime oldVal && propertyArgs.NewValue is DateTime newVal)
            {
                var isStart = propertyArgs.NewValue == StartTime;
                Messenger.Send(new EntryDateChangedMessage(this, isStart, oldVal, newVal));
            }
        }

        private void AutoTag()
        {
            if (Tag != null || autoTags == null || string.IsNullOrEmpty(EntryText))
            {
                return;
            }

            CustomTag? tag = null;
            foreach (var rule in autoTags)
            {
                if (this.EntryText.Contains(rule.Text, StringComparison.OrdinalIgnoreCase))
                {
                    tag = rule.Tag;
                    break;
                }
            }

            if (tag != null)
            {
                Tag = tag;
            }
        }
    }
}
