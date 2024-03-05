﻿using CoreUtilities.HelperClasses;
using Diary.Dtos;
using Diary.Extensions;
using Diary.Messages;
using Diary.Messages.Base;
using Diary.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace Diary.ViewModels.Views
{
    public class DiaryDayViewModel : ViewModelBase
    {
        public ICommand AddFirstChildCommand => new RelayCommand(AddFirstChild);

        private IEnumerable<DiaryEntryViewModel> castChildren => ChildViewModels.Cast<DiaryEntryViewModel>();

        public bool Loaded { get; private set; }

        private IEnumerable<DiaryEntryDto> entries;

        private string dayOfWeek;
        public string DayOfWeek
        {
            get => dayOfWeek;
            set => SetProperty(ref dayOfWeek, value);
        }

        private bool showFullDates;
        public bool ShowFullDates
        {
            get => showFullDates;
            set
            {
                SetProperty(ref showFullDates, value);
                ChildViewModels.ToList().ForEach(x => (x as DiaryEntryViewModel).ShowFullDates = value);
            }
        }

        private bool syncDates;
        public bool SyncDates
        {
            get => syncDates;
            set => SetProperty(ref syncDates, value);
        }

        private RangeObservableCollection<TagSummaryViewModel> tagSummaries;
        public RangeObservableCollection<TagSummaryViewModel> TagSummaries
        {
            get => tagSummaries;
            set => SetProperty(ref tagSummaries, value);
        }

        private string notes;
        public string Notes
        {
            get => notes;
            set
            {
                SetProperty(ref notes, value);
                Messenger.Send(new EntryKeyDownMessage(this, null));
            }
        }

        private DiaryEntryViewModel lastFocusedVm;

        public DiaryDayViewModel() : base("")
        {
            AllowShowDropdownIndicator = false;
            ShowsInSearch = true;
            GenerateSummary();
            Loaded = true;
        }

        public DiaryDayViewModel(IEnumerable<DiaryEntryDto> entries) : base("")
        {
            this.entries = entries;

            AllowShowDropdownIndicator = false;
            ShowsInSearch = true;
            GenerateSummary();
        }

        public void LoadEntries()
        {
            this.ChildViewModels = new RangeObservableCollection<ViewModelBase>(entries.Select(x => DiaryEntryViewModel.FromDto(x)));
            Loaded = true;
            entries = new List<DiaryEntryDto>();
        }

        internal static DiaryDayViewModel FromDto(DiaryDayDto dto)
        {
            var vm = new DiaryDayViewModel(dto.Entries) { Name = dto.Name, Notes = dto.Notes, DayOfWeek = dto.DayOfWeek };
            return vm;
        }

        protected override void Select()
        {
            IsSelected = true;
            Messenger.Send(new ViewModelRequestShowMessage<DiaryDayViewModel>(this));
            GenerateSummary();

            if (!Loaded)
            {
                LoadEntries();
            }
        }

        protected override void OnChildrenChanged()
        {
            GenerateSummary();
            base.OnChildrenChanged();
        }

        protected override void OnViewModelRequestDelete(ViewModelRequestDeleteMessage message)
        {
            if (message.ViewModel is DiaryEntryViewModel entryVm && ChildViewModels.Contains(entryVm))
            {
                if (lastFocusedVm == message.ViewModel)
                {
                    lastFocusedVm = null;
                }
            }
            base.OnViewModelRequestDelete(message);
        }

        protected override void BindMessages()
        {
            Messenger.Register<EntryKeyDownMessage>(this, (recipient, message) =>
            {
                if (ChildViewModels.Contains(message.Sender))
                {
                    EntryKeyDown(message.Args);
                }
            });
            Messenger.Register<EntryFocusedMessage>(this, (recipient, message) =>
            {
                if (ChildViewModels.Contains(message.Sender))
                {
                    SetFocus(message.Sender);
                }
            });
            Messenger.Register<TagChangedMessage>(this, (recipient, message) =>
            {
                if (ChildViewModels.Contains(message.Sender))
                {
                    GenerateSummary();
                }
            });
            Messenger.Register<EntryDateChangedMessage>(this, (recipient, message) =>
            {
                if (ChildViewModels.Contains(message.Sender))
                {
					if ((message.IsStartDate ? message.Sender.StartTime : message.Sender.EndTime).ToShortDateString() != Name)
					{
						var date = DateTime.Parse(Name);
						if (message.IsStartDate)
						{
							message.Sender.StartTime.SetDate(date);
						}
						else
						{
							message.Sender.EndTime.SetDate(date);
						}
					}
                    if (SyncDates)
                    {
                        var index = ChildViewModels.IndexOf(message.Sender);
                        if (message.IsStartDate && index > 0)
                        {
                            var entryBefore = ChildViewModels[index - 1];
                            ((DiaryEntryViewModel)entryBefore).EndTime = message.NewValue;
                        }
                        else if (index < ChildViewModels.Count - 1)
                        {
                            var entryAfter = ChildViewModels[index + 1];
                            ((DiaryEntryViewModel)entryAfter).StartTime = message.NewValue;
                        }   
                    }
                }
            });
            base.BindMessages();
            Messenger.Unregister<ViewModelRequestShowMessage>(this);
        }

        private void EntryKeyDown(KeyEventArgs args)
        {
            Messenger.Send(new EntryKeyDownMessage(this, args));

            if (args.Key == Key.Enter 
                && ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) 
                    || (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))))
            {
                var start = lastFocusedVm != null ? lastFocusedVm.EndTime : DateTime.Now - TimeSpan.FromMinutes(5);
                var end = lastFocusedVm != null
                    ? ChildViewModels.IndexOf(lastFocusedVm) < ChildViewModels.Count - 1
                        ? (ChildViewModels[ChildViewModels.IndexOf(lastFocusedVm) + 1] as DiaryEntryViewModel).StartTime
                        : DateTime.Now < start ? start + TimeSpan.FromMinutes(5) : DateTime.Now
                    : DateTime.Now < start ? start + TimeSpan.FromMinutes(5) : DateTime.Now;
                var newVM = new DiaryEntryViewModel()
                {
                    StartTime = start,
                    EndTime = end,
                };

                AddChild(newVM, index: lastFocusedVm != null ? ChildViewModels.IndexOf(lastFocusedVm) + 1 : null);
            }
        }

        private void AddFirstChild()
        {
            var start = ChildViewModels.Any()? (ChildViewModels.Last() as DiaryEntryViewModel).EndTime : DateTime.Now - TimeSpan.FromMinutes(5);
            var newVM = new DiaryEntryViewModel()
            {
                StartTime = start,
                EndTime = DateTime.Now < start ? start + TimeSpan.FromMinutes(5) : DateTime.Now,
            };

            AddChild(newVM);
        }

        private void SetFocus(DiaryEntryViewModel entry)
        {
            lastFocusedVm = entry;
            foreach (var vm in ChildViewModels) 
            {
                if (vm != lastFocusedVm)
                {
                    (vm as DiaryEntryViewModel).IsFocused = false;
                }
            }
        }

        public IList<TagSummaryViewModel> GenerateSummary(bool sendMessage = true)
        {
            var summaries = new List<TagSummaryViewModel>(
                castChildren
                .Where(x => x.Tag != null)
                .Select(x => x.Tag)
                .Distinct()
                .Select(x => new TagSummaryViewModel(x, castChildren.Where(y => (y.Tag?.Tag ?? "") == x.Tag).Sum(z => z.Span.TotalSeconds))));
            TagSummaries = new RangeObservableCollection<TagSummaryViewModel>(summaries);
            if (sendMessage) Messenger.Send(new SummaryChangedMessage(this));
            return summaries;
        }
    }
}
