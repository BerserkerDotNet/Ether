using System;

namespace Ether.Tests.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AddBusinessDays(this DateTime dateTime, int days)
        {
            var dayOfWeek = (int)dateTime.DayOfWeek - 1;
            var monday = dateTime.GetMondayOfCurrentWeek();
            var numberOfWeeks = days < 5 ? 0 : Math.Floor(days / 5.0D);
            return monday.AddDays(days + dayOfWeek + (numberOfWeeks * 2));
        }

        public static DateTime GetMondayOfCurrentWeek(this DateTime dateTime)
        {
            var dayOfWeek = (int)dateTime.DayOfWeek;
            return dateTime.AddDays(-(dayOfWeek - 1));
        }
    }
}
