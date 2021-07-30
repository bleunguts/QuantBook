using QLNet;
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
    }
}
