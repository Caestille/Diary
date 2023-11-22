using Diary.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Diary.ViewModels.Views
{
    public class TagSummaryViewModel : ObservableObject
    {
        private CustomTag tag;
        public CustomTag Tag
        {
            get => tag;
            set => SetProperty(ref tag, value);
        }

        private string day;
        public string Day
        {
            get => day;
            set => SetProperty(ref day, value);
        }

        private TimeSpan time;
        public TimeSpan Time
        {
            get => time;
            set => SetProperty(ref time, value);
        }

        private string formattedTime;
        public string FormattedTime
        {
            get => formattedTime;
            set => SetProperty(ref formattedTime, value);
        }

        public TagSummaryViewModel(CustomTag tag, double timespanSeconds, bool formatAsDecimal = true)
        {
            Tag = tag;
            Time = TimeSpan.FromSeconds(timespanSeconds);
            FormattedTime = $"{Time.Days * 24 + Time.Hours:00}:{Time.Minutes:00}{(formatAsDecimal ? $" ({Time.TotalHours:0.00})": "")}";
        }
    }
}
