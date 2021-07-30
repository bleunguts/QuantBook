using QLNet;
using QuantBook.Models.Isda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.FixedIncome
{
    public enum ResultType { FromInputMaturities = 0, MonthlyResults = 1 }

    public static class QuantLibFIHelper
    {
        public static (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) BondPrice(DateTime evalDate,
                                                                                                            DateTime issueDate,
                                                                                                            DateTime maturity,
                                                                                                            int settlementDays,
                                                                                                            double faceValue,
                                                                                                            double rate,
                                                                                                            double coupon,
                                                                                                            Frequency frequency)
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

        public static List<(DateTime maturity, double couponRate, double equivalentRate, double discountRate)> ZeroCouponDirect(double faceAmount,
                                                                                                                                Date evalDate,
                                                                                                                                List<double> coupons,
                                                                                                                                List<double> bondPrices,
                                                                                                                                Date[] maturities)
        {
            DayCounter dc = new ActualActual(ActualActual.Convention.Bond);
            Calendar calender = new UnitedKingdom();            

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

        public static List<(Date maturity, double years, double zeroRate, double discount, double eqRate)> ZeroCouponBootstrap(double faceAmount,
                                                                                                                                     Date evalDate,
                                                                                                                                     double[] depositRates,
                                                                                                                                     Period[] depositMaturities,
                                                                                                                                     double[] bondPrices,
                                                                                                                                     double[] bondCoupons,
                                                                                                                                     Period[] bondMaturities,
                                                                                                                                     ResultType resultType = ResultType.MonthlyResults)
        {           
            DayCounter dc = new ActualActual(ActualActual.Convention.Bond);
            Settings.setEvaluationDate(evalDate);
            Calendar calendar = new UnitedStates();

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
          
            var termStructure = new PiecewiseYieldCurve<Discount, Linear>(evalDate, instruments, dc);
            var result = new List<(Date maturity, double years, double zeroRate, double discount, double eqRate)>();
            switch (resultType)
            {
                case ResultType.MonthlyResults:
                    {
                        Date d = evalDate + depositMaturities[0];
                        Date lastDate = evalDate + (new Period(3, TimeUnit.Years));
                        while (d < lastDate)
                        {
                            var years = dc.yearFraction(evalDate, d);
                            var zeroRate = termStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                            var discount = termStructure.discount(d);
                            var eqRate = zeroRate.equivalentRate(dc, Compounding.Compounded, Frequency.Daily, evalDate, d).rate();
                            result.Add((d, years, zeroRate.rate(), discount, eqRate));
                            d = d + new Period(1, TimeUnit.Months);
                        }
                    }
                    break;
                case ResultType.FromInputMaturities:
                    foreach(Date d in termStructure.dates().Where(d => d > evalDate))
                    {                        
                        var years = dc.yearFraction(evalDate, d);
                        var zeroRate = termStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                        var discount = termStructure.discount(d);
                        var eqRate = zeroRate.equivalentRate(dc, Compounding.Compounded, Frequency.Daily, evalDate, d).rate();
                        result.Add((d, years, zeroRate.rate(), discount, eqRate));
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unsupported Result type {resultType}");
            }            
            return result;
        }

        public static List<(Date maturity, double years, double zeroRate, double zeroRateZSpreaded, double discount, double discountZSpreaded)> ZeroCouponBootstrapZspread(double[] depositRates,
                                                                                                                                            Period[] depositMaturities,
                                                                                                                                            double[] bondPrices,
                                                                                                                                            double[] bondCoupons,
                                                                                                                                            Period[] bondMaturities,
                                                                                                                                            double zSpread,
                                                                                                                                            ResultType resultType = ResultType.MonthlyResults)
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

            var termStructure = new PiecewiseYieldCurve<Discount, Linear>(evalDate, instruments, dc);
            var zSpreadHandle = new Handle<Quote>(new SimpleQuote(zSpread / 10_000.0));
            ZeroSpreadedTermStructure zs = new ZeroSpreadedTermStructure(new Handle<YieldTermStructure>(termStructure), zSpreadHandle);            
       
            var result = new List<(Date maturity, double years, double zeroRate, double zeroRateZSpreaded, double discount, double discountZSpreaded)>();
            switch (resultType)
            {
                case ResultType.MonthlyResults:
                    {
                        Date d = evalDate + depositMaturities[0];
                        Date lastDate = evalDate + (new Period(3, TimeUnit.Years));
                        while (d < lastDate)
                        {
                            var years = dc.yearFraction(evalDate, d);
                            var zeroRate = termStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                            var zeroRateZSpreaded = zs.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                            var discount = termStructure.discount(d);
                            var discountZSpreaded = zs.discount(d);
                            result.Add((d, years, zeroRate.rate(), zeroRateZSpreaded.rate(), discount, discountZSpreaded));
                            d = d + new Period(1, TimeUnit.Months);
                        }
                    }
                    break;
                case ResultType.FromInputMaturities:
                    foreach (Date d in termStructure.dates().Where(d => d > evalDate))
                    {
                        var years = dc.yearFraction(evalDate, d);
                        var zeroRate = termStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                        var zeroRateZSpreaded = zs.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                        var discount = termStructure.discount(d);
                        var discountZSpreaded = zs.discount(d);
                        result.Add((d, years, zeroRate.rate(), zeroRateZSpreaded.rate(), discount, discountZSpreaded));
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unsupported Result type {resultType}");
            }
            return result;
        }

        public static (double? npv, double? cprice, double? dprice, double? accrued, double? ytm, List<(double zSpread, double npv)> zResults) BondPriceCurveRateZSpread(double faceValue,
                                                                                                                                                                         double coupon)
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

        public static (double npv, double hazardRate, double fairSpread, double defaultProbability, double survivalpProbability) CdsPv(Protection.Side side,
                                                                                                                                       string ccy,
                                                                                                                                       Date evalDate,
                                                                                                                                       Date effectiveDate,
                                                                                                                                       Date maturity,
                                                                                                                                       double recoveryRate,
                                                                                                                                       string spreads,
                                                                                                                                       string tenors,
                                                                                                                                       double hazardRateForProbCurve = 1.0E-12,
                                                                                                                                       double notional,
                                                                                                                                       Frequency couponFrequency,
                                                                                                                                       double coupon)
        {
            Calendar calendar = new TARGET();
            evalDate = calendar.adjust(evalDate);
            Date settlementDate = calendar.advance(evalDate, new Period(2, TimeUnit.Days));

            var termStructureCurve = new Handle<YieldTermStructure>(IsdaZeroCurve(evalDate, ccy));

            // construct proability curve based on hazardRate
            Handle<Quote> hazardRate = new Handle<Quote>(new SimpleQuote(1.0E-12));
            RelinkableHandle<DefaultProbabilityTermStructure> probabilityCurve =
               new RelinkableHandle<DefaultProbabilityTermStructure>();
            probabilityCurve.linkTo(new FlatHazardRate(0, calendar, hazardRate, new Actual360()));            

            var schedule = new Schedule(effectiveDate, settlementDate, new Period(couponFrequency), calendar, BusinessDayConvention.Following, BusinessDayConvention.Following, DateGeneration.Rule.TwentiethIMM, false);
            var creditDefaultSwap = new CreditDefaultSwap(side, notional, coupon / 10_000, schedule, BusinessDayConvention.ModifiedFollowing, new ActualActual(), false);
            var engine = new MidPointCdsEngine(probabilityCurve, recoveryRate, termStructureCurve);
            creditDefaultSwap.setPricingEngine(engine);

            var npv = creditDefaultSwap.NPV();
            var hazardRate_ = creditDefaultSwap.impliedHazardRate(npv, termStructureCurve, new ActualActual());
            var defaultProbability = probabilityCurve.link.defaultProbability(evalDate, maturity);
            var survivalProbability = probabilityCurve.link.survivalProbability(maturity);
            return (npv, hazardRate_, creditDefaultSwap.fairSpread(), defaultProbability, survivalProbability);
        }

        public static (double accrual, double upfront, double cleanPrice, double dirtyPrice, double dv01) CdsPrice(Protection.Side side,
                                                                                                                   string ccy,
                                                                                                                   Date evalDate,
                                                                                                                   Date effectiveDate,
                                                                                                                   Date maturity,
                                                                                                                   double recoveryRate,
                                                                                                                   string spreads,
                                                                                                                   string tenors,
                                                                                                                   Frequency couponFrequency,
                                                                                                                   double coupon)
        {
            double notional = 100.0;
            int numDays = Utilities.get_number_calendar_days(effectiveDate, evalDate) + 1;

            var cds = CdsPv(side, ccy, evalDate, effectiveDate, maturity, recoveryRate, spreads, tenors, notional, couponFrequency, coupon);

            double accrual = coupon * numDays / 360.0 / 100.0;
            double upfront = cds.npv;
            double? dirtyPrice = null;
            double? cleanPrice = null;
            switch(side)
            {
                case Protection.Side.Buyer:
                    accrual = -accrual;
                    dirtyPrice = 100 - upfront;
                    cleanPrice = dirtyPrice + accrual;

                    break;
                case Protection.Side.Seller:
                    dirtyPrice = 100 + upfront;
                    cleanPrice = dirtyPrice - accrual;

                    break;
            }

            double parSpread = cds.fairSpread;
            double coupon1 = parSpread + 1.0;
            var cds2 = CdsPv(side, ccy, evalDate, effectiveDate, maturity, recoveryRate, spreads, tenors, notional, couponFrequency, coupon1);
            double dv01 = cds2.npv;

            return (accrual, upfront, cleanPrice.Value, dirtyPrice.Value, dv01);
        }

        public static (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) BondPriceCurveRate(double faceValue = 100,
                                                                                                                     double coupon = 0.05)
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

        public static PiecewiseYieldCurve<Discount, Cubic> InterbankTermStructure(DateTime settlementDate,
                                                                double[] depositRates,
                                                                Period[] depositMaturities,
                                                                double[] futurePrices,
                                                                double[] swapRates,
                                                                Period[] swapMaturities)
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

     

        public static List<(Date maturity, double timesToMaturity, double zeroCouponRate, double equivalentRate, double discountRate)> InterbankZeroCoupon(Date settlementDate,
                                                                                                                                                                          double[] depositRates,
                                                                                                                                                                          Period[] depositMaturities,
                                                                                                                                                                          double[] futurePrices,
                                                                                                                                                                          double[] swapRates,
                                                                                                                                                                          Period[] swapMaturities)
        {            
            DayCounter dc = new Actual360();
            var interbankTermStructure = InterbankTermStructure(settlementDate, depositRates, depositMaturities, futurePrices, swapRates, swapMaturities);

            var results = new List<(Date maturity, double timesToMaturity, double zeroCouponRate, double equivalentRate, double discountRate)>();
            var upperDate = settlementDate + swapMaturities.Last();
            foreach (var referenceDate in interbankTermStructure.dates().Where(d => d > settlementDate && d <= upperDate))
            {
                var years = dc.yearFraction(settlementDate, referenceDate);
                var zeroRate = interbankTermStructure.zeroRate(years, Compounding.Compounded, Frequency.Annual);
                var discount = interbankTermStructure.discount(referenceDate);
                var eqRate = zeroRate.equivalentRate(dc, Compounding.Compounded, Frequency.Daily, settlementDate, referenceDate).rate();
                results.Add((referenceDate, years, zeroRate.rate(), eqRate, discount));
            }

            return results;
        }

        public static List<(Date evalDate, double timesToMaturity, double hazardRate, double survivalProbability, double defaultProbability)> CdsHazardRate(Date evalDate,
                                                                                                                                                            double recoveryRate,
                                                                                                                                                            double[] spreads,
                                                                                                                                                            string[] tenors,
                                                                                                                                                            bool isDataPoints)
        {
            DayCounter dayCounter = new Actual365Fixed();
            Period[] periods = tenors.ToPeriods();
            Calendar calendar = new TARGET();
            evalDate = calendar.adjust(evalDate);
            Settings.setEvaluationDate(evalDate);

            /*
            List<double> defaultProbabilities = new List<double>();
            defaultProbabilities.Add(0.0000);
            defaultProbabilities.Add(0.0047);
            defaultProbabilities.Add(0.0093);
            defaultProbabilities.Add(0.0286);
            defaultProbabilities.Add(0.0619);
            defaultProbabilities.Add(0.0953);
            defaultProbabilities.Add(0.1508);
            defaultProbabilities.Add(0.2288);
            defaultProbabilities.Add(0.3666);

            List<double> hazardRates = new List<double>();
            hazardRates.Add(0.0);
            for (int i = 1; i < dates.Count; ++i)
            {
                double t1 = dayCounter.yearFraction(dates[0], dates[i - 1]);
                double t2 = dayCounter.yearFraction(dates[0], dates[i]);
                double S1 = 1.0 - defaultProbabilities[i - 1];
                double S2 = 1.0 - defaultProbabilities[i];
                hazardRates.Add(Math.Log(S1 / S2) / (t2 - t1));
            }

            // bootstrap hazard rates            
            RelinkableHandle<DefaultProbabilityTermStructure> piecewiseFlatHazardRate = new RelinkableHandle<DefaultProbabilityTermStructure>();
            piecewiseFlatHazardRate.linkTo(new InterpolatedHazardRateCurve<BackwardFlat>(dates, hazardRates, new Thirty360()));
            */
            throw new NotImplementedException("Quant Library does not contain a piecewiseFlatHazardRate that is generated by set of spreads");
        }

        public static YieldTermStructure IsdaZeroCurve(DateTime evalDate, string ccy)
        {
            DateTime rateDate = Utilities.get_previous_workday(evalDate);
            Settings.setEvaluationDate(evalDate);
            Calendar calendar = new TARGET();

            var isdaRates = IsdaHelper.GetIsdaRates(ccy, rateDate, rateDate);
            var instruments = new List<RateHelper>();

            foreach(var isdaRate in isdaRates)
            {
                int fixedDays = Utilities.get_number_calendar_days(isdaRate.SnapTime, isdaRate.SpotDate.To<DateTime>());
                if (IsDepositRate(isdaRate))
                {
                    var period = ToPeriod(isdaRate.Tenor);
                    var rate = Convert.ToDouble(isdaRate.Rate);
                    instruments.Add(new DepositRateHelper(rate, period, fixedDays, calendar, BusinessDayConvention.Unadjusted, false, new Actual360()));
                }
                else // swaps
                {                    
                    IborIndex swFloatingLegIndex = new USDLibor(new Period(3, TimeUnit.Months));
                    var period = ToPeriod(isdaRate.Tenor);
                    var rate = Convert.ToDouble(isdaRate.Rate);
                    instruments.Add(new SwapRateHelper(rate, period, calendar, Frequency.Semiannual, BusinessDayConvention.Unadjusted, new Thirty360(), swFloatingLegIndex));
                }
            }           

            return new PiecewiseYieldCurve<Discount, LogCubic>(rateDate, instruments, new Actual365Fixed());

            bool IsDepositRate(IsdaRate r) => string.IsNullOrEmpty(r.FixedDayCountConvention);

            Period ToPeriod(string t) => 
                t.Contains("M") ? new Period(Convert.ToInt32(t.Split('M').First()), TimeUnit.Months) :
                t.Contains("Y") ? new Period(Convert.ToInt32(t.Split('Y').First()), TimeUnit.Years) : throw new NotSupportedException($"tenor format is invalid {t}");
                    
        }
    }
}
