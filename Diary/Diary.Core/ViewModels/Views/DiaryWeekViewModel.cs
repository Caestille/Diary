using CoreUtilities.HelperClasses;
using Diary.Core.Dtos;
using Diary.Core.Extensions;
using Diary.Core.Messages;
using Diary.Core.Messages.Base;
using Diary.Core.Models;
using Diary.Core.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using MoreLinq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diary.Core.ViewModels.Views
{
    public class DiaryWeekViewModel : NameableViewModelBase
    {
        private string workingDirectory;

        public string WritePath => $"{Path.Combine(workingDirectory, guid.ToString())}.json";

        private List<CustomTag> tags = new List<CustomTag>();

        private Guid guid;

        public ICommand ShowWeekSummaryCommand => new RelayCommand(() => { GenerateSummary(); ShowWeekSummary = true; });

        public ICommand CloseWeekSummaryCommand => new RelayCommand(() => ShowWeekSummary = false);

        private bool showWeekSummary;
        public bool ShowWeekSummary
        {
            get => showWeekSummary;
            set => SetProperty(ref showWeekSummary, value);
        }

        private bool showFullDates;
        public bool ShowFullDates
        {
            get => showFullDates;
            set
            {
                SetProperty(ref showFullDates, value);
                ChildViewModels.ForEach(x => (x as DiaryDayViewModel).ShowFullDates = value);
            }
        }

        private DateTime weekStart;
        public DateTime WeekStart
        {
            get => weekStart;
            set => SetProperty(ref weekStart, value);
        }

        private DiaryDayViewModel selectedDay;
        public DiaryDayViewModel SelectedDay
        {
            get => selectedDay;
            set => SetProperty(ref selectedDay, value);
        }

        public RangeObservableCollection<DayTagSummaryViewModel> summary;
        public RangeObservableCollection<DayTagSummaryViewModel> Summary
        {
            get => summary;
            set => SetProperty(ref summary, value);
        }

        public RangeObservableCollection<TagSummaryViewModel> tagSummary;
        public RangeObservableCollection<TagSummaryViewModel> TagSummary
        {
            get => tagSummary;
            set => SetProperty(ref tagSummary, value);
        }

        private string formattedTotal;
        public string FormattedTotal
        {
            get => formattedTotal;
            set => SetProperty(ref formattedTotal, value);
        }

        public override bool SupportsAddingChildren => false;

        public DiaryWeekViewModel(
            string workingDirectory, DateTime? overrideWeekStart = null, Guid? overrideGuid = null) : base("", "")
        {
            this.workingDirectory = workingDirectory;
            var firstDayOfWeek = overrideWeekStart ?? DateTime.Now;
            guid = overrideGuid ?? Guid.NewGuid();
            while (firstDayOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                firstDayOfWeek -= TimeSpan.FromDays(1);
            }
            WeekStart = firstDayOfWeek;
            Name = $"Week {firstDayOfWeek.ToString("dd/MM/yyyy")}";
            SetDaysForStartOfWeek();
            SupportsDeleting = true;
            GenerateSummary();
        }

        internal static DiaryWeekViewModel FromDto(DiaryWeekDto dto, string workingDirectory, Guid guid)
        {
            return new DiaryWeekViewModel(workingDirectory, dto.WeekStart, guid) { ChildViewModels = new RangeObservableCollection<ViewModelBase>(dto.Days.Select(x => DiaryDayViewModel.FromDto(x))) };
        }

        protected override void OnCommitNameUpdate()
        {
            DateTime date = new DateTime();
            Name.Split(' ').FirstOrDefault(x => DateTime.TryParse(x, out date));
            if (date != default(DateTime))
            {
                WeekStart = date;
                Messenger.Send(new WeekChangedMessage());
            }
            SetDaysForStartOfWeek();
            base.OnCommitNameUpdate();
        }

        protected override void BindMessages()
        {
            Messenger.Register<SummaryChangedMessage>(this, (recipient, message) =>
            {
                if (ChildViewModels.Contains(message.Sender))
                {
                    GenerateSummary();
                }
            });
            Messenger.Register<SyncTagsMessage>(this, (recipient, message) =>
            {
                tags = new List<CustomTag>(message.Tags);
            });
            Messenger.Register<ViewModelRequestShowMessage<DiaryDayViewModel>>(
                this,
                (sender, message) =>
                {
                    if (ChildViewModels.Contains(message.ViewModel))
                    {
                        SelectedDay = message.ViewModel as DiaryDayViewModel;
                        foreach (var vm in ChildViewModels)
                        {
                            if (vm != message.ViewModel && vm.IsSelected)
                            {
                                vm.IsSelected = false;
                            }
                        }
                    }
                });
            base.BindMessages();
        }

        protected override void OnDelete()
        {
            Messenger.UnregisterAll(this);
            if (File.Exists(WritePath))
            {
                File.Delete(WritePath);
            }
            base.OnDelete();
        }

        protected override void OnChildrenChanged()
        {
            GenerateSummary();
            base.OnChildrenChanged();
        }

        protected override void OnShutdownStart(object? sender, EventArgs e)
        {
            Save();
            base.OnShutdownStart(sender, e);
        }

        public void Save()
        {
            File.WriteAllText(WritePath, JsonSerializer.Serialize(this.ToDto()));
        }

        private void SetDaysForStartOfWeek()
        {
            var firstDayOfWeek = WeekStart;

            for (int i = 0; i < 7; i++)
            {
                if (ChildViewModels.Count > i)
                {
                    ChildViewModels[i].Name = firstDayOfWeek.ToString("dd/MM/yyyy");
                    (ChildViewModels[i] as DiaryDayViewModel).DayOfWeek = firstDayOfWeek.DayOfWeek.ToString();
                }
                else
                {
                    var dayVM = new DiaryDayViewModel()
                    {
                        Name = firstDayOfWeek.ToString("dd/MM/yyyy"),
                        DayOfWeek = firstDayOfWeek.DayOfWeek.ToString()
                    };
                    ChildViewModels.Add(dayVM);
                }
                firstDayOfWeek += TimeSpan.FromDays(1);
            }
        }

        private void GenerateSummary()
        {
            var summaries = ChildViewModels.Select(x => new DayTagSummaryViewModel((x as DiaryDayViewModel).DayOfWeek, (x as DiaryDayViewModel).GenerateSummary(false))).ToList();

            foreach (var summary in summaries)
            {
                var items = summary.Tags;
                var newItems = tags.Where(x => x.IsIncluded).Select(x => new TagSummaryViewModel(x, (items.Any(y => (y.Tag != null ? y.Tag.Tag : "") == x.Tag) ? items.First(y => y.Tag.Tag == x.Tag).Time : TimeSpan.FromSeconds(0)).TotalSeconds));
                summary.Tags = new RangeObservableCollection<TagSummaryViewModel>(newItems);
            }

            Summary = new RangeObservableCollection<DayTagSummaryViewModel>(summaries);

            TagSummary = new RangeObservableCollection<TagSummaryViewModel>(
                tags.Where(x => x.IsIncluded)
                    .Select(x => new TagSummaryViewModel(
                        x,
                        Summary.SelectMany(x => x.Tags.Select(y => (y.Tag, y.Time)))
                            .Where(z => z.Tag.Tag == x.Tag)
                            .Sum(a => a.Time.TotalSeconds),
                        false)));

            var total = TimeSpan.FromSeconds(TagSummary.Sum(x => x.Time.TotalSeconds));
            FormattedTotal = TagSummary.Any() ? $"{total.Days * 24 + total.Hours:00}:{total.Minutes:00}" : "00:00";
        }
    }
}
