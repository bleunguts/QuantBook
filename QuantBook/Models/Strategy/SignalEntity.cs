using System;

namespace QuantBook.Models.Strategy
{
    public class SignalEntity
    {
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public double Price { get;  set; }

        // signal stuff
        public double UpperBand { get;  set; }
        public double PricePredicted { get;  set; }
        public double LowerBand { get;  set; }
        public double Signal { get; set; }

        public override string ToString()
        {
            return $"Date={Date},Price={Price},UpperBand={UpperBand},PricePredicted={PricePredicted},LowerBand={LowerBand},Signal={Signal},Ticker={Ticker}";
        }
    }
}
