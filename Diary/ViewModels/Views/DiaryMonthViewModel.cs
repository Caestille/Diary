using Diary.Messages;
using ModernThemables.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using CoreUtilities.HelperClasses;
using System.Globalization;

namespace Diary.ViewModels.Views
{
    public class DiaryMonthViewModel : ViewModelBase<DiaryWeekViewModel>
    {
        private string shortName;
        public string ShortName
        {
            get => shortName;
            set => SetProperty(ref shortName, value);
        }

        public DiaryMonthViewModel(
            string workingDirectory, IEnumerable<DiaryWeekViewModel> startingWeeks = null, string startingName = "")
            : base("", () => new DiaryWeekViewModel(workingDirectory))
        {
            if (!string.IsNullOrEmpty(startingName))
            {
                Name = startingName;
                ShortName = DateTime.ParseExact(Name, "MMMM", CultureInfo.InvariantCulture).ToString("MMM");
            }
            else
            {
                Name = DateTime.Now.ToString("MMMM");
                ShortName = DateTime.Now.ToString("MMM");
            }

            if (startingWeeks != null && startingWeeks.Any())
            {
                var weeks = startingWeeks
                    .Where(x => x.WeekStart.Month == DateTime.ParseExact(Name, "MMMM", CultureInfo.InvariantCulture).Month)
                    .OrderBy(x => x.WeekStart);
                foreach (var week in weeks)
                {
                    AddChild(week);
                }
            }
            else
            {
                AddChild();
                ChildViewModels.Last().SelectCommand.Execute(this);
            }
        }

        protected override void BindMessages()
        {
            Messenger.Register<WeekChangedMessage>(this, (recipient, sender) => 
            {
                ChildViewModels = new (ChildViewModels.OrderByDescending(x => x.WeekStart));
            });
            base.BindMessages();
        }
    }
}
