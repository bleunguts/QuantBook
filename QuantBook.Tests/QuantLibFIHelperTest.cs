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
        public void WhenCalculatingZeroCouponRate()
        {
            var results = QuantLibFIHelper.ZeroCouponDirect();
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
        public void WhenFetchingInterbankZeroCoupon()
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
            (DateTime referenceDate, double timesToMaturity, InterestRate zeroCouponRate, InterestRate equivalentRate, double discountRate) = QuantLibFIHelper.InterbankZeroCoupon(settlementDate, depositRates, depositMaturities, futurePrices, swapRates, swapMaturities);
            Console.WriteLine($"InterbankZeroCoupon @ {referenceDate} with timeToMaturity: {timesToMaturity} results in zeroCoupon: {zeroCouponRate.rate()} with equivalentRate: {equivalentRate.rate()} and discountRate: {discountRate} ");
            Assert.That(zeroCouponRate.rate(), Is.GreaterThan(0));
            Assert.That(equivalentRate.rate(), Is.GreaterThanOrEqualTo(0));
            Assert.That(discountRate, Is.GreaterThan(0));
        }
    }
}
