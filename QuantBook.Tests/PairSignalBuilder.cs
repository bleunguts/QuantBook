using QLNet;
using QuantBook.Models.Strategy;
using System;

namespace QuantBook.Tests
{
    public class PairSignalBuilder
    {
        private const string ticker1 = "aTicker1";
        private const string ticker2 = "aTicker2";
        private Date date = new Date(DateTime.Now);

        public PairSignalBuilder()
        {
        }

        public PairSignalEntity NewSignal(double price1, double price2)
        {            
            return new PairSignalEntity(ticker1, ticker2, date, price1, price2, 1.0, 1.0, 1.0);
        }
    }
}
