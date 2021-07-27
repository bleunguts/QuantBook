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
    }
}
