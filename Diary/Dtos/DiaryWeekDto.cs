namespace Diary.Dtos
{
    public class DiaryWeekDto
    {
        public DiaryWeekDto() { }

        public DateTime WeekStart { get; set; }

        public List<DiaryDayDto> Days { get; set; }
    }
}
