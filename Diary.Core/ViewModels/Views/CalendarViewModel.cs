using Diary.Core.ViewModels.Base;
using MoreLinq;

namespace Diary.Core.ViewModels.Views
{
    public class CalendarViewModel : ViewModelBase
    {
        public CalendarViewModel(
            string workingDirectory, IEnumerable<DiaryWeekViewModel> startingWeeks)
            : base("Calendar", () => new DiaryYearViewModel(workingDirectory))
        {
            if (startingWeeks.Any())
            {
                var years = startingWeeks.Select(x => x.WeekStart.Year).Distinct();
                years.ForEach(x => AddChild(new DiaryYearViewModel(workingDirectory, startingWeeks, x.ToString())));
            }

            IsShowingChildren = false;
        }

        public override void AddChild(ViewModelBase viewModelToAdd = null, string name = "", int? index = null)
        {
            base.AddChild(viewModelToAdd, name, 0);
        }

        protected override void OnChildrenChanged()
        {
            IsShowingChildren = true;
            base.OnChildrenChanged();
        }
    }
}
