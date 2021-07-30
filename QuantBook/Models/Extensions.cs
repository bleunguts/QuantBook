using Caliburn.Micro;
using QLNet;
using QuantBook.Models.Isda;
using QuantBook.Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models
{
    public static class Extensions
    {
        public static Option.Type ToOptionType(this OptionType optionType)
        {
            switch (optionType)
            {
                case OptionType.Call:
                    return Option.Type.Call;
                case OptionType.Put:
                    return Option.Type.Put;
            };

            throw new NotSupportedException($"optionType not supported: {optionType}");
        }

        public static Period ToPeriod(this string tenor)
        {
            int num = tenor.Remove(tenor.Length - 1, 1).To<int>();
            string sub = tenor.Substring(tenor.Length - 1, 1).ToUpper();

            if (sub == "M") return new Period(num, TimeUnit.Months);
            else if (sub == "Y") return new Period(num, TimeUnit.Years);

            throw new NotSupportedException($"cannot convert tenor, {tenor} month nor year detected.");
        }

        public static Period[] ToPeriods(this string[] tenors) 
        {
            return (tenors.Select(tenor => tenor.ToPeriod())).ToArray();
        }

        public static IEnumerable<(Period thePeriod, double theRate)> ToPeriodRates(this BindableCollection<IsdaRate> isdaRates)
        {
            var theIsdaRates = new List<(Period thePeriod, double theRate)>();
            foreach (var rate in isdaRates)
            {
                var fixedDays = Utilities.get_number_calendar_days(rate.SnapTime, rate.SpotDate.To<DateTime>());
                if (!string.IsNullOrEmpty(rate.FixedDayCountConvention))
                {
                    int tenor = Convert.ToInt32(rate.Tenor.Split('Y').First());
                    theIsdaRates.Add((new Period(tenor, TimeUnit.Years), Convert.ToDouble(rate.Rate)));
                }
                else
                {
                    (Period thePeriod, double theRate)? isdaRate = null;
                    switch (rate.Tenor)
                    {
                        case string tenor when tenor.Contains("M"):
                            isdaRate = (new Period(Convert.ToInt32(tenor.Split('M').First()), TimeUnit.Months), Convert.ToDouble(rate.Rate));
                            break;
                        case string tenor when tenor.Contains("Y"):
                            isdaRate = (new Period(Convert.ToInt32(tenor.Split('Y').First()), TimeUnit.Years), Convert.ToDouble(rate.Rate));
                            break;
                        throw new InvalidOperationException($"A non fixedDayaCountConvention must have a tenor in the format of iiM or iiY, tenor: {tenor}");
                    }
                    theIsdaRates.Add(isdaRate.Value);
                }
            }
            return theIsdaRates;
        }
    }
}
