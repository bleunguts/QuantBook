using QLNet;
using System;

namespace QuantBook.Models.Strategy
{
    public class PairSignalBuilder
    {
        public string Ticker1 { get; set; } = "aTicker1";
        public string Ticker2 { get; set; } = "aTicker2";

        private Random random = new Random();

        private static int counter = 0;
        private static DateTime date = DateTime.Now.AddDays(-100);

        public PairSignalEntity NewSignal(double price1, double price2)
        {
            BumpDate();
            var correlation = 1.0 + random.NextDouble();
            var beta = 2.0 + random.NextDouble();   
            return new PairSignalEntity(Ticker1, Ticker2, date, price1, price2, correlation, beta, 1.0);
        }

        private static void BumpDate() => date = date.AddDays(counter++);
    }
}
