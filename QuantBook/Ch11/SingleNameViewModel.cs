using Caliburn.Micro;
using QuantBook.Models;
using QuantBook.Models.Strategy;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuantBook.Ch11
{
 

    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class SingleNameViewModel : Screen
    {
        private readonly IEventAggregator events;

        [ImportingConstructor]
        public SingleNameViewModel(IEventAggregator events)
        {
            this.events = events;
            DisplayName = "01. Single Name";
            LineSeriesCollection1 = new BindableCollection<Series>();
            LineSeriesCollection2 = new BindableCollection<Series>();
            SignalCollection = new BindableCollection<SignalEntity>();
            PnlCollection = new BindableCollection<PnlEntity>();

            // These parameters are specific to the amount of resultant data from Quandl API 
            // Moving Window < Size of data, the smaller the bigger the Signal data set
            Ticker = "IBM";
            StartDate = new DateTime(2012, 3, 1);
            EndDate = new DateTime(2013, 7, 31);
            MovingWindow = 5;
        }

        public BindableCollection<Series> LineSeriesCollection1 { get; set; }
        public BindableCollection<Series> LineSeriesCollection2 { get; set; }
        public BindableCollection<SignalEntity> SignalCollection { get; set; }
        public BindableCollection<PnlEntity> PnlCollection { get; set; }

        private string ticker = "IBM";

        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; NotifyOfPropertyChange(() => Ticker); }
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

        private int movingWindow = 30;

        public int MovingWindow
        {
            get { return movingWindow; }
            set { movingWindow = value; NotifyOfPropertyChange(() => MovingWindow); }
        }

        private IEnumerable<PriceTypeEnum> priceType;

        public IEnumerable<PriceTypeEnum> PriceType
        {
            get { return Enum.GetValues(typeof(PriceTypeEnum)).Cast<PriceTypeEnum>(); }
            set { priceType = value; NotifyOfPropertyChange(() => PriceType); }
        }

        private PriceTypeEnum selecetedPriceType;

        public PriceTypeEnum SelectedPriceType
        {
            get { return selecetedPriceType; }
            set { selecetedPriceType = value; NotifyOfPropertyChange(() => SelectedPriceType); }
        }

        private IEnumerable<SignalTypeEnum> signalType;

        public IEnumerable<SignalTypeEnum> SignalType
        {
            get { return Enum.GetValues(typeof(SignalTypeEnum)).Cast<SignalTypeEnum>(); }
            set { signalType = value; NotifyOfPropertyChange(() => SignalType); }
        }

        private SignalTypeEnum selectedSignalType;

        public SignalTypeEnum SelectedSignalType
        {
            get { return selectedSignalType; }
            set { selectedSignalType = value; NotifyOfPropertyChange(() => SelectedSignalType); }
        }

        private IEnumerable<StrategyTypeEnum> strategyType;

        public IEnumerable<StrategyTypeEnum> StrategyType
        {
            get { return Enum.GetValues(typeof(StrategyTypeEnum)).Cast<StrategyTypeEnum>(); }
            set { strategyType = value; NotifyOfPropertyChange(() => StrategyType); }
        }

        private StrategyTypeEnum selectedStrategyType;

        public StrategyTypeEnum SelectedStrategyType
        {
            get { return selectedStrategyType; }
            set { selectedStrategyType = value; }
        }

        private double notional = 10_000;

        public double Notional
        {
            get { return notional; }
            set { notional = value; NotifyOfPropertyChange(() => Notional); }
        }

        private double signalIn = 2.0;

        public double SignalIn
        {
            get { return signalIn; }
            set { signalIn = value; NotifyOfPropertyChange(() => SignalIn); }
        }

        private double signalOut = 0;

        public double SignalOut
        {
            get { return signalOut; }
            set { signalOut = value; NotifyOfPropertyChange(() => SignalOut); }
        }

        private bool isReinvest = false;

        public bool IsReinvest
        {
            get { return isReinvest; }
            set { isReinvest = value; NotifyOfPropertyChange(() => IsReinvest); }
        }

        private string title1 = string.Empty;

        public string Title1
        {
            get { return title1; }
            set { title1 = value; NotifyOfPropertyChange(() => Title1); }
        }

        private string title2 = string.Empty;

        public string Title2
        {
            get { return title2; }
            set { title2 = value; NotifyOfPropertyChange(() => Title2); }
        }

        private string ylabel1 = string.Empty;

        public string YLabel1
        {
            get { return ylabel1; }
            set { ylabel1 = value; NotifyOfPropertyChange(() => YLabel1); }
        }

        private string ylabel2;

        public string YLabel2
        {
            get { return ylabel2; }
            set { ylabel2 = value; NotifyOfPropertyChange(() => YLabel2); }
        }

        private DataTable yearlyPnlTable;

        public DataTable YearlyPnlTable
        {
            get { return yearlyPnlTable; }
            set { yearlyPnlTable = value; NotifyOfPropertyChange(() => YearlyPnlTable); }
        }

        DataTable drawdownTable = new DataTable();
        public void DrawdownStrategy() => Drawdown(true);
        public void DrawdownHold() => Drawdown(false);

        private void Drawdown(bool isStrategy)
        {
            YLabel2 = "Drawdown (%)";
            Series ds = new Series();
            if(isStrategy)
            {
                Title2 = string.Format("{0}: Drawdown for Signal Type = {1}", Ticker, SelectedSignalType);
                LineSeriesCollection2.Clear();
                ds = new Series
                {
                    ChartType = SeriesChartType.Line,
                    Name = "Drawdown"
                };
                foreach(DataRow row in drawdownTable.Rows)
                {
                    ds.Points.AddXY(row["Date"], Convert.ToDouble(row["Drawdown"]));
                }
                LineSeriesCollection2.Add(ds);

                ds = new Series
                {
                    ChartType = SeriesChartType.Line,
                    Color = System.Drawing.Color.Red,
                    Name = "MaxDrawdown"
                };
                foreach(DataRow row in drawdownTable.Rows)
                {
                    ds.Points.AddXY(row["Date"], row["MaxDrawdown"]);
                }
                LineSeriesCollection2.Add(ds);
            }
            else
            {
                Title2 = $"{Ticker}: Drawdown for Holding";
                LineSeriesCollection2.Clear();
                ds = new Series()
                {
                    ChartType = SeriesChartType.Line,
                    Name = "DrawndownHold"
                };
                foreach(DataRow row in drawdownTable.Rows)
                {
                    ds.Points.AddXY(row["Date"], row["DrawdownHold"]);
                }
                LineSeriesCollection2.Add(ds);

                ds = new Series
                {
                    ChartType = SeriesChartType.Line,
                    Color = System.Drawing.Color.Red,
                    Name = "MaxDrawdownHold"
                };
                foreach(DataRow row in drawdownTable.Rows)
                {
                    ds.Points.AddXY(row["Date"], row["MaxDrawdownHold"]);
                }
                LineSeriesCollection2.Add(ds);
            }
        }
        public async void GetSignalData()
        {
            PnlCollection.Clear();
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();
            var data = await SignalHelper.GetStockData(Ticker, StartDate, EndDate, SelectedPriceType);
            var signal = SignalHelper.GetSignal(data, MovingWindow, SelectedSignalType);
            SignalCollection.Clear();
            SignalCollection.AddRange(signal);
            AddSignalCharts();
        }      

        public void ComputePnl()
        {
            var pnl = BacktestHelper.ComputeLongShortPnl(SignalCollection, Notional, SignalIn, SignalOut, SelectedStrategyType, IsReinvest);
            PnlCollection.Clear();
            PnlCollection.AddRange(pnl);
            DataTable dt = BacktestHelper.GetYearlyPnl(PnlCollection);
            YearlyPnlTable = dt;
            drawdownTable = BacktestHelper.GetDrawDown(PnlCollection, Notional);
            AddPnlCharts();
        }

        private void AddSignalCharts()
        {
            Title1 = $"{Ticker}: Stock Price (Price Type = {SelectedPriceType}, Signal Type = {SelectedSignalType})";
            Title2 = $"{Ticker}: Signal (Price Type = {SelectedPriceType}, Signal Type = {SelectedSignalType})";
            YLabel1 = "Stock Price";
            YLabel2 = "Signal";
            LineSeriesCollection1.Clear();
            Series ds = new Series
            {
                ChartType = SeriesChartType.Line,
                MarkerStyle = MarkerStyle.Diamond,
                MarkerSize = 4,
                Name = "Original Price"
            };
            foreach (var p in SignalCollection)
            {
                ds.Points.AddXY(p.Date, p.Price);
            }
            LineSeriesCollection1.Add(ds);

            ds = new Series
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                Name = "Predicted Price"
            };
            foreach(var p in SignalCollection)
            {
                ds.Points.AddXY(p.Date, p.PricePredicted);
            }
            LineSeriesCollection1.Add(ds);

            ds = new Series
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.DarkGreen,
                Name = "Upper Band"
            };
            foreach (var p in SignalCollection)
            {
                ds.Points.AddXY(p.Date, p.UpperBand);
            }
            LineSeriesCollection1.Add(ds);

            ds = new Series
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.DarkGreen,
                Name = "Lower Band"
            };
            foreach (var p in SignalCollection)
            {
                ds.Points.AddXY(p.Date, p.LowerBand);
            }
            LineSeriesCollection1.Add(ds);

            LineSeriesCollection2.Clear();
            ds = new Series
            {
                ChartType = SeriesChartType.Line
            };
            foreach(var p in SignalCollection)
            {
                ds.Points.AddXY(p.Date, p.Signal);
            }
            LineSeriesCollection2.Add(ds);
        }

        private void AddPnlCharts()
        {
            DataRow latestPnlRow = YearlyPnlTable.Rows[YearlyPnlTable.Rows.Count - 1];
            double pnl = Math.Round(latestPnlRow["PnL"].To<double>(), 0);
            double sharpe = Math.Round(latestPnlRow["Sharpe"].To<double>(), 3);
            int numTrades = latestPnlRow["NumTrades"].To<int>();
            Title1 = $"{Ticker}: P&L (Total PnL = {pnl}, Sharpe = {sharpe}, NumTrades = {numTrades}";
            YLabel1 = "Cummulated P&L";

            LineSeriesCollection1.Clear();
            Series ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Name = "PnL for Holding Position"
            };
            foreach(var p in PnlCollection)
            {
                ds.Points.AddXY(p.Date, p.PnLCumHold);
            }
            LineSeriesCollection1.Add(ds);
            ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                Name = "PnL"
            };
            foreach (var p in PnlCollection)
            {
                ds.Points.AddXY(p.Date, p.PnLCum);
            }
            LineSeriesCollection1.Add(ds);
            Drawdown(true);
        }
    }
}
