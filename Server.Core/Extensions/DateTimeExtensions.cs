using System;

namespace Server.Core.Extensions;

public static class DateTimeExtensions
{
    public static int GetWeekNumberOfMonth(this DateTime date)
    {
        date = date.Date;
        var firstMonthDay = new DateTime(date.Year, date.Month, 1);
        var firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);

        if (firstMonthMonday > date)
        {
            firstMonthDay = firstMonthDay.AddMonths(-1);
            firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
        }

        return (date - firstMonthMonday).Days / 7 + 1;
    }
}