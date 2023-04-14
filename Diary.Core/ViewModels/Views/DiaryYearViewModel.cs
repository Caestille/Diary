using Diary.Core.ViewModels.Base;
using System.Globalization;

namespace Diary.Core.ViewModels.Views
{
    public class DiaryYearViewModel : ViewModelBase
    {
        public DiaryYearViewModel(
            string workingDirectory, IEnumerable<DiaryWeekViewModel> startingWeeks = null, string startingName = "")
            : base("", () => new DiaryMonthViewModel(workingDirectory))
        {
            SupportsDeleting = true;

            if (!string.IsNullOrEmpty(startingName))
            {
                Name = startingName;
            }
            else
            {
                Name = DateTime.Now.Year.ToString();
            }

            if (startingWeeks != null && startingWeeks.Any())
            {
                var months = startingWeeks
                    .Where(x => x.WeekStart.Year == int.Parse(Name))
                    .Select(x => x.WeekStart.Month)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                months.ForEach(
                    x => AddChild(new DiaryMonthViewModel(
                        workingDirectory,
                        startingWeeks.Where(x => x.WeekStart.Year == int.Parse(Name)),
                        CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(x))));
            }
            else
            {
                AddChild();
                IsShowingChildren = true;
            }
        }

        public override void AddChild(ViewModelBase viewModelToAdd = null, string name = "", int? index = null)
        {
            base.AddChild(viewModelToAdd, name, 0);
        }
    }
}
