using System;
using System.Collections.Generic;
using System.Text;

namespace TiffCSharp
{
    /// <summary>
    /// TIFF and STACK files use julian day and milliseconds to midnight to record time. 
    /// This class provides static methods that perform conversion between those and .NET DateTime structure.
    /// </summary>
    public class JulianDatetime
    {
        /// <summary>
        /// Convert local DateTime to Julian day and milliseconds from midnight.
        /// </summary>
        /// <param name="local">Local DateTime.</param>
        /// <param name="julian">Julian day.</param>
        /// <param name="millisec">Milliseconds from midnight.</param>
        public static void getSTKdatetime(DateTime local, out long julian, out long millisec)
        {
            DateTime utc = local.ToUniversalTime();
            int a, b = 0;
            int work_year = utc.Year;
            int work_month = utc.Month;
            int work_day = utc.Day;

            // correct for negative year
            if (work_year < 0)
            {
                work_year++;
            }

            if (work_month <= 2)
            {
                work_year--;
                work_month += 12;
            }

            // deal with Gregorian calendar
            if (work_year * 10000.0 + work_month * 100.0 + work_day >= 15821015.0)
            {
                a = (int)(work_year / 100.0);
                b = (int)(2 - a + a / 4);
            }

            julian = (long)(365.25 * work_year) + (long)(30.6001 * (work_month + 1)) + work_day + 1720994L + b;
            millisec = 1000 * (((long)utc.Hour) * 3600 + ((long)utc.Minute) * 60 + ((long)utc.Second)) + utc.Millisecond;

        }

        /// <summary>
        /// Convert Julian day and milliseconds from midnight to local DateTime.
        /// </summary>
        /// <param name="julian">Julian day.</param>
        /// <param name="millsec">Milliseconds from midnight.</param>
        /// <param name="local">Local DateTime.</param>
        /// <returns>True if converted succesfully. False if not a valid time.</returns>
        public static bool getLocalTime(long julian, long millsec, out DateTime local)
        {
            long a, b, c, d, e, alpha;
            long z = julian + 1;

            // dealing with Gregorian calendar reform
            if (z < 2299161L)
            {
                a = z;
            }
            else
            {
                alpha = (long)((z - 1867216.25) / 36524.25);
                a = z + 1 + alpha - alpha / 4;
            }

            b = (a > 1721423L ? a + 1524 : a + 1158);
            c = (long)((b - 122.1) / 365.25);
            d = (long)(365.25 * c);
            e = (long)((b - d) / 30.6001);

            int day = (int)(b - d - (long)(30.6001 * e));
            int month = (int)((e < 13.5) ? e - 1 : e - 13);
            int year = (int)((month > 2.5) ? (c - 4716) : c - 4715);

            int millUtc = (int)(millsec % 100);
            millsec /= 1000;

            int sec = (int)(millsec % 60);
            millsec /= 60;

            int min = (int)(millsec % 60);
            int hour = (int)(millsec / 60);

            try
            {
                DateTime utc = new DateTime(year, month, day, hour, min, sec, millUtc, DateTimeKind.Utc);
                local = utc.ToLocalTime();
                return true;
            }
            catch
            {
                local = DateTime.MinValue;
                return false;
            }
        }
    }
}
