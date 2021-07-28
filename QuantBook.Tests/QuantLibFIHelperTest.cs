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
    }
}
