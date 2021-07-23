using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.DataModel.Yahoo
{
    public class StockQuote : PropertyChangedBase
    {
        public StockQuote(string ticker)
        {
            symbol = ticker;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(propertyName);
            return true;
        }

        private string symbol;
        public string Symbol { get => symbol; set => SetField(ref symbol, value); }

        private decimal? bid;

        public decimal? Bid
        {
            get => bid;
            set => SetField(ref bid, value);
        }

        private decimal? ask;

        public decimal? Ask
        {
            get => ask;
            set => SetField(ref ask, value);
        }

        private decimal? precentChange;

        public decimal? PercentChange
        {
            get => precentChange;
            set => SetField(ref precentChange, value);
        }

        private decimal? change;

        public decimal? Change
        {
            get => change;
            set => SetField(ref change, value);
        }

        private DateTime? lastTradeTime;

        public DateTime? LastTradeTime
        {
            get => lastTradeTime;
            set => SetField(ref lastTradeTime, value);
        }

        private decimal? dailyLow;

        public decimal? DailyLow
        {
            get => dailyLow;
            set => SetField(ref dailyLow, value);
        }

        private decimal? dailyHigh;

        public decimal? DailyHigh
        {
            get => dailyHigh;
            set => SetField(ref dailyHigh, value);
        }

        private decimal? yearlyLow;

        public decimal? YearlyLow
        {
            get => yearlyLow;
            set => SetField(ref yearlyLow, value);
        }

        private decimal? yearlyHigh;

        public decimal? YearlyHigh
        {
            get { return yearlyHigh; }
            set { SetField(ref yearlyHigh, value); }
        }


        private decimal? lastTradePrice;

        public decimal? LastTradePrice
        {
            get => lastTradePrice;
            set => SetField(ref lastTradePrice, value);
        }

        private string name;

        public string Name
        {
            get => name;
            set => SetField(ref name, value);
        }
        private decimal? open;

        public decimal? Open
        {
            get => open;
            set => SetField(ref open, value);
        }

        private decimal? volume;

        public decimal? Volume
        {
            get => volume;
            set => SetField(ref volume, value);
        }

        private string stockExchange;

        public string StockExchange
        {
            get => stockExchange;
            set => SetField(ref stockExchange, value);
        }

        private DateTime lastUpdate;

        public DateTime LastUpdate
        {
            get => lastUpdate;
            set => SetField(ref lastUpdate, value);
        }
    }
}
