using System;

namespace QuantBook.Models.Isda
{
    public class IsdaRate
    {
        public string Currency { get; set; }
        public string EffectiveAsOf { get; set; }
        public string BadDayConvention { get; set; }
        public string Calendar { get; set; }
        public DateTime SnapTime { get; set; }
        public string SpotDate { get; set; }
        public string Maturity { get; set; }
        public string DayCountConvention { get; set; }
        public string FixedDayCountConvention { get; set; }
        public string FloatingPaymentFrequency { get; set; }
        public string FixedPaymentFrequency { get; set; }       
        public string Tenor { get; set; }
        public string Rate { get; set; }
    }
}
