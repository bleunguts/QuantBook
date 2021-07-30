using Caliburn.Micro;
using NUnit.Framework;
using QLNet;
using QuantBook.Models;
using QuantBook.Models.Isda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Tests
{
    public class IsdaHelperTest
    {
        [Test]
        public void WhenGettingIsdaRatesFromTheWeb()
        {
            DateTime evalDate = DateTime.Today;
            DateTime rateDate = Utilities.get_previous_workday(evalDate);

            var theIsdaRates = IsdaHelper.GetIsdaRates("USD", rateDate, rateDate).ToPeriodRates();
            foreach (var r in theIsdaRates)
            {
                Console.WriteLine($"Rate: {r.theRate} [{r.thePeriod}]");
                Assert.That(r.theRate, Is.GreaterThan(0));
                Assert.That(r.thePeriod, Is.Not.Null);
            }
        }

       
    }
}
