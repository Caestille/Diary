using Diary.Messages;
using ModernThemables.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernThemables.Charting.Interfaces;
using System.Collections.ObjectModel;
using ModernThemables.Charting.Models;
using ModernThemables.Charting.Models.BarChart;
using System.Windows.Media;
using CoreUtilities.HelperClasses.Extensions;
using ModernThemables.Charting.Models.Brushes;
using System.Windows;

namespace Diary.ViewModels.Views
{
	public class StatsViewModel : ViewModelBase
	{
		private bool ready;
		private CalendarViewModel calendar;
		
		private ObservableCollection<ISeries> displayedSeries = new();
		public ObservableCollection<ISeries> DisplayedSeries
		{
			get => displayedSeries;
			set => SetProperty(ref displayedSeries, value);
		}

		public StatsViewModel(CalendarViewModel calendar)
			: base("Stats")
		{
			this.calendar = calendar;
		}

		public void AllWeeksLoaded()
		{
			ready = true;
			GenerateData();
		}

		protected override void BindMessages()
		{
			Messenger.Register<SummaryChangedMessage>(this, (recipient, sender) =>
			{
				if (!ready) return;

				GenerateData();
			});
			base.BindMessages();
		}

		private void GenerateData()
		{
			var weeks = calendar.ChildViewModels.SelectMany(x => x.ChildViewModels).SelectMany(x => x.ChildViewModels).ToList().OrderBy(x => x.WeekStart);
			var tags = weeks.SelectMany(x => x.ChildViewModels).SelectMany(x => x.ChildViewModels).Select(x => x.Tag?.Tag).Distinct().Where(x => x != null).ToList();
			var vals = new Dictionary<DateTime, Dictionary<string, TimeSpan>>();
			foreach (var week in weeks)
			{
				if (!vals.ContainsKey(week.WeekStart))
				{
					vals[week.WeekStart] = new Dictionary<string, TimeSpan>();
				}

				foreach (var day in week.ChildViewModels)
				{
					foreach (var entry in day.ChildViewModels)
					{
						if (entry.Tag?.Tag == null) continue;

						if (!vals[week.WeekStart].ContainsKey(entry.Tag?.Tag!))
						{
							vals[week.WeekStart][entry.Tag.Tag] = TimeSpan.Zero;
						}
						vals[week.WeekStart][entry.Tag.Tag] += entry.Span;
					}
				}
			}

			var series = new List<ISeries>();
			foreach (var tag in tags)
			{
				var colour = ColorExtensions.RandomColour();
				series.Add(new Series()
				{
					Name = tag,
					Fill = new SolidBrush(colour),
					Stroke = new SolidBrush(colour),
					Values = new ObservableCollection<IChartEntity>(vals.Select(x => new LabelledBar(x.Value.ContainsKey(tag) ? x.Value[tag].TotalMinutes : 0, x.Key.ToShortDateString(), 0)))
				});
			}

			Application.Current.Dispatcher.Invoke(() => DisplayedSeries = new ObservableCollection<ISeries>(series));
		}
	}
}
