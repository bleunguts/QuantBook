using Moq;
using QuantBook.Models.Strategy;
using System;

namespace QuantBook.Tests
{
    public class SignalBuilder
    {
        private readonly double basePrice;
        private readonly Func<double, double> randomizer;
        private DateTime currentDate;
        private readonly string ticker = "aTicker";
        private static Random random = new Random();

        public SignalBuilder(DateTime date) : this(date, null, 1.5) { }
        public SignalBuilder(DateTime date, Func<double, double> randomFunction, double basePrice)
        {
            this.currentDate = date;
            this.basePrice = basePrice;
            randomizer = randomFunction ?? new Func<double, double>(RandomPrice);
        }

        public SignalEntity NewSignal(double signal)
        {
            var entity = new Mock<SignalEntity>();
            entity.SetupGet(x => x.Date).Returns(currentDate);
            var price = randomizer(basePrice);
            entity.SetupGet(x => x.Price).Returns(price);
            entity.SetupGet(x => x.Signal).Returns(signal);
            entity.SetupGet(x => x.Ticker).Returns(ticker);

            MoveNext();
            return entity.Object;
        }

        private void MoveNext()
        {
            currentDate = currentDate.AddDays(1);
        }

        static double RandomPrice(double basePrice)
        {
            int multiplier = random.Next(0, 1) == 0 ? -1 : 1;
            return basePrice + random.NextDouble() * multiplier;
        }
    }    
}
