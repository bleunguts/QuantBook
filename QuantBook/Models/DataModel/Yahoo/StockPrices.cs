using System;

namespace QuantBook.Models.Yahoo 
{
    public class StockPrice
    {
        public StockPrice(string symbol)
        {
            Ticker = symbol;
        }
        public string Ticker { get; set; }
        public DateTime? Date { get; set; }
        public decimal? PriceOpen { get; set; }
        public decimal? PriceHigh { get; set; }
        public decimal? PriceLow { get; set; }
        public decimal? PriceClose { get; set; }
        public decimal? PriceAdj { get; set; }
        public decimal? Volume { get; set; }
    }

    public class YahooTicker
    {
        public YahooTicker(string symbol)
        {
            Ticker = symbol;
        }
        public string Ticker { get; set; }
    }
}
