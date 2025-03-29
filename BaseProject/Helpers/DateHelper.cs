namespace SupportMe.Helpers
{
    public static class DateHelper
    {
        [Flags]
        public enum Weekday
        {
            Sunday = 1,
            Monday = 2,
            Tuesday = 4,
            Wednesday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64
        }

        internal static List<DayOfWeek> Map(Weekday weekday) =>
            (from Weekday c in Enum.GetValues(typeof(Weekday))
             where (weekday & c) != 0
             select MapSingle(c)).ToList();

        private static DayOfWeek MapSingle(Weekday weekday)
        {
            switch (weekday)
            {
                case Weekday.Sunday: return DayOfWeek.Sunday;
                case Weekday.Monday: return DayOfWeek.Monday;
                case Weekday.Tuesday: return DayOfWeek.Tuesday;
                case Weekday.Wednesday: return DayOfWeek.Wednesday;
                case Weekday.Thursday: return DayOfWeek.Thursday;
                case Weekday.Friday: return DayOfWeek.Friday;
                case Weekday.Saturday: return DayOfWeek.Saturday;
                default: throw new ArgumentOutOfRangeException(nameof(weekday), weekday, $"Not supported weekday: {weekday}");
            }
        }

        public static DateTime GetDateInZoneTime(DateTime utcDate, string zoneTime, int? utcOffset = null)
        {
            DateTime result = utcDate;

            try
            {
                if (utcOffset.HasValue)
                {
                    result = utcDate.AddMinutes(utcOffset.Value);
                    return result;
                }

                TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById(zoneTime);
                result = TimeZoneInfo.ConvertTimeFromUtc(utcDate, tzone);
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        /// <summary>
        /// Retuns a UTC date from a local date, using zoneTime and utcOffset from database
        /// </summary>
        /// <param name="localDate"></param>
        /// <param name="zoneTime"></param>
        /// <param name="utcOffset"></param>
        /// <returns></returns>
        public static DateTime GetUTCDateFromLocalDate(DateTime localDate, string zoneTime, int? utcOffset = null)
        {
            DateTime result = localDate;

            try
            {
                if (utcOffset.HasValue)
                {
                    result = localDate.AddMinutes(-utcOffset.Value);
                    return result;
                }

                TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById(zoneTime);
                result = TimeZoneInfo.ConvertTimeToUtc(localDate, tzone);

            }
            catch (Exception ex)
            {
            }

            return result;
        }


        public static string GetGMTDescriptionText(string zoneTime, int? utcOffset = null)
        {
            TimeSpan timeSpan = new TimeSpan();

            if (utcOffset.HasValue)
            {
                timeSpan = DateTime.UtcNow.AddMinutes(utcOffset.Value) - DateTime.UtcNow;
            }
            else
            {
                TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(zoneTime);
                timeSpan = zone.GetUtcOffset(DateTime.UtcNow);
            }

            string gmt = String.Join(":", timeSpan.ToString().Split(':').Take(2));

            return $"(GMT{gmt})";
        }

        public static bool IsValidForDate(DateTime date, DateTime? fromDate, DateTime? toDate, int? weekdays)
        {
            string weekday = date.ToString("dddd", new System.Globalization.CultureInfo("en-US"));

            return isDayWithinRange(date, fromDate, toDate) && isWeekdayWithinRange(date.DayOfWeek, weekdays);
        }

        public static bool IsValidForTime(TimeSpan time, TimeSpan? fromTime, TimeSpan? toTime)
        {
            bool result = false;

            if (fromTime.HasValue && toTime.HasValue)
            {
                // see if start comes before end
                if (fromTime < toTime)
                {
                    result = fromTime <= time && time <= toTime;
                }
                else
                {
                    // start is after end, so do the inverse comparison
                    result = !(toTime < time && time < fromTime);
                }
            }
            else
                result = true;

            return result;
        }

        public static bool IsValidForAnticipation(DateTime showDate, DateTime purchaseDate, int? fromAnticipationDays, int? toAnticipationDays)
        {
            bool result = true;

            int DiffDays = (showDate.Date - purchaseDate.Date).Days;

            if (fromAnticipationDays.HasValue)
            {
                result = DiffDays >= fromAnticipationDays;
            }

            if (toAnticipationDays.HasValue)
            {
                result = DiffDays <= toAnticipationDays || toAnticipationDays == 0;
            }

            return result;
        }

        public static DateTime GetDateWithoutTime(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }

        public static DateTime DateTimeWithoutTimeZone(DateTime date)
        {
            return new DateTime(date.Ticks, DateTimeKind.Unspecified);
        }

        public static DateTime DateTimeUTCTimeZone(DateTime date)
        {
            return new DateTime(date.Ticks, DateTimeKind.Utc);
        }

        private static bool isDayWithinRange(DateTime dateToCheck, DateTime? fromDate, DateTime? toDate)
        {
            bool result = true;

            if (fromDate.HasValue && fromDate.Value.Date.CompareTo(dateToCheck.Date) > 0)
                result = false;

            if (toDate.HasValue && toDate.Value.Date.CompareTo(dateToCheck.Date) < 0)
                result = false;

            return result;
        }

        private static bool isWeekdayWithinRange(DayOfWeek dayToCheck, int? weekdays)
        {
            if (!weekdays.HasValue)
                return true; //no need to check by week day

            List<DayOfWeek> daysOfWeek = Map((Weekday)weekdays.Value);

            return Array.IndexOf(daysOfWeek.ToArray(), dayToCheck) != -1;
        }
        public static int? CalculateAge(DateTimeOffset? borndate)
        {
            if (borndate == null) return null;

            DateTime _borndate = new DateTime(borndate.Value.Year, borndate.Value.Month, borndate.Value.Day);
            DateTime now = DateTime.Now.Date;

            int age = now.Year - _borndate.Year;

            if (now.Month < _borndate.Month || (now.Month == _borndate.Month && now.Day < _borndate.Day))
                age--;

            return age;
        }
        public static int GetUtcMinutesOffset(string timeZone)
        {
            if (!string.IsNullOrEmpty(timeZone))
            {
                return (int)TimeZoneInfo.FindSystemTimeZoneById(timeZone.Trim()).BaseUtcOffset.TotalMinutes;
            }
            else return 0;
        }

        public static List<DateTime> MissingDays(DateTime? StartDate, DateTime? EndDate, List<DateTime> presentDates, DatePart datePart)
        {
            List<DateTime> missingDays = new List<DateTime>();

            StartDate ??= presentDates.Any() ? presentDates.OrderBy(d => d).FirstOrDefault() : DateTime.Now;
            EndDate ??= presentDates.Any() ? (DateTime.Now > presentDates.Max() ? DateTime.Now : presentDates.Max()) : DateTime.Now;


            DateTime currentDate = StartDate.Value.Date;
            DateTime endDateAdjusted = (datePart == DatePart.HOUR && EndDate.HasValue)
            ? EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
            : EndDate.GetValueOrDefault();


            while (currentDate <= endDateAdjusted)
            {
                if (!presentDates.Any(date =>
                {
                    switch (datePart)
                    {
                        case DatePart.YEAR:
                            return date.Year == currentDate.Year;
                        case DatePart.MONTH:
                            return date.Year == currentDate.Year && date.Month == currentDate.Month;
                        case DatePart.DAY:
                            return date.Year == currentDate.Year && date.Month == currentDate.Month && date.Day == currentDate.Day;
                        default:
                            return date == currentDate;
                    }
                }))
                {
                    missingDays.Add(currentDate);
                }

                currentDate = GetNextDate(currentDate, datePart);
            }

            return missingDays;

        }

        private static DateTime GetNextDate(DateTime currentDate, DatePart datePart)
        {
            switch (datePart)
            {
                case DatePart.HOUR:
                    return currentDate.AddHours(1);
                case DatePart.DAY:
                    return currentDate.AddDays(1);
                case DatePart.MONTH:
                    return currentDate.AddMonths(1);
                case DatePart.YEAR:
                    return currentDate.AddYears(1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(datePart), datePart, "Tipo de fecha no válido.");
            }
        }
        public enum DatePart { HOUR, DAY, MONTH, YEAR, }

    }
}
