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
    }
}
