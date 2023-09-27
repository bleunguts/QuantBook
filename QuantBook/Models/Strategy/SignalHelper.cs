using Caliburn.Micro;
using QLNet;
using QuantBook.Ch11;
using QuantBook.Models.AnalysisModel;
using QuantBook.Models.DataModel.Quandl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuantBook.Models.Strategy
{
    public class SignalHelper
    {
        public static IEnumerable<SignalEntity> GetSignal(IEnumerable<SignalEntity> input, int movingWindow, SignalTypeEnum signalType)
        {
            switch (signalType)
            {
                case SignalTypeEnum.MovingAverage:
                    return MovingAverage(input, movingWindow);
                case SignalTypeEnum.LinearRegression:
                    return LinearRegression(input, movingWindow);
                case SignalTypeEnum.RSI:
                    return RSINormalized(input, movingWindow);
                case SignalTypeEnum.WilliamR:
                    return WilliamsRNormalized(input, movingWindow);
            }
            throw new NotSupportedException($"SignalType = {signalType} not supported.");
        }

        private static IEnumerable<SignalEntity> WilliamsRNormalized(IEnumerable<SignalEntity> input, int movingWindow)
        {
            var results = new BindableCollection<SignalEntity>();
            for (int i = movingWindow - 1; i < input.Count(); i++)
            {
                var temp = new List<SignalEntity>();
                for (int j = i - movingWindow + 1; j <= i; j++)
                {
                    temp.Add(input.ElementAt(j));
                }

                double price = input.ElementAt(i).Price;
                double high = temp.Max(x => x.Price);
                double low = temp.Min(x => x.Price);
                // Formula is Signal = -4 * (highest high - Close) / (highest high - lowest low) + 2
                var williamsRResult = -4.0 * (high - price) / (high - low) + 2.0;
                results.Add(new SignalEntity
                {
                    Date = input.ElementAt(i).Date,
                    Ticker = input.ElementAt(i).Ticker,
                    Price = input.ElementAt(i).Price,
                    PricePredicted = input.ElementAt(i).Price,
                    UpperBand = input.ElementAt(i).Price,
                    LowerBand = input.ElementAt(i).Price,
                    Signal = williamsRResult,
                });
            }

            return results;
        }

        private static IEnumerable<SignalEntity> RSINormalized(IEnumerable<SignalEntity> input, int movingWindow)
        {
            var results = new BindableCollection<SignalEntity>();

            for (int i = movingWindow - 1; i < input.Count(); i++)
            {
                results.Add(new SignalEntity
                {
                    Date = input.ElementAt(i).Date,
                    Ticker = input.ElementAt(i).Ticker,
                    Price = input.ElementAt(i).Price,
                    PricePredicted = input.ElementAt(i).Price,
                    UpperBand = input.ElementAt(i).Price,
                    LowerBand = input.ElementAt(i).Price,
                    Signal = (double)CalculateRSIFrom(i),
                });
            }
            return results;

            double CalculateRSIFrom(int index)
            {
                var prices = input
                    .Skip(index - (movingWindow - 1))
                    .Take(movingWindow + 1)
                    .Select(i => i.Price)
                    .ToArray();
                return (double)CalculateRSI(prices);
            }
        }

        private static double CalculateRSI(double[] prices)
        {
            double sumGain = 0;
            double sumLoss = 0;
            for (int i = 1; i < prices.Length; i++)
            {
                double priceDiff = prices[i] - prices[i - 1];
                if (priceDiff >= 0)
                    sumGain += priceDiff;
                else
                    sumLoss -= priceDiff;
            }
            double rs = CalculateRS(sumGain, sumLoss);

            double rsi = 100.0 - 100.0 / (1.0 + rs);
            return (rsi - 50.0) / 25.0;

            double CalculateRS(double gains, double losses)
            {
                if (gains == 0)
                {
                    return 0.0;
                }
                if (Math.Abs(losses) < 1.0e-15)
                {
                    return 100.0;
                }
                return gains / losses;
            }
        }

        private static IEnumerable<SignalEntity> LinearRegression(IEnumerable<SignalEntity> raw, int movingWindow)
        {
            var rawSignals = new List<SignalEntity>(raw);
            var computedSignals = new List<SignalEntity>();

            for (int i = movingWindow - 1; i < rawSignals.Count; i++)
            {
                var xa = new List<double>();
                var ya = new List<double>();
                var tmp = new List<SignalEntity>();
                for (int j = i - movingWindow + 1; j <= i; j++)
                {
                    xa.Add(1.0 * j);
                    ya.Add(rawSignals[j].Price);
                    tmp.Add(rawSignals[j]);
                }
                var lr = AnalysisModel.LinearAnalysisHelper.GetSimpleRegression(xa, ya);
                double price = rawSignals[i].Price;
                double priceLR = lr.Alpha + lr.Beta * xa[xa.Count - 1];
                double std = tmp.StdDev(x => x.Price);
                double zscore = (price - priceLR) / std;
                computedSignals.Add(new SignalEntity
                {
                    Ticker = rawSignals[i].Ticker,
                    Date = rawSignals[i].Date,
                    Price = rawSignals[i].Price,
                    PricePredicted = priceLR,
                    UpperBand = priceLR + 2.0 * std,
                    LowerBand = priceLR - 2.0 * std,
                    Signal = zscore
                });
            }
            return computedSignals;
        }

        private static IEnumerable<SignalEntity> MovingAverage(IEnumerable<SignalEntity> raw, int movingWindow)
        {
            var rawSignals = new List<SignalEntity>(raw);
            var computedSignals = new List<SignalEntity>();

            for (int i = movingWindow - 1; i < rawSignals.Count; i++)
            {
                var temp = new List<SignalEntity>();
                for (int j = i - movingWindow + 1; j <= i; j++)
                {
                    temp.Add(rawSignals[j]);
                }
                double avg = temp.Average(x => x.Price);
                double std = temp.StdDev(x => x.Price);
                double price = rawSignals[i].Price;
                double zscore = (price - avg) / std;
                computedSignals.Add(new SignalEntity
                {
                    Ticker = rawSignals[i].Ticker,
                    Date = rawSignals[i].Date,
                    Price = rawSignals[i].Price,
                    PricePredicted = avg,
                    UpperBand = avg + 2.0 * std,
                    LowerBand = avg - 2.0 * std,
                    Signal = zscore
                });
            }
            return computedSignals;
        }

        public static async Task<IEnumerable<SignalEntity>> GetStockData(string ticker, DateTime startDate, DateTime endDate, PriceTypeEnum priceType)
        {
            var data = await MarketData.GetStockData(ticker, startDate, endDate);
            var signals = new List<SignalEntity>();

            foreach (var p in data)
            {
                double price = (double)p.AdjClose;
                switch (priceType)
                {
                    case PriceTypeEnum.Close:
                        price = (double)p.AdjClose;
                        break;
                    case PriceTypeEnum.Average:
                        price = (double)(p.AdjOpen + p.AdjHigh + p.AdjLow + p.AdjClose) / 4.0;
                        break;
                    case PriceTypeEnum.TypicalPrice:
                        price = (double)(p.AdjHigh + p.AdjLow + p.AdjClose) / 3.0;
                        break;
                }
                signals.Add(new SignalEntity
                {
                    Ticker = p.Ticker,
                    Date = p.Date.Value,
                    Price = price
                });
            }
            return signals;
        }

        public static IEnumerable<PairSignalEntity> GetPairCorrelation(string ticker1, string ticker2, DateTime startDate, DateTime endDate, int correlationWindow, out double[] betas)
        {
            var builder = new PairSignalBuilder();
            builder.Ticker1 = ticker1;
            builder.Ticker2 = ticker2;  
            var results = new List<PairSignalEntity>();
            for (int i = 0; i < 200; i++)
            {
                results.Add(builder.NewSignal(RandomPrice(), RandomPrice()));
            };
            
            betas = results.Select(x => x.Beta).ToArray();            
            return results;
        }
        
        private static double RandomPrice() 
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return random.Next(1, 99) + random.NextDouble();
        }

        internal static IEnumerable<PairSignalEntity> GetPairSignal(PairTypeEnum selectedPairType, PairSignalEntity[] pairSignalEntities, int movingWindow)
        {
            if(selectedPairType == PairTypeEnum.PriceRatio)
            {
                return GetPairPriceRatioSignal(pairSignalEntities, movingWindow);
            }
            else
            {
                return GetPairSpreadSignal(pairSignalEntities, movingWindow);
            }
        }

        internal static IEnumerable<PairSignalEntity> GetPairPriceRatioSignal(PairSignalEntity[] inputPairSignals, int movingWindow)
        {
            if (inputPairSignals.Length < 1)
            {
                return Enumerable.Empty<PairSignalEntity>();
            }

            var results = new List<PairSignalEntity>();
            for (int i = movingWindow - 1; i < inputPairSignals.Length; i++)
            {
                var signalsInWindow = inputPairSignals
                    .Skip(i - movingWindow + 1)
                    .Take(movingWindow - 1)
                    .ToList();
                double zscore = zScore(inputPairSignals[i].Spread, (double)signalsInWindow.Average(x => x.Spread), (double)signalsInWindow.StdDev(x => x.Spread));
                results.Add(new PairSignalEntity(inputPairSignals[i].Ticker1, inputPairSignals[i].Ticker2, inputPairSignals[i].Date, inputPairSignals[i].Price1, inputPairSignals[i].Price2, inputPairSignals[i].Correlation, inputPairSignals[i].Beta, zscore));                
            }
            return results;
        }        

        internal static IEnumerable<PairSignalEntity> GetPairSpreadSignal(PairSignalEntity[] inputPairSignals, int movingWindow)
        {
            if (inputPairSignals.Length < 1)
            {
                return Enumerable.Empty<PairSignalEntity>();
            }

            var results = new List<PairSignalEntity>();
            for (int i = movingWindow - 1; i < inputPairSignals.Length; i++)
            {
                var xa = inputPairSignals
                    .Skip(i - movingWindow + 1)
                    .Take(movingWindow - 1)
                    .Select(x => x.Price1)
                    .ToList();
                var ya = inputPairSignals
                    .Skip(i - movingWindow + 1)
                    .Take(movingWindow - 1)
                    .Select(x => x.Price2)
                    .ToList();
                var lr = AnalysisModel.LinearAnalysisHelper.GetSimpleRegression(ya, xa);
                double spread = Spread(xa[xa.Count - 1], ya[ya.Count - 1], lr.Beta);                
                double[] spreads = new double[xa.Count];
                for (int ii = 0; ii < xa.Count; ii++)
                {                   
                    spreads[ii] = Spread(xa[ii], ya[ii], lr.Beta);
                }
                double[] avgStd = ModelHelper.GetAvgStd(spreads);
                double zscore = zScore(spread, avgStd[0], avgStd[1]);
                results.Add(new PairSignalEntity(inputPairSignals[i].Ticker1, inputPairSignals[i].Ticker2, inputPairSignals[i].Date, inputPairSignals[i].Price1, inputPairSignals[i].Price2, inputPairSignals[i].Correlation, inputPairSignals[i].Beta, zscore));
            }

            return results;
        }

        static double Spread(double price1, double price2, double beta) => price1 - beta * price2;

        static double zScore(double spread, double avg, double std) => (spread - avg) / std;

      
    }
}
