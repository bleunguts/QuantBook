using Caliburn.Micro;
using QuantBook.Models.DataModel.Yahoo;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QuantBook.Ch04
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class YahooStockViewModel : Screen
    {
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
        private readonly IEventAggregator _events;
        private DataTable myPrices;
        private string ticker;
        private DateTime startDate;
        private DateTime endDate;

        [ImportingConstructor]
        public YahooStockViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "01. Yahoo Stock";
            MyQuotes = new BindableCollection<StockQuote>();
            MyPrices = new DataTable();
            Ticker = "IBM";
            StartDate = DateTime.Today.AddYears(-5);
            EndDate = DateTime.Today;
        }

        public BindableCollection<StockQuote> MyQuotes { get; private set; }
        public DataTable MyPrices { get => myPrices; set => SetField(ref myPrices, value); }
        public string Ticker { get => ticker; private set => SetField(ref ticker, value); }
        public DateTime StartDate { get => startDate; set => SetField(ref startDate, value); }
        public DateTime EndDate { get => endDate; set => SetField(ref endDate, value); }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(propertyName);
            return true;

        }

        public void HistPrices()
        {
            MyPrices = YahooHelper.GetYahooHistStockDataTAble(Ticker, StartDate, EndDate);
        }

        public void StockQuotes()
        {
            MyQuotes.Clear();
            MyQuotes.Add(new StockQuote("^IXIC"));
            MyQuotes.Add(new StockQuote("^GSPC"));
            MyQuotes.Add(new StockQuote("MSFT"));
            MyQuotes.Add(new StockQuote("INTC"));
            MyQuotes.Add(new StockQuote("IBM"));
            MyQuotes.Add(new StockQuote("AMZN"));
            MyQuotes.Add(new StockQuote("BIDU"));
            MyQuotes.Add(new StockQuote("SINA"));
            MyQuotes.Add(new StockQuote("NVDA"));
            MyQuotes.Add(new StockQuote("AMD"));
            MyQuotes.Add(new StockQuote("WMT"));
            MyQuotes.Add(new StockQuote("GLD"));
            MyQuotes.Add(new StockQuote("SLV"));
            MyQuotes.Add(new StockQuote("V"));
            MyQuotes.Add(new StockQuote("MCD"));

            YahooHelper.GetQuotes(MyQuotes);
            
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += (_, e) => YahooHelper.GetQuotes(MyQuotes);
            timer.Start();           
        }
    }
 }
