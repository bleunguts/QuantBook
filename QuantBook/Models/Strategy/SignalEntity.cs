using System;

namespace QuantBook.Models.Strategy
{
    public class SignalEntity
    {
        virtual public string Ticker { get; set; }
        virtual public DateTime Date { get; set; }
        virtual public double Price { get;  set; }

        // signal stuff
        virtual public double UpperBand { get;  set; }
        virtual public double PricePredicted { get;  set; }
        virtual public double LowerBand { get;  set; }
        virtual public double Signal { get; set; }

        public override string ToString()
        {
            return $"Date={Date},Price={Price},UpperBand={UpperBand},PricePredicted={PricePredicted},LowerBand={LowerBand},Signal={Signal},Ticker={Ticker}";
        }
    }
}
