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

            var theIsdaRates = FromIsdaRates(IsdaHelper.GetIsdaRates("USD", rateDate, rateDate));
            foreach (var r in theIsdaRates)
            {
                Console.WriteLine($"Rate: {r.theRate} [{r.thePeriod}]");
                Assert.That(r.theRate, Is.GreaterThan(0));
                Assert.That(r.thePeriod, Is.Not.Null);
            }
        }

        private static IEnumerable<(Period thePeriod, double theRate)> FromIsdaRates(BindableCollection<IsdaRate> isdaRates)
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

                            throw new InvalidOperationException($"A non fixedDayaCountConvention must have a tenor of either M or Y, tenor: {tenor}");
                    }
                    theIsdaRates.Add(isdaRate.Value);
                }             
            }
            return theIsdaRates;
        }
    }
}
