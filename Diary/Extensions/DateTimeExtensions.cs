namespace Diary.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FirstDayOfWeek(this DateTime dateTime)
        {
            var firstDayOfWeek = dateTime;
            while (firstDayOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                firstDayOfWeek -= TimeSpan.FromDays(1);
            }
            return firstDayOfWeek;
        }

		public static DateTime SetDate(this DateTime dateTime, DateTime newDay)
		{
			return new DateTime(newDay.Year, newDay.Month, newDay.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
		}
    }
}
