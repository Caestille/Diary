﻿using CoreUtilities.HelperClasses;
using Diary.Dtos;
using Diary.Extensions;
using Diary.Messages;
using Diary.Messages.Base;
using ModernThemables.ViewModels;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;
using ModernThemables.Messages;
using Diary.Models.Tagging;

namespace Diary.ViewModels.Views
{
	public class DiaryDayViewModel : ViewModelBase<DiaryEntryViewModel>
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
				ChildViewModels.ToList().ForEach(x => x.ShowFullDates = value);
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
			GenerateSummary();
			Loaded = true;
		}

		public DiaryDayViewModel(IEnumerable<DiaryEntryDto> entries) : base("")
		{
			this.entries = entries;
			GenerateSummary();
		}

		public void LoadEntries()
		{
			this.ChildViewModels.ToList().ForEach(x => x.Stop());
			this.ChildViewModels = new (entries.Select(DiaryEntryViewModel.FromDto));
			Loaded = true;
			entries = new List<DiaryEntryDto>();
		}

		public void ApplyDto(DiaryDayDto dto)
		{
			Notes = dto.Notes;
			DayOfWeek = dto.DayOfWeek;
			entries = dto.Entries;
			LoadEntries();
		}

		public void Stop()
		{
			this.ChildViewModels.ToList().ForEach(x => x.Stop());
		}

		internal static DiaryDayViewModel FromDto(DiaryDayDto dto)
		{
			var vm = new DiaryDayViewModel(dto.Entries) { Name = dto.Name, Notes = dto.Notes, DayOfWeek = dto.DayOfWeek };
			return vm;
		}

		public override void Select(GenericViewModelBase? sender = null)
		{
			IsSelected = true;
			if (!Loaded)
			{
				LoadEntries();
			}
			Messenger.Send(new ViewModelRequestShowMessage<DiaryDayViewModel>(this, sender ?? this));
			GenerateSummary();
		}

		protected override void OnChildrenChanged()
		{
			GenerateSummary();
			base.OnChildrenChanged();
			Messenger.Send(new EntryKeyDownMessage(this));
		}

		protected override void OnRequestDeleteReceived(ViewModelRequestDeleteMessage message)
		{
			if (message.ViewModel is DiaryEntryViewModel entryVm && ChildViewModels.Contains(entryVm))
			{
				if (lastFocusedVm == message.ViewModel)
				{
					lastFocusedVm = null;
				}
			}
			base.OnRequestDeleteReceived(message);
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
					Messenger.Send(new TagChangedMessage(this, message.Tag));
				}
			});
			Messenger.Register<EntryDateChangedMessage>(this, (recipient, message) =>
			{
				if (ChildViewModels.Contains(message.Sender))
				{
					if ((message.IsStartDate ? message.Sender.StartTime : message.Sender.EndTime).ToShortDateString() != Name)
					{
						var date = DateTime.Parse(Name);
						var toSet = message.Sender;
						if (message.IsStartDate)
						{
							toSet.StartTime =  message.Sender.StartTime.SetDate(date);
						}
						else
						{
							toSet.EndTime = message.Sender.EndTime.SetDate(date);
						}
					}
					if (SyncDates)
					{
						var index = ChildViewModels.IndexOf(message.Sender);
						if (message.IsStartDate && index > 0)
						{
							var entryBefore = ChildViewModels[index - 1];
							entryBefore.EndTime = message.NewValue;
						}
						else if (index < ChildViewModels.Count - 1)
						{
							var entryAfter = ChildViewModels[index + 1];
							entryAfter.StartTime = message.NewValue;
						}   
					}
					if (message.Sender.ShowFullDates != ShowFullDates)
					{
						message.Sender.ShowFullDates = ShowFullDates;
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
				var now = DateTime.Now.SetDate(DateTime.Parse(Name));
				var start = lastFocusedVm != null ? lastFocusedVm.EndTime : now - TimeSpan.FromMinutes(5);
				var end = lastFocusedVm != null
					? ChildViewModels.IndexOf(lastFocusedVm) < ChildViewModels.Count - 1
						? ChildViewModels[ChildViewModels.IndexOf(lastFocusedVm) + 1].StartTime
						: now < start ? start + TimeSpan.FromMinutes(5) : now
					: now < start ? start + TimeSpan.FromMinutes(5) : now;
				var newVM = new DiaryEntryViewModel()
				{
					StartTime = start,
					EndTime = end,
				};

				AddChild(newVM, index: lastFocusedVm != null ? ChildViewModels.IndexOf(lastFocusedVm) + 1 : null);
				Messenger.Send(new EntryKeyDownMessage(this, args));
			}
		}

		private void AddFirstChild()
		{
			var now = DateTime.Now.SetDate(DateTime.Parse(Name));
			var start = ChildViewModels.Any()? ChildViewModels.Last().EndTime : now - TimeSpan.FromMinutes(5);
			var newVM = new DiaryEntryViewModel()
			{
				StartTime = start,
				EndTime = now < start ? start + TimeSpan.FromMinutes(5) : now,
			};

			AddChild(newVM);
			Messenger.Send(new EntryKeyDownMessage(this));
		}

		private void SetFocus(DiaryEntryViewModel entry)
		{
			lastFocusedVm = entry;
			foreach (var vm in ChildViewModels) 
			{
				if (vm != lastFocusedVm)
				{
					vm.IsFocused = false;
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
			var total = new TagSummaryViewModel(
				new CustomTag() { IsIncluded = false, Tag = "Total" },
				summaries.Where(x => x.Tag.IsIncluded).Sum(x => x.Time.TotalSeconds));
			summaries.Add(total);
			TagSummaries = new RangeObservableCollection<TagSummaryViewModel>(summaries);
			if (sendMessage) Messenger.Send(new SummaryChangedMessage(this));
			return summaries;
		}
	}
}
