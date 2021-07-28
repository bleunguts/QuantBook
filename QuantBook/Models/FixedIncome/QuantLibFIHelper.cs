﻿using QLNet;
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

        public static (double? npv, double? cprice, double? dprice, double? accrued, double? ytm, List<(double zSpread, double npv)> zResults) BondPriceCurveRateZSpread(double faceValue, double coupon)
        {
            DayCounter bondDayCount = new ActualActual(ActualActual.Convention.Bond);
            Frequency frequency = Frequency.Annual;
            const int settlementDays = 1;

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

            double[] zSpreads = new double[21];
            for (int i = 0; i < zSpreads.Length; i++)
            {
                zSpreads[i] = 250.0 * i;
            }

            List<(double zSpread, double npv)> zSpreadResults = new List<(double zSpread, double npv)>();
            var leg = bond.cashflows();
            for (int i = 0; i < zSpreads.Length; i++)
            {
                var znpv = CashFlows.npv(leg, rateCurve, zSpreads[i] / 10_000.0, bondDayCount, Compounding.Compounded, Frequency.Semiannual, true, settlementDate, evalDate);
                zSpreadResults.Add((zSpreads[i], znpv));
            }

            var npv = Utilities.SafeExec(() => bond.NPV());
            var cprice = Utilities.SafeExec(() => bond.cleanPrice());
            var dprice = Utilities.SafeExec(() => bond.dirtyPrice());
            var accrued = Utilities.SafeExec(() => bond.accruedAmount());
            var ytm = Utilities.SafeExec(() => bond.yield(bondDayCount, Compounding.Continuous, frequency));
            return (npv, cprice, dprice, accrued, ytm, zSpreadResults);
        }

        public static (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) BondPriceCurveRate(double faceValue = 100, double coupon = 0.05)
        {
            DayCounter bondDayCount = new ActualActual(ActualActual.Convention.Bond);
            Frequency frequency = Frequency.Annual;
            const int settlementDays = 1;
           
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

        public static YieldTermStructure InterbankTermStructure(DateTime settlementDate, double[] depositRates, Period[] depositMaturities, double[] futurePrices, double[] swapRates, Period[] swapMaturities)
        {
            DayCounter dc = new Actual360();
            const int fixingsDays = 2;

            Calendar calendar = new JointCalendar(new UnitedKingdom(UnitedKingdom.Market.Exchange), new UnitedStates(UnitedStates.Market.Settlement));
            var evalDate = calendar.advance(settlementDate, -fixingsDays, TimeUnit.Days);
            Settings.setEvaluationDate(evalDate);

            List<RateHelper> instruments = new List<RateHelper>();
            // Money market deposits
            for (int i = 0; i < depositRates.Length; i++)
            {
                instruments.Add(new DepositRateHelper(depositRates[i], depositMaturities[i], fixingsDays, calendar, BusinessDayConvention.ModifiedFollowing, true, dc));
            }

            // futures contracts
            Date imm = IMM.nextDate(settlementDate);
            for (int i = 0; i < futurePrices.Length; i++)
            {
                instruments.Add(new FuturesRateHelper(futurePrices[i], imm, 3, calendar, BusinessDayConvention.ModifiedFollowing, true, dc));
                imm = IMM.nextDate(imm + 1);
            }

            // swap rates
            var floatingLegIndex = new USDLibor(new Period(3, TimeUnit.Months));
            for (int i = 0; i < swapRates.Length; i++)
            {
                instruments.Add(new SwapRateHelper(swapRates[i], swapMaturities[i], calendar, Frequency.Annual, BusinessDayConvention.Unadjusted, dc, floatingLegIndex)); 
            }

            return new PiecewiseYieldCurve<Discount, Cubic>(settlementDate, instruments, dc);
        }

     

        public static (DateTime referenceDate, double timesToMaturity, InterestRate zeroCouponRate, InterestRate equivalentRate, double discountRate) InterbankZeroCoupon(DateTime settlementDate, double[] depositRates, Period[] depositMaturities, double[] futurePrices, double[] swapRates, Period[] swapMaturities)
        {
            DayCounter dc = new Actual360();
            var interbankTermStructure = InterbankTermStructure(settlementDate, depositRates, depositMaturities, futurePrices, swapRates, swapMaturities);

            var referenceDate = interbankTermStructure.referenceDate();
            var years = dc.yearFraction(settlementDate, referenceDate);
            var zeroRate = interbankTermStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
            var discount = interbankTermStructure.discount(referenceDate);
            var eqRate = zeroRate.equivalentRate(dc, Compounding.Compounded, Frequency.Daily, settlementDate.AddDays(-2), settlementDate);
            return (referenceDate, years, zeroRate, eqRate, discount);
        }

        public static List<(Date maturity, double years, InterestRate zeroRate, double discount, double eqRate)> ZeroCouponBootstrap(double[] depositRates, Period[] depositMaturities, double[] bondPrices, double[] bondCoupons, Period[] bondMaturities)
        {
            var faceAmount = 100.0;
            DayCounter dc = new ActualActual(ActualActual.Convention.Bond);
            var evalDate = new Date(15, Month.January, 2015);
            Settings.setEvaluationDate(evalDate);
            Calendar calendar = new UnitedKingdom();

            // deposits
            var instruments = new List<RateHelper>();
            for (int i = 0; i < depositMaturities.Length; i++)
            {
                instruments.Add(new DepositRateHelper(depositRates[i], depositMaturities[i], 0, calendar, BusinessDayConvention.Unadjusted, true, dc));
            }

            // bonds
            for (int i = 0; i < bondMaturities.Length; i++)
            {
                var quote = new Handle<Quote>(new SimpleQuote(bondPrices[i]));
                Date maturity = evalDate + bondMaturities[i];
                var schedule = new Schedule(evalDate, maturity, new Period(Frequency.Annual), calendar, BusinessDayConvention.Unadjusted, BusinessDayConvention.Unadjusted, DateGeneration.Rule.Backward, true);
                instruments.Add(new FixedRateBondHelper(quote, 0, faceAmount, schedule, new List<double>(bondCoupons), dc));
            }

            var result = new List<(Date maturity, double years, InterestRate zeroRate, double discount, double eqRate)>();
            var termStructure = new PiecewiseYieldCurve<Discount, Linear>(evalDate, instruments, dc);
            Date d = evalDate + depositMaturities[0];
            Date lastDate = evalDate + (new Period(3, TimeUnit.Years));
            while (d < lastDate)
            {
                var years = dc.yearFraction(evalDate, d);
                var zeroRate = termStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                var discount = termStructure.discount(d);
                var eqRate = zeroRate.equivalentRate(dc, Compounding.Compounded, Frequency.Daily, evalDate, d).rate();
                result.Add((d, years, zeroRate, discount, eqRate));
                d = d + new Period(1, TimeUnit.Months);
            }
            return result;
        }

        public static List<(Date maturity, double years, InterestRate zeroRate, double discount, double eqRate)> ZeroCouponBootstrapZspread(double[] depositRates, Period[] depositMaturities, double[] bondPrices, double[] bondCoupons, Period[] bondMaturities, double zSpread)
        {
            var faceAmount = 100.0;
            DayCounter dc = new ActualActual(ActualActual.Convention.Bond);
            var evalDate = new Date(15, Month.January, 2015);
            Settings.setEvaluationDate(evalDate);
            Calendar calendar = new UnitedKingdom();

            // deposits
            var instruments = new List<RateHelper>();
            for (int i = 0; i < depositMaturities.Length; i++)
            {
                instruments.Add(new DepositRateHelper(depositRates[i], depositMaturities[i], 0, calendar, BusinessDayConvention.Unadjusted, true, dc));
            }

            // bonds
            for (int i = 0; i < bondMaturities.Length; i++)
            {
                var quote = new Handle<Quote>(new SimpleQuote(bondPrices[i]));
                Date maturity = evalDate + bondMaturities[i];
                var schedule = new Schedule(evalDate, maturity, new Period(Frequency.Annual), calendar, BusinessDayConvention.Unadjusted, BusinessDayConvention.Unadjusted, DateGeneration.Rule.Backward, true);
                instruments.Add(new FixedRateBondHelper(quote, 0, faceAmount, schedule, new List<double>(bondCoupons), dc));
            }

            var result = new List<(Date maturity, double years, InterestRate zeroRate, double discount, double eqRate)>();
            var termStructure = new PiecewiseYieldCurve<Discount, Linear>(evalDate, instruments, dc);
            var zSpreadHandle = new Handle<Quote>(new SimpleQuote(zSpread / 10_000.0));
            ZeroSpreadedTermStructure zs = new ZeroSpreadedTermStructure(new Handle<YieldTermStructure>(termStructure), zSpreadHandle);

            Date d = evalDate + depositMaturities[0];
            Date lastDate = evalDate + (new Period(3, TimeUnit.Years));
            while (d < lastDate)
            {
                var years = dc.yearFraction(evalDate, d);
                var zeroRate = zs.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                var discount = zs.discount(d);
                var eqRate = zeroRate.equivalentRate(dc, Compounding.Compounded, Frequency.Daily, evalDate, d).rate();
                result.Add((d, years, zeroRate, discount, eqRate));
                d = d + new Period(1, TimeUnit.Months);
            }
            return result;
        }
    }
}
