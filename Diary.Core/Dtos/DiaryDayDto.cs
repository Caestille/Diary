namespace Diary.Core.Dtos
{
    public class DiaryDayDto
    {
        public DiaryDayDto() { }

        public string Name { get; set; }

        public string DayOfWeek { get; set; }

        public string Notes { get; set; }

        public List<DiaryEntryDto> Entries { get; set; }
    }
}
