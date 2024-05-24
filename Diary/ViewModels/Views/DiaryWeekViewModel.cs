using CoreUtilities.HelperClasses;
using Diary.Dtos;
using Diary.Extensions;
using Diary.Messages;
using Diary.Messages.Base;
using Diary.Models.Tagging;
using ModernThemables.ViewModels;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Timers;
using System.Windows.Input;
using ModernThemables.Messages;

namespace Diary.ViewModels.Views
{
	public class DiaryWeekViewModel : AliasableViewModelBase<DiaryDayViewModel>
    {
        private string workingDirectory;
        private System.Timers.Timer autoSaveTimer;

        public string WritePath => $"{Path.Combine(workingDirectory, guid.ToString())}.json";

        private List<CustomTag> tags = new List<CustomTag>();

        private Guid guid;

		private object locker = new();
		private bool isReloading;

        public ICommand ShowWeekSummaryCommand => new RelayCommand(() =>
		{
			_ = Task.Run(() =>
			{
				foreach (var day in this.ChildViewModels.Cast<DiaryDayViewModel>())
				{
					if (!day.Loaded)
					{
						day.LoadEntries();
					}
				}
				GenerateSummary();
				ShowWeekSummary = true;
			});
		});
		
        public ICommand CloseWeekSummaryCommand => new RelayCommand(() => ShowWeekSummary = false);

        public ICommand SaveCommand => new AsyncRelayCommand(Save);

