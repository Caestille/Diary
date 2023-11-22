using CoreUtilities.HelperClasses;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Diary.ViewModels.Views
{
    public class DayTagSummaryViewModel : ObservableObject
    {
        private string day;
        public string Day
        {
            get => day;
            set => SetProperty(ref day, value);
        }

        private RangeObservableCollection<TagSummaryViewModel> tags;
        public RangeObservableCollection<TagSummaryViewModel> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value);
        }

        private TimeSpan time;
        public TimeSpan Time
        {
            get => time;
            set => SetProperty(ref time, value);
        }

        private string formattedTotal;
        public string FormattedTotal
        {
            get => formattedTotal;
            set => SetProperty(ref formattedTotal, value);
        }

        public DayTagSummaryViewModel(string day, IList<TagSummaryViewModel> tags)
        {
            Day = day;
            Tags = new RangeObservableCollection<TagSummaryViewModel>(tags);
            Time = TimeSpan.FromSeconds(Tags.Where(x => x.Tag?.IsIncluded ?? false).Sum(x => x.Time.TotalSeconds));
            FormattedTotal = Tags.Any() ? $"{Time.Hours:00}:{Time.Minutes:00}" : "00:00";
        }
    }
}
