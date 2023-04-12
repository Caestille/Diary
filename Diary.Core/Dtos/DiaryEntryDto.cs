namespace Diary.Core.Dtos
{
    public class DiaryEntryDto
    {
        public DiaryEntryDto() { }  

        public string Entry { get; set; }

        public string Tag { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}
