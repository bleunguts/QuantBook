using Caliburn.Micro;
using QuantBook.Models.DataModel.Quandl;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Ch04
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class QuandlViewModel : Screen
    {
        private readonly IEventAggregator _events;
        [ImportingConstructor]
        public QuandlViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "06. Quandl";
            ticker = "FB";
            DataSource = "WIKI";
            DataLabel = "Data from Quandl";
            StartDate = new DateTime(2018, 2, 28);
            EndDate = new DateTime(2018, 5, 1);
        }

        private DataTable myTable;

        public DataTable MyTable
        {
            get { return myTable; }
            set { myTable = value; NotifyOfPropertyChange(() => MyTable); }
        }

        private string ticker;

        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; NotifyOfPropertyChange(() => Ticker); }
        }

        private string dataLabel;

        public string DataLabel
        {
            get { return dataLabel; }
            set { dataLabel = value; NotifyOfPropertyChange(() => DataLabel); }
        }

        private string dataSource;

        public string DataSource
        {
            get { return dataSource; }
            set { dataSource = value; NotifyOfPropertyChange(() => DataSource); }
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

        public async void GetData()
        {
            DataLabel = string.Format("Data for {0} from {1}:", Ticker, DataSource);
            var table = await QuandlHelper.GetQuandlDataAsync(Ticker, DataSource, StartDate, EndDate);
            MyTable = table;
        }
    }
}
