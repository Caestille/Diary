using ModernThemables.ViewModels;
using System.IO;

namespace Diary.ViewModels.Views
{
    public class CalendarViewModel : ViewModelBase<DiaryYearViewModel>
    {
        public CalendarViewModel(
            string workingDirectory, IEnumerable<DiaryWeekViewModel> startingWeeks)
            : base("Calendar", () => new DiaryYearViewModel(workingDirectory))
        {
            var backupDir = Path.Combine(workingDirectory, "Backup");
            if (Directory.Exists(backupDir))
            {
                Directory.Delete(backupDir, true);
            }
            Directory.CreateDirectory(backupDir);

            var files = Directory.GetFiles(workingDirectory).ToList();
            files.ForEach(x => File.Copy(x, Path.Combine(backupDir, Path.GetFileName(x))));

            if (startingWeeks.Any())
            {
                var years = startingWeeks.Select(x => x.WeekStart.Year).Distinct().ToList();
                years.ForEach(x => AddChild(new DiaryYearViewModel(workingDirectory, startingWeeks, x.ToString())));
            }
        }

		public override void AddChild(DiaryYearViewModel? viewModelToAdd = null, string name = "", int? index = null)
		{
            base.AddChild(viewModelToAdd, name, 0);
        }
    }
}
