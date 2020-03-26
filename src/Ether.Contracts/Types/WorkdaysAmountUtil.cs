using System;

namespace Ether.Contracts.Types
{
    public static class WorkdaysAmountUtil
    {
        public static int CalculateWorkdaysAmount(DateTime start, DateTime end, params DateTime[] holidays)
        {
            start = start.Date;
            end = end.Date;

            if (start > end)
            {
                throw new ArgumentException("Incorrect last day " + end);
            }

            TimeSpan span = end - start;
            int businessDays = span.Days + 1;
            int fullWeekCount = businessDays / 7;

            if (businessDays > fullWeekCount * 7)
            {
                int firstDayOfWeek = start.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)start.DayOfWeek;
                int lastDayOfWeek = end.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)end.DayOfWeek;

                if (lastDayOfWeek < firstDayOfWeek)
                {
                    lastDayOfWeek += 7;
                }

                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)
                    {
                        businessDays -= 2;
                    }
                    else if (lastDayOfWeek >= 6)
                    {
                        businessDays -= 1;
                    }
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)
                {
                    businessDays -= 1;
                }
            }

            businessDays -= fullWeekCount + fullWeekCount;

            foreach (var holiday in holidays)
            {
                var bh = holiday.Date;

                if (start <= bh && bh <= end)
                {
                    --businessDays;
                }
            }

            return businessDays;
        }
    }
}
