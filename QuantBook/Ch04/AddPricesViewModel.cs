using Caliburn.Micro;
using QuantBook.Models;
using QuantBook.Models.DataModel.Yahoo;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Ch04
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class AddPricesViewModel : Screen
    {
        private readonly IEventAggregator _events;
        [ImportingConstructor]
        public AddPricesViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "03. Add Prices";
            StartDate = Convert.ToDateTime("1/1/2010");
            EndDate = Convert.ToDateTime("1/1/2015");
            PriceCollection = new BindableCollection<Price>();
            TickerCollection = new BindableCollection<Symbol>();
            SymbolID = 1;
        }

        public BindableCollection<Price> PriceCollection { get; set; }
        public BindableCollection<Symbol> TickerCollection { get; set; }

        private int symbolId;

        public int SymbolID
        {
            get { return symbolId; }
            set { symbolId = value; NotifyOfPropertyChange(() => SymbolID); }
        }

        private DateTime startDate;

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; NotifyOfPropertyChange(() => StartDate); }
        }

        private DateTime endDate;

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; NotifyOfPropertyChange(() => EndDate); }
        }

        public async void GetPrice()
        {
            string tk = YahooHelper.IdToTicker(SymbolID);
            var prices = YahooHelper.GetYahooHistStockData(SymbolID, tk, StartDate, EndDate);
            PriceCollection.Clear();
            PriceCollection.AddRange(prices);
            await _events.PublishOnUIThreadAsync(new ModelEvents(new List<object>(new object[] { string.Format("Get Price From Yahoo: Ticker = {0}, Count={1}", tk, PriceCollection.Count) })));
        }

        public async void SavePrice()
        {
            if(PriceCollection.Count > 0)
            {
                YahooHelper.PriceInsert(PriceCollection);
                await _events.PublishOnUIThreadAsync(new ModelEvents(new List<object>(new object[] { "Save Price: Count = " + PriceCollection.Count.ToString() })));
            }
        }

        public async void GetPrices()
        {
            await Task.Run(async () =>
            {
                TickerCollection.Clear();
                TickerCollection.AddRange(YahooHelper.GetTickers());
                var objs = new List<object>();
                objs.Add("Get data from Yahoo:");
                objs.Add(0);
                objs.Add(TickerCollection.Count());
                objs.Add(0);

                int count = 0;
                foreach (var tc in TickerCollection)
                {
                    var price = YahooHelper.GetYahooHistStockData(tc.SymbolID, tc.Ticker, StartDate, EndDate);
                    objs[0] = $"Get data from Yahoo: Ticker = {tc.Ticker}, Count = {count}, Records = {price.Count}";
                    objs[3] = count;
                    await _events.PublishOnUIThreadAsync(new ModelEvents(objs));
                    count++;
                }
            });
        }
    }
}