		public ICommand OpenJsonCommand => new RelayCommand(() => 
		{
			Process.Start(new ProcessStartInfo(this.WritePath)
				{
					UseShellExecute = true,
					CreateNoWindow = true
				});
		});

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
                ChildViewModels.ToList().ForEach(x => x.ShowFullDates = value);
            }
        }

        private bool syncDates;
        public bool SyncDates
        {
            get => syncDates;
            set
            {
                SetProperty(ref syncDates, value);
                ChildViewModels.ToList().ForEach(x => x.SyncDates = value);
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

        private bool isAutoSaveEnabled = true;
        public bool IsAutoSaveEnabled
        {
            get => isAutoSaveEnabled;
            set => SetProperty(ref isAutoSaveEnabled, value);
        }

        private bool isSaving;
        public bool IsSaving
        {
            get => isSaving;
            set => SetProperty(ref isSaving, value);
        }

        private bool canSave;
        public bool CanSave
        {
            get => canSave;
            set => SetProperty(ref canSave, value);
        }

        public DiaryWeekViewModel(
            string workingDirectory, DateTime? overrideWeekStart = null, Guid? overrideGuid = null) : base("", "")
        {
            this.workingDirectory = workingDirectory;
            var firstDayOfWeek = (overrideWeekStart ?? DateTime.Now).FirstDayOfWeek();
            guid = overrideGuid ?? Guid.NewGuid();
            WeekStart = firstDayOfWeek;
            Name = $"Week {firstDayOfWeek.ToString("dd/MM/yyyy")}";
            SetDaysForStartOfWeek();
            GenerateSummary();

            autoSaveTimer = new System.Timers.Timer(5000);
            autoSaveTimer.Elapsed += Timer_Elapsed;
            autoSaveTimer.Start();
        }

        public static DiaryWeekViewModel FromDto(DiaryWeekDto dto, string workingDirectory, Guid guid)
        {
            return new DiaryWeekViewModel(
                workingDirectory,
                dto.WeekStart,
                guid)
            {
                ChildViewModels = new (
                    dto.Days.Select(x => DiaryDayViewModel.FromDto(x)))
            };
        }

		protected override void OnCommitAliasUpdate()
		{
            DateTime date = new DateTime();
            Name.Split(' ').FirstOrDefault(x => DateTime.TryParse(x, out date));
            if (date != default(DateTime))
            {
                WeekStart = date;
                Messenger.Send(new WeekChangedMessage());
            }
            SetDaysForStartOfWeek();
            base.OnCommitAliasUpdate();
        }

        protected override void BindMessages()
        {
            Messenger.Register<SummaryChangedMessage>(this, (recipient, message) =>
            {
                if (ChildViewModels.Contains(message.Sender))
				{
					CanSave = GenerateSummary();
                }
            });
			Messenger.Register<TagChangedMessage>(this, (recipient, message) =>
			{
				if (ChildViewModels.Contains(message.Sender))
				{
					CanSave = true;
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
                        SelectedDay = message.ViewModel;
                        foreach (var vm in ChildViewModels)
                        {
                            if (vm != message.ViewModel && vm.IsSelected)
                            {
                                vm.IsSelected = false;
                            }
                        }
                    }
                });
            Messenger.Register<EntryKeyDownMessage>(this, (recipient, message) =>
            {
				lock (locker)
				{
					if (!isReloading && ChildViewModels.Contains(message.Sender))
					{
						var content = File.ReadAllText(this.WritePath);
						var self = JsonSerializer.Serialize(this.ToDto());
						CanSave = self != content;
					}
				}
            });
            base.BindMessages();
        }

		public override void Select(GenericViewModelBase? sender = null)
		{
			ChildViewModels.Where(x => !x.Loaded).ToList().ForEach(x => x.LoadEntries());
			base.Select(sender);
		}

		public override void OnDelete()
		{
			Messenger.UnregisterAll(this);
			if (File.Exists(WritePath))
			{
				File.Delete(WritePath);
			}
        }

        protected override void OnChildrenChanged()
        {
			CanSave = true;
            GenerateSummary();
            base.OnChildrenChanged();
        }

        protected override void OnShutdownStart(object? sender, EventArgs e)
        {
            CanSave = false;
            autoSaveTimer.Stop();
            autoSaveTimer.Elapsed -= Timer_Elapsed;
            _ = Save();
			this.ChildViewModels.ToList().ForEach(x => x.Stop());
            base.OnShutdownStart(sender, e);
		}

		private bool Reload()
		{
			if (!ChildViewModels.All(x => x.Loaded)) return false;

			isReloading = true;

			var content = File.ReadAllText(this.WritePath);
			var self = JsonSerializer.Serialize(this.ToDto());
			if (self == content) { isReloading = false; return false; }

			IEnumerable<DiaryDayDto>? days = null;
			try
			{
				days = JsonSerializer.Deserialize<DiaryWeekDto>(content)?.Days;
			}
			catch
			{
				//Failed to deserialise json, just return
			}

			if (days is null) { isReloading = false; return false; }
			// TODO messagebox to confirm this failed to user

			foreach (var day in ChildViewModels.Cast<DiaryDayViewModel>())
			{
				var matchingDto = days.FirstOrDefault(x => x.Name == day.Name);
				if (matchingDto == null) continue;
				day.ApplyDto(matchingDto);
			}

			isReloading = false;

			return true;
		}

		private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
			lock (locker)
			{
				if (CanSave)
				{
					if (IsAutoSaveEnabled)
					{
						_ = Save();
					}
				}
				else
				{
					Reload();
				}
			}
        }

        public async Task Save()
        {
            var days = this.ChildViewModels.Cast<DiaryDayViewModel>();
            if (days.Any(x => x.Loaded))
            {
                foreach (var day in days.Where(x => !x.Loaded))
                {
                    day.LoadEntries();
                }
                IsSaving = true;
                CanSave = false;
                File.WriteAllText(WritePath, JsonSerializer.Serialize(this.ToDto()));
                await Task.Delay(500);
                IsSaving = false;
            }
            
        }

        private void SetDaysForStartOfWeek()
        {
            var firstDayOfWeek = WeekStart;

            for (int i = 0; i < 7; i++)
            {
                if (ChildViewModels.Count > i)
                {
                    ChildViewModels[i].Name = firstDayOfWeek.ToString("dd/MM/yyyy");
                    ChildViewModels[i].DayOfWeek = firstDayOfWeek.DayOfWeek.ToString();
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

        private bool GenerateSummary()
        {
            var summaries = ChildViewModels.Select(x => new DayTagSummaryViewModel(
                x.DayOfWeek,
                x.GenerateSummary(false))).ToList();

            foreach (var summary in summaries)
            {
                var items = summary.Tags;
                var newItems = tags
                    .Where(x => x.IsIncluded)
                    .Select(x => new TagSummaryViewModel(
                        x,
                        (items.Any(y => (y.Tag != null ? y.Tag.Tag : "") == x.Tag)
                            ? items.First(y => y.Tag.Tag == x.Tag).Time
                            : TimeSpan.FromSeconds(0)).TotalSeconds));
                summary.Tags = new RangeObservableCollection<TagSummaryViewModel>(newItems);
            }

			var changed = Summary == null || (Summary.Count == summaries.Count && !Summary.All(x => summaries.Any(y => y.Equals(x))));
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

			return changed;
        }
    }
}
