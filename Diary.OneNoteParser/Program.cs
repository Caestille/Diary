
using CoreUtilities.HelperClasses;
using Diary.Core.Extensions;
using Diary.Core.Models;
using Diary.Core.ViewModels.Base;
using Diary.Core.ViewModels.Views;
using System.Globalization;

var path = @"C:\Users\jward\OneDrive - Mercedes-Benz Grand Prix Ltd\Documents\ToParse.txt";

var lines = File.ReadAllLines(path).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
lines.Add("End");

var dayData = new List<(DateTime start, DateTime end, string entry)>();
var firstDay = DateTime.ParseExact(lines[0], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
var week = new DiaryWeekViewModel(@"C:\\Users\\jward\\AppData\\Local\\Diary", firstDay);
week.ChildViewModels.Clear();
var currentDay = firstDay;

foreach (var line in lines)
{
    var trimmedLine = line.Trim();
    if (DateTime.TryParse(trimmedLine, out var date) || line == lines.Last())
    {
        if (dayData.Any())
        {
            week.AddChild(new DiaryDayViewModel()
            {
                Name = currentDay.ToString("dd/MM/yyyy"),
                DayOfWeek = currentDay.DayOfWeek.ToString(),
                ChildViewModels = new RangeObservableCollection<ViewModelBase>(dayData.Select(x => new DiaryEntryViewModel()
                {
                    StartTime = x.start,
                    EndTime = x.end,
                    EntryText = x.entry,
                    Tag = new CustomTag() { Tag = "F1", IsIncluded = true }
                }))
            });
            dayData.Clear();
            currentDay += TimeSpan.FromDays(1);
        }
    }
    else
    {
        var splitLine = trimmedLine.Replace("    -", "").Split('-').SelectMany(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToList();
        var date1 = splitLine[0].Split(' ').ToList().First();
        var date2 = splitLine[1].Split(' ').ToList().First();
        var line2 = string.Join(' ', splitLine.Skip(2));
        dayData.Add((DateTime.ParseExact($"{firstDay.ToString("dd/MM/yyyy")} {date1}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None),
            DateTime.ParseExact($"{firstDay.ToString("dd/MM/yyyy")} {date2}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None),
            line2));
    }
}

week.Save();