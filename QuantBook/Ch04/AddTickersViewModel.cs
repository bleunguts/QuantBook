using Caliburn.Micro;
using QuantBook.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QuantBook.Models.DataModel.Yahoo;

namespace QuantBook.Ch04
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class AddTickersViewModel : Screen
    {
        private readonly IEventAggregator _events;

        [ImportingConstructor]
        public AddTickersViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "02. Add Tickers";

            //TickerFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\StockTickers.csv";
            TickerFile = @"C:\temp\StockTickers.csv";
            TickerCollection = new BindableCollection<Symbol>();
        }

        public BindableCollection<Symbol> TickerCollection { get; set; }

        private string tickerFile;

        public string TickerFile
        {
            get => tickerFile;
            set { tickerFile = value; NotifyOfPropertyChange(() => TickerFile); }
        }


        private string ticker;

        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; NotifyOfPropertyChange(() => Ticker); }
        }

        private string region;

        public string Region
        {
            get { return region; }
            set { region = value; NotifyOfPropertyChange(() => Region); }
        }

        private string sector;

        public string Sector
        {
            get { return sector; }
            set { sector = value; NotifyOfPropertyChange(() => Sector); }
        }
     
        public async void LoadCsv()
        {
            var tickers = YahooHelper.CsvToSymbolCollection(TickerFile);
            TickerCollection.Clear();
            TickerCollection.AddRange(tickers);
            await _events.PublishOnUIThreadAsync(new ModelEvents(new List<object>(new object[] { "From CSV file loading: Count = " + TickerCollection.Count.ToString() })));
        }

        public async void AddTicker()
        {
            var symbol = new Symbol();
            symbol.Ticker = Ticker;
            symbol.Region = Region;
            symbol.Sector = Sector;

            YahooHelper.SymbolInsert(symbol);
            await _events.PublishOnUIThreadAsync(new ModelEvents(new List<object> { new object[] { "Add single ticker to Symbol: name = " + Ticker.ToString() } }));
        }

        public async void AddTickers()
        {
            YahooHelper.SymbolInsert(TickerCollection);
            await _events.PublishOnUIThreadAsync(new ModelEvents(new List<object>(new object[] { "Add Tickers to Symbol: Count =  = " + TickerCollection.Count.ToString() })));
        }

    }
}
