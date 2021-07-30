using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models
{
    public static class Utilities
    {
        public static double? SafeExec(Func<double?> f)
        {
            try
            {
                return f();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return null;
        }

        public static int get_number_calendar_days(DateTime startDate, DateTime endDate)
        {
            DateTime date = startDate;
            int num = 0;
            while (date <= endDate)
            {
                date = date.AddDays(1);
                num++;
            }
            return num - 1;
        }

        public static DateTime get_previous_workday(DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            }
            while (date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday ||
                   IsHoliday(date));

            return date;
        }

        private static bool IsHoliday(DateTime date)
        {
            string ss = @"SELECT * FROM Holiday WHERE HDate = '" + date + "'";
            //DataTable holidayTable = xu_data.sql_load(ss);
            bool res = false;
            //if (holidayTable.Rows.Count > 0)
            //    res = true;
            return res;
        }
    }
}
