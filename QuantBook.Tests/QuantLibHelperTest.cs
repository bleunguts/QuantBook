using NUnit.Framework;
using QLNet;
using QuantBook.Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Tests
{
    public class QuantLibHelperTest
    {
        [Test]
        public void WhenPricingEuropeanOption()
        {
            var strike = 100.0;
            var spot = 110.0;
            var q = 0.0;
            var r = 0.1;
            var vol = 0.3;
            var yearsToMaturity = 1.0;
            var evalDate = DateTime.Now;
            var (value, delta, gamma, theta, rho, vega) = QuantLibHelper.EuropeanOption(OptionType.Call, evalDate, yearsToMaturity, strike, spot, q, r, vol, EuropeanEngineType.Analytic);
            Assert.Greater(value, 0);
            Assert.Greater(delta, 0);
            Assert.Greater(gamma, 0);
            Assert.Less(theta, 0);
            Assert.AreNotEqual(vega, 0);

        }

        [Test]
        public void WhenCalculatingImpliedVolForAnOption()
        {
            var quotedPrice = 90.0;
            var spot = 110.0;
            var strike = 100.0;
            var q = 0.0;
            var r = 0.1;
            var evalDate = DateTime.Now;
            var yearsToMaturity = 1;

            double impliedVol = QuantLibHelper.EuropeanOptionImpliedVol(OptionType.Call, evalDate, yearsToMaturity, strike, spot, q, r, quotedPrice);

            Assert.Greater(impliedVol, 0);
            Assert.That(impliedVol, Is.EqualTo(2.5).Within(5).Percent);
        }

        [Test]
        public void WhenCalculatingImpliedVolForAnOptionForTheUI()
        {
            var spot = 100.0;
            var strike = 100.0;
            var q = 0.01;
            var r = 0.05;
            var evalDate = DateTime.Now;
            var maturity = 0.1;
            var quotedPrice = 3.34;
            var vol = 0.249;

            var price = QuantLibHelper.EuropeanOption(OptionType.Call, evalDate, maturity, strike, spot, q, r, 0.249, EuropeanEngineType.Analytic);
            Console.WriteLine($"Price of call is {price}");
            Assert.That(price.Item1, Is.EqualTo(3.3470).Within(5).Percent);

            var impliedVol = QuantLibHelper.EuropeanOptionImpliedVol(OptionType.Call, evalDate, maturity, strike, spot, q, r, quotedPrice);
            Console.WriteLine($"ImpliedVol of call is {impliedVol}");
            Assert.That(impliedVol, Is.EqualTo(vol).Within(5).Percent);

            var prices = new double[] { 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };

            for (int i = 0; i < 10; i++)
            {
                var quotedPrice_ = quotedPrice + prices[i];
                double maturity_ = (i + 1.0) / 10.0;
                var impliedVol_ = QuantLibHelper.EuropeanOptionImpliedVol(OptionType.Call, evalDate, maturity_, strike, spot, q, r, quotedPrice_);
                Console.WriteLine($"ImpliedVol of call for m({maturity_}) and quotedPrice({quotedPrice_}) is {impliedVol_}");
                Assert.That(impliedVol, Is.GreaterThan(0));
            }
        }

        [Test]
        public void WhenValuingAnAmericanOption()
        {
            var spot = 100.0;
            var strike = 100.0;
            var divYield = 0.1;
            var rate = 0.1;
            var evalDate = DateTime.Now;
            var maturity = 0.1;
            var vol = 0.3;

            var (price, _,_,_,_,_ )= QuantLibHelper.AmericanOption(OptionType.Call, evalDate, maturity, strike, spot, divYield, rate, vol, AmericanEngineType.Barone_Adesi_Whaley);
            Console.WriteLine($"Price of american call option is {price}");
            Assert.That(price, Is.EqualTo(3.7527d).Within(5).Percent);
            var quotedPrice = price.Value + 0.5;
            var impliedVol = QuantLibHelper.AmericanOptionImpliedVol(OptionType.Call, evalDate, maturity, strike, spot, divYield, rate, quotedPrice);
        }

        [Test]
        public void WhenValuingARealWorldAmericanOption()
        {
            var spot = 100.0;
            double[] strikes = new[] { 99.0, 100.0, 101.0, 102.0, 103.0, 104.0, 105.0 };
            double[] vols = new[] { 0.23362, 0.21374, 0.20661, 0.20132, 0.19921, 0.19983, 0.20122 };
            double[] rates = new[] { 0.001049, 0.0012925, 0.001675, 0.00207, 0.002381, 0.0035140, 0.005841 };
            double dividend = spot * 0.01;
            int dividendFrequency = 3; // dividend paid quarterly

            //var evalDate = new Date(DateTime.Now);
            //var maturity = evalDate + 9 months;
            //var exDivDate = maturity - 16;

            var evalDate = new Date(28, Month.Jul, 2021);
            var maturity = new Date(21, Month.Apr, 2022);
            var exDivDate = new Date(5, Month.Apr, 2022);                     

            var results = QuantLibHelper.AmericanOptionRealWorld(OptionType.Call, evalDate, maturity, spot, strikes, vols, rates, dividend, dividendFrequency, exDivDate, AmericanEngineType.Barone_Adesi_Whaley, 100);
            foreach(var result in results)
            {
                Console.WriteLine($"Price of american call option @ ${result.strike} with spot ${spot} is {result.npv}");
                Assert.That(result.npv, Is.GreaterThan(0));
            }            
        }
    }
}

