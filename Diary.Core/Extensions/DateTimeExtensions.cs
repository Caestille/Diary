namespace Diary.Core.Extensions
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
    }
}
