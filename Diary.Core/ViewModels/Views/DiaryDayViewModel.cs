using CoreUtilities.HelperClasses;
using Diary.Core.Dtos;
using Diary.Core.Messages;
using Diary.Core.Messages.Base;
using Diary.Core.ViewModels.Base;
using FinanceTracker.Core.Messages.Base;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using MoreLinq;
using System.Windows.Input;

namespace Diary.Core.ViewModels.Views
{
    public class DiaryDayViewModel : ViewModelBase
    {
        public ICommand AddFirstChildCommand => new RelayCommand(AddFirstChild);

        private IEnumerable<DiaryEntryViewModel> castChildren => ChildViewModels.Cast<DiaryEntryViewModel>();

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
                ChildViewModels.ForEach(x => (x as DiaryEntryViewModel).ShowFullDates = value);
            }
        }

        private RangeObservableCollection<TagSummaryViewModel> tagSummaries;
        public RangeObservableCollection<TagSummaryViewModel> TagSummaries
        {
            get => tagSummaries;
            set => SetProperty(ref tagSummaries, value);
        }

        private DiaryEntryViewModel lastFocusedVm;

        public DiaryDayViewModel() : base("")
        {
            AllowShowDropdownIndicator = false;
            ShowsInSearch = true;
            GenerateSummary();
        }

        internal static DiaryDayViewModel FromDto(DiaryDayDto dto)
        {
            var vm = new DiaryDayViewModel() { Name = dto.Name, ChildViewModels = new RangeObservableCollection<ViewModelBase>(dto.Entries.Select(x => DiaryEntryViewModel.FromDto(x))), DayOfWeek = dto.DayOfWeek };
            return vm;
        }

        protected override void Select()
        {
            IsSelected = true;
            Messenger.Send(new ViewModelRequestShowMessage<DiaryDayViewModel>(this));
            GenerateSummary();
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
            base.BindMessages();
            Messenger.Unregister<ViewModelRequestShowMessage>(this);
        }

        private void EntryKeyDown(KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
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
