using QLNet;
using System;
using System.Collections.Generic;
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

    public class QuantLibHelper
    {        
        private static BlackScholesMertonProcess BlackScholesMertonProcess(Quote u, YieldTermStructure q, YieldTermStructure r, BlackVolTermStructure vol)=> new BlackScholesMertonProcess(
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
            return (option.NPV(), option.delta(), option.gamma(), option.theta(), option.rho(), option.theta());
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
