using Chart3DControl;
using NUnit.Framework;
using QuantBook.Models.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Tests
{
    public class OptionHelperTest
    {
        [Test]
        public void WhenPlottingGreeksZValuesAreValid()
        {
            var dataSeries = new DataSeries3D();
            var z = OptionPlotHelper.PlotGreeks(dataSeries, GreekTypeEnum.Price, OptionType.CALL, 100, 0.1, 0.04, 0.3);
            AssertZValue(z, rounding: 1);
        }        

        [Test]
        public void WhenPricingACallOption()
        {
            // For a call option that is deep ITM price is gt 0 should be expensive
            var deepItmPrice = OptionHelper.BlackScholes(OptionType.CALL, 510, 100, 0.1, 0.04, 2.0, 0.3);
            Assert.Greater(deepItmPrice, 0);
            Assert.That(deepItmPrice, Is.EqualTo(370.4568).Within(1).Percent);

            // For a call option that is ITM price is gt 0 should be relative expensive
            var itmPrice = OptionHelper.BlackScholes(OptionType.CALL, 110, 100, 0.1, 0.04, 2.0, 0.3);            
            Assert.Greater(itmPrice, 0);
            Assert.That(itmPrice, Is.EqualTo(24.1620).Within(1).Percent);

            // For a call option that is ATM price is gt 0 should be fair priced
            var atmPrice = OptionHelper.BlackScholes(OptionType.CALL, 100, 100, 0.1, 0.04, 2.0, 0.3);
            Assert.Greater(atmPrice, 0);
            Assert.That(atmPrice, Is.EqualTo(17.9866).Within(1).Percent);

            // For a call option that is OTM price is cheaper
            var otmPrice = OptionHelper.BlackScholes(OptionType.CALL, 70, 100, 0.1, 0.04, 2.0, 0.3);
            Assert.Greater(otmPrice, 0);
            Assert.That(otmPrice, Is.EqualTo(4.6253).Within(1).Percent);

            // For a call option that is Deep OTM price is worthless
            var deepOtmPrice = OptionHelper.BlackScholes(OptionType.CALL, 2, 100, 0.1, 0.04, 2.0, 0.3);            
            Assert.AreEqual(deepOtmPrice, 0);
            Assert.That(deepOtmPrice, Is.EqualTo(0).Within(1).Percent);          
        }

        [Test]
        public void WhenCalculatingGammaForAnOption()
        {
            // stock with 6 months expiration, stock price is 100, strike price is 110, risk free interest rate 0.1 per year, continuous dividend yield 0.06 and volatility is 0.3 

            // Generalised model we use b differently

            // b = r: standard Black Scholes 1973 stock option model
            // b = r - q: gives the Merton 19973 stock option model with contionuous dividend yield q

            var spot = 100;
            var strike = 110;
            var r = 0.1;
            var q = 0.06;
            var b = r - q;
            var maturity = 0.5;
            var vol = 0.3;

            var call = OptionHelper.BlackScholes(OptionType.CALL, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of call is {call}");
            Assert.That(call, Is.EqualTo(5.2515).Within(1).Percent);

            var put = OptionHelper.BlackScholes(OptionType.PUT, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of put is {put}");
            Assert.That(put, Is.EqualTo(12.8422).Within(1).Percent);

            var gamma = OptionHelper.BlackScholes_Gamma(spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Gamma of call/put {gamma}");
            Assert.That(gamma, Is.EqualTo(0.01769).Within(1).Percent);
        }
        
        [Test]
        public void WhenCalculatingThetaForAnOption()
        {
            // stock with 6 months expiration, stock price is 100, strike price is 110, risk free interest rate 0.1 per year, continuous dividend yield 0.06 and volatility is 0.3 

            // Generalised model we use b differently

            // b = r: standard Black Scholes 1973 stock option model
            // b = r - q: gives the Merton 19973 stock option model with contionuous dividend yield q

            var spot = 100;
            var strike = 110;
            var r = 0.1;
            var q = 0.06;
            var b = r - q;
            var maturity = 0.5;
            var vol = 0.3;

            var call = OptionHelper.BlackScholes(OptionType.CALL, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of call is {call}");
            Assert.That(call, Is.EqualTo(5.2515).Within(1).Percent);

            var put = OptionHelper.BlackScholes(OptionType.PUT, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of put is {put}");
            Assert.That(put, Is.EqualTo(12.8422).Within(1).Percent);

            var theta = OptionHelper.BlackScholes_Theta(OptionType.CALL, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Theta for a call is {theta}");
            Assert.That(theta, Is.EqualTo(-8.9962).Within(1).Percent);

            var thetaPut = OptionHelper.BlackScholes_Theta(OptionType.PUT, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Theta for a Put is {thetaPut}");
            Assert.That(thetaPut, Is.EqualTo(-4.3554).Within(1).Percent);

            var z = OptionPlotHelper.PlotGreeks(new DataSeries3D(), GreekTypeEnum.Theta, OptionType.CALL, strike, r, b, vol);
            AssertZValue(z, rounding: 0);
        }

        [Test] 
        public void WhenCalculatingRhoForAnoption()
        {
            // stock with 6 months expiration, stock price is 100, strike price is 110, risk free interest rate 0.1 per year, continuous dividend yield 0.06 and volatility is 0.3 

            // Generalised model we use b differently

            // b = r: standard Black Scholes 1973 stock option model
            // b = r - q: gives the Merton 19973 stock option model with contionuous dividend yield q

            var spot = 100;
            var strike = 110;
            var r = 0.1;
            var q = 0.06;
            var b = r - q;
            var maturity = 0.5;
            var vol = 0.3;

            var call = OptionHelper.BlackScholes(OptionType.CALL, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of call is {call}");
            Assert.That(call, Is.EqualTo(5.2515).Within(1).Percent);

            var put = OptionHelper.BlackScholes(OptionType.PUT, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of put is {put}");
            Assert.That(put, Is.EqualTo(12.8422).Within(1).Percent);

            var rho = OptionHelper.BlackScholes_Rho(OptionType.CALL, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Rho of call is {rho}");
            Assert.That(rho, Is.EqualTo(16.8656).Within(1).Percent);

            var rhoPut = OptionHelper.BlackScholes_Rho(OptionType.PUT, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Rho of put is {rho}");
            Assert.That(rhoPut, Is.EqualTo(-35.4519).Within(1).Percent);
        }

        [Test]
        public void WhenCalculatingVegaForAnOption()
        {
            // stock with 6 months expiration, stock price is 100, strike price is 110, risk free interest rate 0.1 per year, continuous dividend yield 0.06 and volatility is 0.3 

            // Generalised model we use b differently

            // b = r: standard Black Scholes 1973 stock option model
            // b = r - q: gives the Merton 19973 stock option model with contionuous dividend yield q

            var spot = 100;
            var strike = 110;
            var r = 0.1;
            var q = 0.06;
            var b = r - q;
            var maturity = 0.5;
            var vol = 0.3;

            var call = OptionHelper.BlackScholes(OptionType.CALL, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of call is {call}");
            Assert.That(call, Is.EqualTo(5.2515).Within(1).Percent);

            var put = OptionHelper.BlackScholes(OptionType.PUT, spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Price of put is {put}");
            Assert.That(put, Is.EqualTo(12.8422).Within(1).Percent);

            var vega = OptionHelper.BlackScholes_Vega(spot, strike, r, b, maturity, vol);
            Console.WriteLine($"Vega of call/put is {vega}");
            Assert.That(vega, Is.EqualTo(27.5649).Within(1).Percent);
        }

        private static void AssertZValue(double[] z, int rounding)
        {
            var zmin = Math.Round(z[0], rounding);
            var zmax = Math.Round(z[1], rounding);
            var zTick = Math.Round((z[1] - z[0]) / 5.0, rounding);

            Assert.IsTrue(zmin < zmax);
            Assert.IsTrue(zTick > 0);
        }
    }
}
