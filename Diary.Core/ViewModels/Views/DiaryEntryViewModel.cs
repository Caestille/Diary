using CoreUtilities.HelperClasses;
using Diary.Core.Dtos;
using Diary.Core.Messages;
using Diary.Core.Messages.Base;
using Diary.Core.Models;
using Diary.Core.ViewModels.Base;
using FinanceTracker.Core.Messages.Base;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace Diary.Core.ViewModels.Views
{
    public class DiaryEntryViewModel : ViewModelBase
    {
        public ICommand EntryGotFocusCommand => new RelayCommand(EntryGotFocus);
        public ICommand EntryKeyDownCommand => new RelayCommand<object>(EntryKeyDown);

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
            set => SetProperty(ref entryText, value);
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
            Messenger.Send(new RequestSyncTagsMessage());
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
                if (starterTag != "")
                {
                    Tag = SelectableTags.First(x => x.Tag == starterTag);
                    starterTag = "";
                }
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
            if (args is KeyEventArgs keyArgs && keyArgs.Key == Key.Enter)
            {
                Messenger.Send(new EntryKeyDownMessage(this, keyArgs));
            }
        }
    }
}
