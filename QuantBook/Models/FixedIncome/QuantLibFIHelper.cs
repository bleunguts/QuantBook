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
