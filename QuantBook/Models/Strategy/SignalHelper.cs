using Caliburn.Micro;
using QuantBook.Ch11;
using QuantBook.Models.DataModel.Quandl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace QuantBook.Models.Strategy
{
    public enum PriceTypeEnum {
        TypicalPrice,
        Close,
        Average
    }
    public enum SignalTypeEnum {
        MovingAverage,
        LinearRegression,
        RSI,
        WilliamR
    }

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
            throw new NotImplementedException();
        }

        private static IEnumerable<SignalEntity> RSINormalized(IEnumerable<SignalEntity> input, int movingWindow)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<SignalEntity> LinearRegression(IEnumerable<SignalEntity> input, int movingWindow)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<SignalEntity> MovingAverage(IEnumerable<SignalEntity> input, int movingWindow)
        {
            throw new NotImplementedException();
        }

        public static async Task<IEnumerable<SignalEntity>> GetStockDataAsync(string ticker, DateTime startDate, DateTime endDate, PriceTypeEnum priceType)
        {       
            var data = await MarketData.GetStockData(ticker, startDate, endDate);
            var signals = new List<SignalEntity>();

            foreach(var p in data)
            {
                double price = (double)p.AdjClose;
                switch(priceType)
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

       
    }
}
