using QLNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.Options
{
    public enum EuropeanEngineType
    {
        Analytic,
        JR, CRR, EQP, TGEO, TIAN, LR, JOSHI,
        FiniteDifferences,
        Integral,
        PseudoMonteCarlo, QuasiMonteCarlo
    }

    public enum AmericanEngineType
    {
        Barone_Adesi_Whaley,
        Bjerksund_Stensland,
        //FiniteDifference,
        //Binomial_Jarrow_Rudd,
        Binomial_Cox_Ross_Rubinstein
    }

    public class QuantLibHelper
    {
        private static BlackScholesMertonProcess BlackScholesMertonProcess(Quote u, YieldTermStructure q, YieldTermStructure r, BlackVolTermStructure vol) => new BlackScholesMertonProcess(
                    new Handle<Quote>(u),
                    new Handle<YieldTermStructure>(q),
                    new Handle<YieldTermStructure>(r),
                    new Handle<BlackVolTermStructure>(vol));

        private static VanillaOption MakeOption(
            StrikedTypePayoff payoff,
            Exercise exercise,
            Quote u,
            YieldTermStructure q,
            YieldTermStructure r,
            BlackVolTermStructure vol,
            EuropeanEngineType engineType,
            int binomialSteps = 100,
            int samples = 1)
        {
            GeneralizedBlackScholesProcess stochProcess = BlackScholesMertonProcess(u, q, r, vol);

            IPricingEngine engine;
            switch (engineType)
            {
                case EuropeanEngineType.Analytic:
                    engine = new AnalyticEuropeanEngine(stochProcess);
                    break;
                case EuropeanEngineType.JR:
                    engine = new BinomialVanillaEngine<JarrowRudd>(stochProcess, binomialSteps);
                    break;
                case EuropeanEngineType.CRR:
                    engine = new BinomialVanillaEngine<CoxRossRubinstein>(stochProcess, binomialSteps);
                    break;
                case EuropeanEngineType.EQP:
                    engine = new BinomialVanillaEngine<AdditiveEQPBinomialTree>(stochProcess, binomialSteps);
                    break;
                case EuropeanEngineType.TGEO:
                    engine = new BinomialVanillaEngine<Trigeorgis>(stochProcess, binomialSteps);
                    break;
                case EuropeanEngineType.TIAN:
                    engine = new BinomialVanillaEngine<Tian>(stochProcess, binomialSteps);
                    break;
                case EuropeanEngineType.LR:
                    engine = new BinomialVanillaEngine<LeisenReimer>(stochProcess, binomialSteps);
                    break;
                case EuropeanEngineType.JOSHI:
                    engine = new BinomialVanillaEngine<Joshi4>(stochProcess, binomialSteps);
                    break;
                case EuropeanEngineType.FiniteDifferences:
                    engine = new FDEuropeanEngine(stochProcess, binomialSteps, samples);
                    break;
                case EuropeanEngineType.Integral:
                    engine = new IntegralEngine(stochProcess);
                    break;
                default:
                    throw new ArgumentException("unknown engine type");
            }

            VanillaOption option = new EuropeanOption(payoff, exercise);
            option.setPricingEngine(engine);
            return option;
        }
        public static (double, double, double, double, double, double) EuropeanOption(OptionType optionType, DateTime evalDate, double yearsToMaturity, double strike, double spot, double q, double r, double vol, EuropeanEngineType priceEngineType)
        {
            DayCounter dc = new Actual360();

            Date today = Date.Today;
            Date exDate = today + (int)(yearsToMaturity * 360);
            Exercise exercise = new EuropeanExercise(exDate);
            StrikedTypePayoff payoff = new PlainVanillaPayoff(optionType.ToOptionType(), strike);

            BlackVolTermStructure volTS = new BlackConstantVol(today, new NullCalendar(), vol, dc); ;
            SimpleQuote qRate = new SimpleQuote(q);
            YieldTermStructure qTS = new FlatForward(today, qRate, dc);
            SimpleQuote rRate = new SimpleQuote(r);
            YieldTermStructure rTS = new FlatForward(today, rRate, dc);
            SimpleQuote u = new SimpleQuote(spot);

            VanillaOption option = MakeOption(payoff, exercise, u, qTS, rTS, volTS, priceEngineType);
            return (option.NPV(), option.delta(), option.gamma(), option.theta(), option.rho(), option.vega());
        }

        public static double EuropeanOptionImpliedVol(OptionType optionType, DateTime evalDate, double yearsToMaturity, double strike, double spot, double q, double r, double targetPrice, double vol = 0.3, double minVol = 0.1, double maxVol = 4.0)
        {
            DayCounter dc = new Actual360();

            Date today = Date.Today;
            Date exDate = today + (int)(yearsToMaturity * 360);
            Exercise exercise = new EuropeanExercise(exDate);
            StrikedTypePayoff payoff = new PlainVanillaPayoff(optionType.ToOptionType(), strike);

            BlackVolTermStructure volTS = new BlackConstantVol(today, new NullCalendar(), vol, dc); ;
            SimpleQuote qRate = new SimpleQuote(q);
            YieldTermStructure qTS = new FlatForward(today, qRate, dc);
            SimpleQuote rRate = new SimpleQuote(r);
            YieldTermStructure rTS = new FlatForward(today, rRate, dc);
            SimpleQuote u = new SimpleQuote(spot);

            VanillaOption option = MakeOption(payoff, exercise, u, qTS, rTS, volTS, EuropeanEngineType.Analytic);
            GeneralizedBlackScholesProcess stochProcess = BlackScholesMertonProcess(u, qTS, rTS, volTS);
            return option.impliedVolatility(targetPrice, stochProcess);
        }

        public static (double?, double?, double?, double?, double?, double?) AmericanOption(OptionType optionType, DateTime evalDate, double yearsToMaturity, double strike, double spot, double divYield, double rate, double vol, AmericanEngineType engineType)
        {
            DayCounter dc = new Actual360();

            Date today = evalDate;
            Date exDate = today + (int)(yearsToMaturity * 360);
            StrikedTypePayoff payoff = new PlainVanillaPayoff(optionType.ToOptionType(), strike);

            BlackVolTermStructure volTS = new BlackConstantVol(today, new NullCalendar(), vol, dc); ;
            SimpleQuote qRate = new SimpleQuote(divYield);
            YieldTermStructure qTS = new FlatForward(today, qRate, dc);
            SimpleQuote rRate = new SimpleQuote(rate);
            YieldTermStructure rTS = new FlatForward(today, rRate, dc);
            SimpleQuote u = new SimpleQuote(spot);

            Exercise exercise = new AmericanExercise(exDate);
            GeneralizedBlackScholesProcess stochProcess = BlackScholesMertonProcess(u, qTS, rTS, volTS);
            IPricingEngine engine = null;
            switch (engineType)
            {
                case AmericanEngineType.Barone_Adesi_Whaley:
                    engine = new BaroneAdesiWhaleyApproximationEngine(stochProcess);
                    break;
                case AmericanEngineType.Bjerksund_Stensland:
                    engine = new BjerksundStenslandApproximationEngine(stochProcess);
                    break;
                case AmericanEngineType.Binomial_Cox_Ross_Rubinstein:
                    engine = new BinomialVanillaEngine<CoxRossRubinstein>(stochProcess, 100);
                    break;
                default:
                    throw new NotSupportedException($"Not supported engine type {engineType}");
            }
            VanillaOption option = new VanillaOption(payoff, exercise);
            option.setPricingEngine(engine);

            double? npv = SafeExec(() => option.NPV());
            double? delta = SafeExec(() => option.delta());
            double? gamma = SafeExec(() => option.gamma());
            double? theta = SafeExec(() => option.theta());
            double? rho = SafeExec(() => option.rho());
            double? vega = SafeExec(() => option.vega());

            return (npv, delta, gamma, theta, rho, vega);
        }

        public static double? SafeExec(Func<double?> f)
        {
            try 
            { 
                return f(); 
            } 
            catch(Exception ex) { Console.WriteLine(ex.Message); }

            return null;
        }

        public static double AmericanOptionImpliedVol(OptionType optionType, Date evalDate, double yearsTomaturity, double strike, double spot, double divYield, double rate, double targetPrice)
        {       
            DayCounter dc = new Actual360();
            Date today = evalDate;
            Date exDate = today + (int)(yearsTomaturity * 360);
            var dividendDates = new[] { today + 30 };
            var dividendAmounts = new[] { divYield };
            double vol = 0.3;

            BlackVolTermStructure volTS = new BlackConstantVol(today, new NullCalendar(), vol, dc); ;
            SimpleQuote qRate = new SimpleQuote(divYield);
            YieldTermStructure qTS = new FlatForward(today, qRate, dc);
            SimpleQuote rRate = new SimpleQuote(rate);
            YieldTermStructure rTS = new FlatForward(today, rRate, dc);
            SimpleQuote u = new SimpleQuote(spot);
            GeneralizedBlackScholesProcess stochProcess = BlackScholesMertonProcess(u, qTS, rTS, volTS);
            var pricingEngine = new FDDividendAmericanEngine(stochProcess);
            var payoff = new PlainVanillaPayoff(optionType.ToOptionType(), strike);
         
            var exercise = new AmericanExercise(exDate);
            var option = new DividendVanillaOption(payoff, exercise, dividendDates.ToList(), dividendAmounts.ToList());
            option.setPricingEngine(pricingEngine);
            return option.impliedVolatility(targetPrice, stochProcess);
        }

        private static YieldTermStructure GetYieldCurve(Date settlementDate, uint fixingsDays, double[] rates, Calendar calendar, DayCounter dc)
        {
            var periods = new Period[]
            {
                new Period(1, TimeUnit.Days),
                new Period(1, TimeUnit.Weeks),
                new Period(1, TimeUnit.Months),
                new Period(2, TimeUnit.Months),
                new Period(3, TimeUnit.Months),
                new Period(6, TimeUnit.Months),
                new Period(12, TimeUnit.Months)
            };

            var instruments = new List<RateHelper>();
            for (int i = 0; i < rates.Length; i++)
            {
                var depositRate = new DepositRateHelper(rates[i], periods[i], (int)fixingsDays, calendar, BusinessDayConvention.ModifiedFollowing, true, dc);
                instruments.Add(depositRate);
            }

            return new PiecewiseYieldCurve<Discount, LogLinear>(settlementDate, instruments, new Actual360());
        }

        /// <summary>
        /// For our shirt-term options, we simply create three dividend data points: last dividend, current dividend and next dividend
        /// </summary>        
        private static InterpolatedZeroCurve<Linear> GetDividendCurve(Date evalDate, Date maturity, Date exDivDate, double spot, double dividend, int dividendFrequency, Calendar calendar)
        {
            // calculate dividend yield
            var annualDividend = dividend * 12.0 / dividendFrequency;
            const int settlementDays = 2;
            var dividendDiscountDays = settlementDays + calendar.businessDaysBetween(evalDate, maturity);
            double dividendYield = (annualDividend / spot) * dividendDiscountDays / 252;

            // Create the interpolated curve            
            var exDivDates = new List<Date>();
            var yields = new List<double>();

            // Last ex div data and yield
            var lastExDivDate = calendar.advance(exDivDate, new Period(-dividendFrequency, TimeUnit.Months), BusinessDayConvention.ModifiedPreceding, true);
            exDivDates.Add(lastExDivDate);
            yields.Add(dividendYield);

            // current announced ex div data and yield
            exDivDates.Add(exDivDate);
            yields.Add(dividendYield);

            // next projected ex div date and yield
            var nextExDivDate = calendar.advance(exDivDate, new Period(dividendFrequency, TimeUnit.Months), BusinessDayConvention.ModifiedPreceding, true);
            exDivDates.Add(nextExDivDate);
            yields.Add(dividendYield);

            return new InterpolatedZeroCurve<Linear>(exDivDates,
                                                          yields,
                                                          new ActualActual(ActualActual.Convention.ISMA),
                                                          new Linear(),
                                                          Compounding.Continuous,
                                                          Frequency.Annual);
        }

        public static BlackVolTermStructure GetVolCurve(Date evalDate, Date maturity, double[] strikes, double[] vols, Calendar calendar, DayCounter dc)
        {
            var expirations = new List<Date>() { maturity };
            var volMatrix = new Matrix(strikes.Length, 1);
            for (int i = 0; i < vols.Length; i++)
            {
                volMatrix[i, 0] = vols[i];
            }

            return new BlackVarianceSurface(evalDate, calendar, expirations, new List<double>(strikes), volMatrix, dc);
        }

        public static List<(double? strike, double? npv, double? delta, double? gamma, double? theta, double? rho, double? vega)> AmericanOptionRealWorld(OptionType optionType,
                                                                                                                                                          Date evalDate,
                                                                                                                                                          Date maturity,
                                                                                                                                                          double spot,
                                                                                                                                                          double[] strikes,
                                                                                                                                                          double[] vols,
                                                                                                                                                          double[] rates,
                                                                                                                                                          double dividend,
                                                                                                                                                          int dividendFrequency,
                                                                                                                                                          Date exDivDate,
                                                                                                                                                          AmericanEngineType engineType,
                                                                                                                                                          int timesteps)
        {
            DayCounter dc = new ActualActual();
            var settlementDate = evalDate + 2;
            SimpleQuote spot_ = new SimpleQuote(spot);
            Calendar calendar = new UnitedStates(UnitedStates.Market.NYSE);

            // build dividend term structure
            InterpolatedZeroCurve<Linear> dividendCurve = GetDividendCurve(evalDate, maturity, exDivDate, spot, dividend, dividendFrequency, calendar);

            // build yield term structure
            YieldTermStructure yieldTermStructure = GetYieldCurve(settlementDate, 2, rates, calendar, dc);

            // build volatility term structure
            BlackVolTermStructure volTermStructure = GetVolCurve(evalDate, maturity, strikes, vols, calendar, dc);

            // DEBUG
            var dividendCurve1 = new FlatForward(evalDate, new SimpleQuote(0.1), dc);
            yieldTermStructure = new FlatForward(evalDate, new SimpleQuote(0.1), dc);
            volTermStructure = new BlackConstantVol(evalDate, new NullCalendar(), 0.3, dc); 

            Exercise exercise = new AmericanExercise(settlementDate, maturity);
            GeneralizedBlackScholesProcess stochProcess = BlackScholesMertonProcess(spot_, dividendCurve1, yieldTermStructure, volTermStructure);
            IPricingEngine engine = null;
            switch (engineType)
            {
                case AmericanEngineType.Barone_Adesi_Whaley:
                    engine = new BaroneAdesiWhaleyApproximationEngine(stochProcess);
                    break;
                case AmericanEngineType.Bjerksund_Stensland:
                    engine = new BjerksundStenslandApproximationEngine(stochProcess);
                    break;
                case AmericanEngineType.Binomial_Cox_Ross_Rubinstein:
                    engine = new BinomialVanillaEngine<CoxRossRubinstein>(stochProcess, timesteps);
                    break;
                default:
                    throw new NotSupportedException($"Not supported engine type {engineType}");
            }

            List<(double? strike, double? npv, double? delta, double? gamma, double? theta, double? rho, double? vega)> result = new List<(double?, double?, double?, double?, double?, double?, double?)>();
            for (int i = 0; i < strikes.Length; i++)
            {
                var payoff = new PlainVanillaPayoff(optionType.ToOptionType(), strikes[i]);
                VanillaOption option = new VanillaOption(payoff, exercise);
                option.setPricingEngine(engine);
                double? npv = SafeExec(() => option.NPV());
                double? delta = SafeExec(() => option.delta());
                double? gamma = SafeExec(() => option.gamma());
                double? theta = SafeExec(() => option.theta());
                double? rho = SafeExec(() => option.rho());
                double? vega = SafeExec(() => option.vega());               
                result.Add((strikes[i], npv, delta, gamma, theta, rho, vega));
            }
            return result;
        }
    }

    /* using QuantLib (cpp)
    public class QuantLibHelper
    {        
        public static (double, double, double, double, double, double) EuropeanOption(Option.Type optionType, DateTime evalDate, DateTime maturity, double strike, double spot, double q, double r, double vol, EuropeanEngineType priceEngineType, int timesteps)
        {
            return EuropeanOption(optionType, new Date((int)evalDate.ToOADate()), new Date((int)maturity.ToOADate()), strike, spot, q, r, vol, priceEngineType, timesteps);
        }

        public static (double, double, double, double, double, double) EuropeanOption(Option.Type optionType, Date evalDate, Date maturity, double strike, double spot, double q, double r, double vol, EuropeanEngineType priceEngineType, int timesteps)
        {
            Date settlementDate = evalDate;
            QuoteHandle spot1 = new QuoteHandle(new SimpleQuote(spot));
            Calendar calendar = new TARGET();
            DayCounter dc = new Actual360();
            YieldTermStructureHandle qTS = new YieldTermStructureHandle(new FlatForward(settlementDate, q, dc));
            YieldTermStructureHandle rTS = new YieldTermStructureHandle(new FlatForward(settlementDate, r, dc));
            BlackVolTermStructureHandle volTS = new BlackVolTermStructureHandle(new BlackConstantVol(settlementDate, calendar, vol, dc));
           
            BlackScholesMertonProcess bsmProcess = new BlackScholesMertonProcess(spot1, qTS, rTS, volTS);
            PricingEngine engine;
            switch(priceEngineType)
            {
                case EuropeanEngineType.Analytic:
                    engine = new AnalyticEuropeanEngine(bsmProcess);
                    break;
                default:
                    throw new NotImplementedException($"{priceEngineType} not supported");
            }
            Payoff payoff = new PlainVanillaPayoff(optionType, strike);
            Exercise exercise = new EuropeanExercise(maturity);
            var option = new VanillaOption(payoff, exercise);
            option.setPricingEngine(engine);
            var value = option.NPV();
            var delta = option.delta();
            var gamma = option.gamma();
            var theta = option.theta();
            var rho = option.rho();
            var vega = option.vega();
            return (value, delta, gamma, theta, rho, vega );
        }

        public static double EuropeanOptionImpliedVol(Option.Type optionType, DateTime evalDate, double yearsToMaturity, double strike, double spot, double q, double r, double targetPrice)
        {
            Date startDate = new Date((int)evalDate.ToOADate());
            Date maturity = startDate.Add(Convert.ToInt32(yearsToMaturity * 360 + 0.5));
            return EuropeanOptionImpliedVol(optionType, startDate, maturity, strike, spot, q, r, targetPrice);
        }

        private static double EuropeanOptionImpliedVol(Option.Type optionType, Date evalDate, Date maturity, double strike, double spot, double q, double r, double targetPrice, double vol = 0.3)
        {
            Settings.instance().setEvaluationDate(evalDate);
            Date settlementDate = evalDate;
            Calendar calendar = new TARGET();
            DayCounter dc = new Actual360();
            QuoteHandle spot1 = new QuoteHandle(new SimpleQuote(spot));
            YieldTermStructureHandle qTS = new YieldTermStructureHandle(new FlatForward(settlementDate, q, dc));
            YieldTermStructureHandle rTS = new YieldTermStructureHandle(new FlatForward(settlementDate, r, dc));
            BlackVolTermStructureHandle volTS = new BlackVolTermStructureHandle(new BlackConstantVol(settlementDate, calendar, vol, dc));
            Payoff payoff = new PlainVanillaPayoff(optionType, strike);
            Exercise exercise = new EuropeanExercise(maturity);

            BlackScholesMertonProcess bsmProcess = new BlackScholesMertonProcess(spot1, qTS, rTS, volTS);
            var option = new VanillaOption(payoff, exercise);

            return option.impliedVolatility(targetPrice, bsmProcess, 1.0e-6, 10_000, 0, 4.0);
        }

        public static (double, double, double, double, double, double) EuropeanOption(Option.Type optionType, DateTime evalDate, double yearsToMaturity, double strike, double spot, double q, double r, double vol, EuropeanEngineType priceEngineType, int timesteps)
        {
            Date startDate = new Date((int)evalDate.ToOADate());
            Date maturity = startDate.Add(Convert.ToInt32(yearsToMaturity * 360 + 0.5));
            return EuropeanOption(optionType, startDate, maturity, strike, spot, q, r, vol, priceEngineType, timesteps);
        }        
    }  
    */
}
