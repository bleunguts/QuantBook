using QLNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.FixedIncome
{
    public static class QuantLibFIHelper
    {
        public static (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) BondPrice(DateTime evalDate, DateTime issueDate, DateTime maturity, int settlementDays, double faceValue, double rate, double coupon, Frequency frequency)
        {
            DayCounter bondDayCount = new ActualActual(ActualActual.Convention.Bond);
            Calendar calendar = new UnitedKingdom(UnitedKingdom.Market.Exchange);

            Settings.setEvaluationDate(evalDate);
            Date settlementDate = evalDate.AddDays(settlementDays);
            settlementDate = calendar.adjust(settlementDate);
            
            FlatForward flatCurve = new FlatForward(settlementDate, new SimpleQuote(rate), bondDayCount, Compounding.Compounded, frequency);
            var discountingTermStructure = new Handle<YieldTermStructure>(flatCurve);

            Schedule schedule = new Schedule(issueDate, maturity, new Period(frequency), calendar,
                                        BusinessDayConvention.Unadjusted, BusinessDayConvention.Unadjusted, DateGeneration.Rule.Backward, false);

            FixedRateBond bond = new FixedRateBond(settlementDays, faceValue, schedule, new List<double>() { coupon }, bondDayCount);

            IPricingEngine bondEngine = new DiscountingBondEngine(discountingTermStructure);
            bond.setPricingEngine(bondEngine);

            var npv = Utilities.SafeExec(() => bond.NPV());
            var cprice = Utilities.SafeExec(() => bond.cleanPrice());
            var dprice = Utilities.SafeExec(() => bond.dirtyPrice());
            var accrued = Utilities.SafeExec(() => bond.accruedAmount());
            var ytm = Utilities.SafeExec(() => bond.yield(bondDayCount, Compounding.Continuous, frequency));
            return (npv, cprice, dprice, accrued, ytm);
        }

        public static List<(DateTime maturity, double couponRate, double equivalentRate, double discountRate)> ZeroCouponDirect()
        {
            DayCounter dc = new ActualActual(ActualActual.Convention.Bond);
            Calendar calender = new UnitedKingdom();

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

            Settings.setEvaluationDate(evalDate);
            var instruments = new List<RateHelper>();
            for (int i = 0; i < maturities.Length; i++)
            {
                var schedule = new Schedule(evalDate, maturities[i], new Period(Frequency.Annual), calender, BusinessDayConvention.Unadjusted, BusinessDayConvention.Unadjusted, DateGeneration.Rule.Backward, true);
                instruments.Add(new FixedRateBondHelper(new Handle<Quote>(new SimpleQuote(bondPrices[i])), 0, faceAmount, schedule, coupons, dc, BusinessDayConvention.Unadjusted, 100.0, evalDate));
            }
            var discountTermStructure = new PiecewiseYieldCurve<Discount, Linear>(evalDate, instruments, dc);

            var results = new List<(DateTime maturity, double couponRate, double equivalentRate, double discountRate)>();
            foreach(var maturityDate in discountTermStructure.dates())
            {
                var years = dc.yearFraction(evalDate, maturityDate);
                var zeroRate = discountTermStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                var discount = discountTermStructure.discount(maturityDate);
                var equivalentRate = zeroRate.equivalentRate(dc, Compounding.Compounded, Frequency.Daily, evalDate, maturityDate).rate();
                results.Add((maturityDate, zeroRate.rate(), equivalentRate, discount));

            }
            return results;
        }

        public static (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) BondPriceCurveRate()
        {
            DayCounter bondDayCount = new ActualActual(ActualActual.Convention.Bond);
            Frequency frequency = Frequency.Annual;
            const int settlementDays = 1;
            double faceValue = 100;
            double coupon = 0.05;
            List<double> rates = new List<double>() { 0, 0.004, 0.006, 0.0065, 0.007 };
            List<Date> rateDates = new List<Date>()
            {
                new Date(15, Month.January, 2015),
                new Date(15, Month.July, 2015),
                new Date(15, Month.January, 2016),
                new Date(15, Month.July, 2016),
                new Date(15, Month.January, 2017)
            };
            Date evalDate = new Date(15, Month.January, 2015);
            Settings.setEvaluationDate(evalDate);
            Date issueDate = new Date(15, Month.January, 2015);
            Date maturity = new Date(15, Month.January, 2017);
            Date settlementDate = evalDate + settlementDays;            
            Calendar calendar = new UnitedKingdom(UnitedKingdom.Market.Exchange);
            settlementDate = calendar.adjust(settlementDate);

            Schedule schedule = new Schedule(issueDate, maturity, new Period(Frequency.Semiannual), calendar, BusinessDayConvention.Unadjusted, BusinessDayConvention.Unadjusted,
                DateGeneration.Rule.Backward, false);
            FixedRateBond bond = new FixedRateBond(settlementDays, faceValue, schedule, new List<double>() { coupon }, bondDayCount);
            var rateCurve = new InterpolatedZeroCurve<Linear>(rateDates, rates, bondDayCount, calendar, new Linear(), Compounding.Compounded, frequency);
            var discountingTermStructure = new Handle<YieldTermStructure>(rateCurve);
            IPricingEngine bondEngine = new DiscountingBondEngine(discountingTermStructure);
            bond.setPricingEngine(bondEngine);

            var npv = Utilities.SafeExec(() => bond.NPV());
            var cprice = Utilities.SafeExec(() => bond.cleanPrice());
            var dprice = Utilities.SafeExec(() => bond.dirtyPrice());
            var accrued = Utilities.SafeExec(() => bond.accruedAmount());
            var ytm = Utilities.SafeExec(() => bond.yield(bondDayCount, Compounding.Continuous, frequency));
            return (npv, cprice, dprice, accrued, ytm);
        }
    }
}
