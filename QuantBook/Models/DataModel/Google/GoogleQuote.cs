using System;
using Caliburn.Micro;

namespace QuantBook.Models.DataModel.Google
{
    public class GoogleQuote : PropertyChangedBase
    {
         public GoogleQuote(string ticker)
        {
            symbol = ticker;
        }
        
        private string symbol;
        private string name;
        private string lastUpdate;
        private string lastTradeTime;
        private string tradeTimeDetail;
        private string lastQuotePrice;
        private string lastTradePrice;
        private string open;
        private string previousClose;
        private string stockExchange;
        private string volume;
        private string dailyLow;
        private string dailyHigh;
        private string change;
        private string percentChange;
        private string yearlyLow;
        private string yearlyHigh;       

        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                NotifyOfPropertyChange(() => Symbol);
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string LastUpdate
        {
            get { return lastUpdate; }
            set
            {
                lastUpdate = value;
                NotifyOfPropertyChange(() => LastUpdate);
            }
        }

        public string LastTradeTime
        {
            get { return lastTradeTime; }
            set
            {
                lastTradeTime = value;
                NotifyOfPropertyChange(() => LastTradeTime);
            }
        }

        public string TradeTimeDetail
        {
            get { return tradeTimeDetail; }
            set
            {
                tradeTimeDetail = value;
                NotifyOfPropertyChange(() => TradeTimeDetail);
            }
        }

        public string LastQuotePrice
        {
            get { return lastQuotePrice; }
            set
            {
                lastQuotePrice = value;
                NotifyOfPropertyChange(() => LastQuotePrice);
            }
        }

        public string LastTradePrice
        {
            get { return lastTradePrice; }
            set
            {
                lastTradePrice = value;
                NotifyOfPropertyChange(() => LastTradePrice);
            }
        }


        public string Open
        {
            get { return open; }
            set
            {
                open = value;
                NotifyOfPropertyChange(() => Open);
            }
        }

        public string PreviousClose
        {
            get { return previousClose; }
            set
            {
                previousClose = value;
                NotifyOfPropertyChange(() => PreviousClose);
            }
        }

        public string StockExchange
        {
            get { return stockExchange; }
            set
            {
                stockExchange = value;
                NotifyOfPropertyChange(() => StockExchange);
            }
        }       

        public string Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                NotifyOfPropertyChange(() => Volume);
            }
        }

        public string DailyHigh
        {
            get { return dailyHigh; }
            set
            {
                dailyHigh = value; 
                NotifyOfPropertyChange(() => DailyHigh); 
            }
        }
        
        public string DailyLow
        {
            get { return dailyLow; }
            set
            {
                dailyLow = value;
                NotifyOfPropertyChange(() => DailyLow); 
            }
        }               

        public string Change
        {
            get { return change; }
            set
            {
                change = value;
                NotifyOfPropertyChange(() => Change);
            }
        }

        public string PercentChange
        {
            get { return percentChange; }
            set
            {
                percentChange = value;
                NotifyOfPropertyChange(() => PercentChange);
            }
        }        

        public string YearlyHigh
        {
            get { return yearlyHigh; }
            set
            {
                yearlyHigh = value;
                NotifyOfPropertyChange(() => YearlyHigh);
            }
        }

        public string YearlyLow
        {
            get { return yearlyLow; }
            set
            {
                yearlyLow = value;
                NotifyOfPropertyChange(() => YearlyLow);
            }
        }

    }
}
