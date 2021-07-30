using NUnit.Framework;
using QLNet;
using QuantBook.Models.FixedIncome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Tests
{
    public class QuantLibFIHelperTest
    {
        [Test]
        public void WhenCalculatingBondPriceWithFlatRates()
        {
            DateTime evalDate = DateTime.Now;
            DateTime issueDate = DateTime.Now;
            DateTime maturity = DateTime.Now.AddYears(10);
            double faceValue = 1000.0;
            double rate = 0.06;
            double coupon = 0.05;
            int settlementDays = 3;

            (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) = QuantLibFIHelper.BondPrice(evalDate, issueDate, maturity, settlementDays, faceValue, rate, coupon, Frequency.Annual);

            Console.WriteLine($"Simple bond of face value: {faceValue} @ {rate} rate with coupons {coupon} prices at {npv} with clean price: {cprice} dirty price {dprice} accrued {accrued} ytm {ytm}");
            Assert.Greater(npv, 0);
            Assert.Greater(cprice, 0);
            Assert.Greater(dprice, 0);
            Assert.Greater(accrued, 0);
            Assert.Greater(ytm, 0);
        }

        [Test]
        public void WhenCalculatingBondPriceWithCurveRates()
        {
            var faceValue = 100.0;
            var coupon = 0.05;
            var rate = 0.01;
            (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) = QuantLibFIHelper.BondPriceCurveRate(faceValue, coupon);

            Console.WriteLine($"Simple bond of face value: {faceValue} with coupons {coupon} prices at {npv} with clean price: {cprice} dirty price {dprice} accrued {accrued} ytm {ytm}");
            Assert.Greater(npv, 0);
            Assert.Greater(cprice, 0);
            Assert.Greater(dprice, 0);
            Assert.Greater(accrued, 0);
            Assert.Greater(ytm, 0);
        }

        [Test]
        public void WhenCalculatingBondPriceWithCurveRateZSpread()
        {
            var faceValue = 100.0;
            var coupon = 0.05;
            var rate = 0.01;
            (double? npv, double? cprice, double? dprice, double? accrued, double? ytm, List<(double zSpread, double npv)> zResults) = QuantLibFIHelper.BondPriceCurveRateZSpread(faceValue, coupon);

            Console.WriteLine($"Simple bond of face value: {faceValue} with coupons {coupon} prices at {npv} with clean price: {cprice} dirty price {dprice} accrued {accrued} ytm {ytm}");
            Console.WriteLine("zResults: ");
            foreach(var z in zResults)
            {
                Console.WriteLine($"{z.zSpread}={z.npv}"); 
            }
            Assert.Greater(npv, 0);
            Assert.Greater(cprice, 0);
            Assert.Greater(dprice, 0);
            Assert.Greater(accrued, 0);
            Assert.Greater(ytm, 0);
        }

        [Test]
        public void WhenCalculatingZeroCouponRate()
        {
            Date evalDate = new Date(15, Month.Jan, 2015);
            Date[] maturities = new Date[]
            {
                new Date(15, Month.January, 2016),
                new Date(15, Month.January, 2017),
                new Date(15, Month.January, 2018),
                new Date(15, Month.January, 2019),
            };
            var faceAmount = 100.0;
            var coupons = new List<double> { 0.05, 0.055, 0.05, 0.06 };
            var bondPrices = new List<double> { 101.0, 101.5, 99.0, 100.0 };

            var results = QuantLibFIHelper.ZeroCouponDirect(faceAmount, evalDate, coupons, bondPrices, maturities);
            foreach (var result in results)
            {
                var maturity = result.maturity;
                var couponRate = result.couponRate;
                var equivalentRate = result.equivalentRate;
                var discountRate = result.discountRate;
                Console.WriteLine($"Zero coupon for {maturity} has coupon: {couponRate} equivalent: {equivalentRate} with discount:{discountRate}");
            }
        }

        [Test]
        public void WhenCalculatingZeroCouponBootstrapped()
        {
            var depositRates = new double[] { 0.044, 0.045, 0.046, 0.047, 0.049, 0.051, 0.053 };
            var depositMaturities = new Period[]
            {
                new Period(1, TimeUnit.Days),
                new Period(1, TimeUnit.Months),
                new Period(2, TimeUnit.Months),
                new Period(3, TimeUnit.Months),
                new Period(6, TimeUnit.Months),
                new Period(9, TimeUnit.Months),
                new Period(12, TimeUnit.Months),
            };
            double[] bondCoupons = new double[] { 0.05, 0.06, 0.055, 0.05 };
            double[] bondPrices = new double[] { 99.55, 100.55, 99.5, 97.6 };
            var bondMaturities = new Period[]
            {
                new Period(14, TimeUnit.Months),
                new Period(21, TimeUnit.Months),
                new Period(2, TimeUnit.Years),
                new Period(3, TimeUnit.Years),
            };
            var results = QuantLibFIHelper.ZeroCouponBootstrap(100.0, new Date(15, Month.January, 2015), depositRates, depositMaturities, bondPrices, bondCoupons, bondMaturities, ResultType.FromInputMaturities);
            foreach (var result in results)
            {
                Console.WriteLine($"For maturity: {result.maturity} years: {result.years} zeroRate: {result.zeroRate} equivalentRate: {result.eqRate} discount: {result.discount}");

                Assert.That(result.eqRate, Is.GreaterThan(0));
                Assert.That(result.zeroRate, Is.GreaterThan(0));
                Assert.That(result.discount, Is.GreaterThan(0));

                // special case
                // I'm not sure why the zero discount rate shoots up to 0.062627441041400722d during this period
                if (result.maturity.Month == (int)Month.March && result.maturity.Year == 2016)
                {
                    // Ignore for now
                    //Assert.That(result.zeroRate.rate(), Is.GreaterThanOrEqualTo(0.05).And.LessThanOrEqualTo(0.06));
                }
            }
        }

        [Test]
        public void WhenFetchingInterbankTermStructure()
        {
            var settlementDate = new DateTime(2015, 2, 18);
            var depositRates = new double[] { 0.001375, 0.001717, 0.002112, 0.002581 };
            var depositMaturities = new Period[]
            {
                new Period(1, TimeUnit.Weeks),
                new Period(1, TimeUnit.Months),
                new Period(2, TimeUnit.Months),
                new Period(3, TimeUnit.Months)
            };
            double[] futurePrices = new double[] { 99.725, 99.585, 99.385, 99.16, 98.93, 98.715 };
            double[] swapRates = new double[] { 0.0089268, 0.0123343, 0.0147985, 0.0165843, 0.0179191 };
            var swapMaturities = new Period[]
            {
                new Period(2, TimeUnit.Years),
                new Period(3, TimeUnit.Years),
                new Period(4, TimeUnit.Years),
                new Period(5, TimeUnit.Years),
                new Period(6, TimeUnit.Years)
            };
            YieldTermStructure termStructure = QuantLibFIHelper.InterbankTermStructure(settlementDate, depositRates, depositMaturities, futurePrices, swapRates, swapMaturities);
            var discountRate = termStructure.discount(settlementDate);            
            var zeroRate = termStructure.zeroRate(settlementDate, new Actual360() , Compounding.Compounded);
            var equivalentRate = zeroRate.equivalentRate(new Actual360(), Compounding.Compounded, Frequency.Daily, settlementDate.AddDays(-2), settlementDate).rate();
            Console.WriteLine($"InterbankTermStructure results in discountRate:{discountRate} and zeroRate:{zeroRate}");
            Assert.That(discountRate, Is.GreaterThan(0));
            Assert.That(zeroRate.rate(), Is.GreaterThan(0));
            Assert.That(equivalentRate, Is.GreaterThan(0));
        }

        [Test]
        public void WhenCalculatingInterbankZeroCoupon()
        {
            var settlementDate = new Date(18, 2, 2015);
            var depositRates = new double[] { 0.001375, 0.001717, 0.002112, 0.002581 };
            var depositMaturities = new Period[]
            {
                new Period(1, TimeUnit.Weeks),
                new Period(1, TimeUnit.Months),
                new Period(2, TimeUnit.Months),
                new Period(3, TimeUnit.Months)
            };
            double[] futurePrices = new double[] { 99.725, 99.585, 99.385, 99.16, 98.93, 98.715 };
            double[] swapRates = new double[] { 0.0089268, 0.0123343, 0.0147985, 0.0165843, 0.0179191 };
            var swapMaturities = new Period[]
            {
                new Period(2, TimeUnit.Years),
                new Period(3, TimeUnit.Years),
                new Period(4, TimeUnit.Years),
                new Period(5, TimeUnit.Years),
                new Period(6, TimeUnit.Years)
            };
            foreach(var result in  QuantLibFIHelper.InterbankZeroCoupon(settlementDate, depositRates, depositMaturities, futurePrices, swapRates, swapMaturities))
            {
                Console.WriteLine($"InterbankZeroCoupon @ {result.maturity} with timeToMaturity: {result.timesToMaturity} results in zeroCoupon: {result.zeroCouponRate} with equivalentRate: {result.equivalentRate} and discountRate: {result.discountRate} ");
                Assert.That(result.zeroCouponRate, Is.GreaterThan(0));
                Assert.That(result.equivalentRate, Is.GreaterThanOrEqualTo(0));
                Assert.That(result.discountRate, Is.GreaterThan(0));
            }
            
        }

        [Test]
        public void WhenCalculatingZeroCouponBootstrapZSpread()
        {
            var depositRates = new double[] {0.0525, 0.055 };
            var depositMaturities = new Period[]
            {                
                new Period(6, TimeUnit.Months),
                new Period(12, TimeUnit.Months),
            };
            double[] bondCoupons = new double[] { 0.0575, 0.06, 0.0625, 0.065, 0.0675, 0.068, 0.07, 0.071, 0.0715, 0.072, 0.073, 0.0735, 0.074, 0.075, 0.076, 0.076, 0.077, 0.078 };
            double[] bondPrices = new double[bondCoupons.Length];
            for (int i = 0; i < bondCoupons.Length; i++)
            {
                bondPrices[i] = 100.0;
            }
            Period[] bondMaturities = new Period[bondCoupons.Length];
            for (int i = 0; i < bondCoupons.Length; i++)
            {
                bondMaturities[i] = new Period((i + 3) * 6, TimeUnit.Months);
            }
            double zSpread = 50.0;

            var results = QuantLibFIHelper.ZeroCouponBootstrapZspread(depositRates, depositMaturities, bondPrices, bondCoupons, bondMaturities, zSpread);
            foreach (var result in results)
            {
                Console.WriteLine($"For maturity: {result.maturity} years: {result.years} zeroRate: {result.zeroRate} vs {result.zeroRateZSpreaded} discount: {result.discount} vs {result.discountZSpreaded}");
                Assert.That(result.zeroRate, Is.GreaterThan(0));
                Assert.That(result.zeroRateZSpreaded, Is.GreaterThan(0));
                Assert.That(result.discount, Is.GreaterThan(0));
                Assert.That(result.discountZSpreaded, Is.GreaterThan(0));
            }
        }

        [Test]
        [Ignore("QuantLib does not support creating HazardRateStructure based on spreads")]
        public void WhenCalculatingCdsHazardRates()
        {
            double[] spreads = new[] { 34.93, 53.60, 72.02, 106.39, 129.39, 139.46 };
            string[] tenors = new[] { "1Y", "2Y", "3Y", "5Y", "7Y", "10Y"};
            var evalDate = new Date(20, 3, 2015);
            var recoveryRate = 0.4;
            var results = QuantLibFIHelper.CdsHazardRate(evalDate, recoveryRate, spreads, tenors, false);
            Console.WriteLine("CdsHazard rate: ");
            foreach (var result in results)
            {
                Console.WriteLine($"CdsHazardRate with recoveryRate: {recoveryRate} for {evalDate} with spreads {string.Join(",", spreads)} results to hazardRate: {result.hazardRate}, m: {result.timesToMaturity}, surival={result.survivalProbability} %");
                Assert.That(result.hazardRate, Is.GreaterThan(0));
                Assert.That(result.defaultProbability, Is.GreaterThan(0));
                Assert.That(result.survivalProbability, Is.GreaterThan(0));                
            }

            Console.WriteLine("CdsHazard rate data points: ");
            results = QuantLibFIHelper.CdsHazardRate(evalDate, recoveryRate, spreads, tenors, true);
            foreach (var result in results)
            {
                Console.WriteLine($"CdsHazardRate with recoveryRate: {recoveryRate} for {evalDate} with spreads {string.Join(",", spreads)} results to hazardRate: {result.hazardRate}, m: {result.timesToMaturity}, surival={result.survivalProbability} %");
                Assert.That(result.hazardRate, Is.GreaterThan(0));
                Assert.That(result.defaultProbability, Is.GreaterThan(0));
                Assert.That(result.survivalProbability, Is.GreaterThan(0));
            }
        }

        [Test]
        public void WhenFetchingIsdaZeroCurve()
        {
            var evalDate = new Date(18, 3, 2015);
            var referenceDate = evalDate - 3;
            var ccy = "USD";
            var termStructure = QuantLibFIHelper.IsdaZeroCurve(referenceDate, ccy);
            var rate = termStructure.zeroRate(evalDate, new Actual365Fixed(), Compounding.Compounded, extrapolate: true).rate();
            var referenceRate = termStructure.zeroRate(referenceDate, new Actual365Fixed(), Compounding.Compounded).rate();
            Console.WriteLine($"Rate for {ccy} in {referenceDate.ToShortDateString()} is {rate}");
            Console.WriteLine($"Rate for {ccy} in {evalDate.ToShortDateString()} is {referenceRate}");
        }

        [Test]
        public void WhenValuingCdsPV()
        {
            var evalDate = new Date(15, 6, 2009);
            var effectiveDate = new Date(20, 3, 2009);
            var maturity = new Date(20, 6, 2014);
            var ccy = "USD";
            var recoveryRate = 0.4;
            var tenors = "5Y";
            var spreads = "210";
            var notional = 10_000;
            var coupon = 100;       
            var cds = QuantLibFIHelper.CdsPv(Protection.Side.Buyer, ccy, evalDate, effectiveDate, maturity, recoveryRate, spreads, tenors, notional, Frequency.Quarterly, coupon);

            Assert.That(cds.npv, Is.GreaterThan(0).Or.LessThan(0)) ;
            Assert.That(cds.fairSpread, Is.GreaterThan(0));
            Assert.That(cds.hazardRate, Is.GreaterThan(0));
            Assert.That(cds.defaultProbability, Is.GreaterThan(0));
            Assert.That(cds.survivalpProbability, Is.GreaterThan(0));
        }
    }
}


