using Caliburn.Micro;
using QLNet;
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
    public class PairsTradingViewModel: Screen
    {
        private readonly IEventAggregator _events;

        [ImportingConstructor]
        public PairsTradingViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "03. Pairs Trading";
        }
        private DateTime startDate = new DateTime(2010, 1, 3);
        private DateTime endDate = new DateTime(2015, 12, 31);
        private string ticker1 = "QQQ";
        private string ticker2 = "SPY";
        private int correlationWindow = 100;
        private IEnumerable<PairTypeEnum> pairType;
        private PairTypeEnum selectedPairType;
        private double hedgeRatio = 1.0;
        private string title1;
        private string title2;
        private string title3;
        private string title4;
        private string title5;
        private string title6;
        private DataTable yearlyPnLTable = null;
        DataTable drawdownTable = new DataTable();
        double[] betas;
        private int notional = 10_000;
        private double signalIn = 2.0;
        private double signalOut = 0;
        private int movingWindow = 3;

        #region Properties
        public BindableCollection<Series> LineSeries1 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries2 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries3 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries4 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries5 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries6 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<PairSignalEntity> PairCollection { get; set; } = new BindableCollection<PairSignalEntity>();
        public BindableCollection<PairPnlEntity> PnLCollection { get; set; } = new BindableCollection<PairPnlEntity>();
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; NotifyOfPropertyChange(() => StartDate); }
        }
        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; NotifyOfPropertyChange(() => EndDate); }
        }
        public string Ticker1
        {
            get { return ticker1; }
            set { ticker1 = value; NotifyOfPropertyChange(() => Ticker1); }
        }
        public string Ticker2
        {
            get { return ticker2; }
            set { ticker2 = value; NotifyOfPropertyChange(() => Ticker2); }
        }
        public int CorrelationWindow
        {
            get { return correlationWindow; }
            set { correlationWindow = value; NotifyOfPropertyChange(() => CorrelationWindow); }
        }
        public IEnumerable<PairTypeEnum> PairType
        {
            get { return Enum.GetValues(typeof(PairTypeEnum)).Cast<PairTypeEnum>(); }
            set { pairType = value; NotifyOfPropertyChange(() => PairType); }
        }
        public PairTypeEnum SelectedPairType
        {
            get { return selectedPairType; }
            set { selectedPairType = value; NotifyOfPropertyChange(() => SelectedPairType); }
        }
        public double HedgeRatio
        {
            get { return hedgeRatio; }
            set { hedgeRatio = value; NotifyOfPropertyChange(() => HedgeRatio); }
        }     
        public string Title1
        {
            get { return title1; }
            set { title1 = value; NotifyOfPropertyChange(() => Title1); }
        }
        public string Title2
        {
            get { return title2; }
            set { title2 = value; NotifyOfPropertyChange(() => Title2); }
        }
        public string Title3
        {
            get { return title3; }
            set { title3 = value; NotifyOfPropertyChange(() => title3); }
        }
        public string Title4
        {
            get { return title4; }
            set { title4 = value; NotifyOfPropertyChange(() => title4); }
        }
        public string Title5
        {
            get { return title5; }
            set { title5 = value; NotifyOfPropertyChange(() => title5); }
        }
        public string Title6
        {
            get { return title6; }
            set { title6 = value; NotifyOfPropertyChange(() => title6); }
        }
        public DataTable YearlyPnlTable
        {
            get { return yearlyPnLTable; }
            set { yearlyPnLTable = value; NotifyOfPropertyChange(() => YearlyPnlTable); }
        }
        public int MovingWindow
        {
            get { return movingWindow; }
            set { movingWindow = value; NotifyOfPropertyChange(() => MovingWindow); }
        }        
        public int Notional
        {
            get { return notional; }
            set { notional = value; NotifyOfPropertyChange(() => Notional); }
        }
        public double SignalIn
        {
            get { return signalIn; }
            set { signalIn = value; NotifyOfPropertyChange(() => SignalIn); }
        }
        public double SignalOut
        {
            get { return signalOut; }
            set { signalOut = value; NotifyOfPropertyChange(() => SignalOut); }
        }
        #endregion

        public void GetSignal()
        {
            PnLCollection.Clear();
            PairCollection.Clear();
            LineSeries1.Clear();
            LineSeries2.Clear();
            LineSeries3.Clear();
            LineSeries4.Clear();
            LineSeries5.Clear();
            LineSeries6.Clear();

            var pairSignals = SignalHelper.GetPairCorrelation(Ticker1, Ticker2, StartDate, EndDate, CorrelationWindow, out betas);
            var pair = SignalHelper.GetPairSignal(SelectedPairType, pairSignals.ToArray(), movingWindow);
            PairCollection.Clear();
            PairCollection.AddRange(pair);
            AddPriceCharts();
        }

        private void AddPriceCharts()
        {
            Title1 = $"{Ticker1},{Ticker2}: Stock Price";
            LineSeries1.Clear();
            Series ds = new Series()
            {
                ChartType =  SeriesChartType.Line,
                Name = $"{Ticker1} Price"
            };
            foreach (var point in PairCollection)
            {
                ds.Points.AddXY(point.Date, point.Price1);
            }
            LineSeries1.Add(ds);
            
            ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                Name = $"{Ticker2} Price",
                YAxisType = AxisType.Secondary
            };            
            foreach (var point in PairCollection)
            {
                ds.Points.AddXY(point.Date, point.Price2);
            }            
            LineSeries1.Add(ds);

            Title2 = $"{Ticker1}, {Ticker2}:Correlation (Correl_Avg={Math.Round(betas[2], 3)}, Correl_All = {Math.Round(betas[3], 3)})";
            LineSeries2.Clear();
            ds = new Series()
            {
                ChartType = SeriesChartType.Line,
            };            
            foreach(var p in PairCollection)
            {
                ds.Points.AddXY(p.Date, p.Correlation);
            }
            LineSeries2.Add(ds);

            Title3 = $"{Ticker1}, {Ticker2}: Beta (Beta_Avg={Math.Round(betas[0],3)}, Beta_All = {Math.Round(betas[1],3)})";
            LineSeries3.Clear();
            ds = new Series() 
            {
                ChartType = SeriesChartType.Line 
            };            
            foreach(var point in PairCollection)
            {
                ds.Points.AddXY(point.Date, point.Beta);
            }
            LineSeries3.Add(ds);

            Title4 = $"{Ticker1}, {Ticker2}: Signal";
            LineSeries4.Clear();
            ds = new Series()
            {
                ChartType = SeriesChartType.Line
            };
            foreach (var point in PairCollection)
            {
                ds.Points.AddXY(point.Date, point.Signal);
            }
            LineSeries4.Add(ds);
        }

        public void ComputePnL()
        {
            (IEnumerable<PairPnlEntity> pairEntities, IEnumerable<PnlEntity> pnlEntities) = BacktestHelper.ComputePnLPair(PairCollection.ToArray(), Notional, SignalIn, SignalOut, HedgeRatio);
            PnLCollection.Clear();
            PnLCollection.AddRange(pairEntities);
            List<(string ticker, string year, int numTrades, double pnl, double sp0, double pnl2, double sp1)> pnls = BacktestHelper.GetYearlyPnl(new List<PnlEntity>(pnlEntities));            
            YearlyPnlTable = ToDataTable(pnls);
            List<DrawDownResult> drawDownResults = BacktestHelper.GetDrawDown(new List<PnlEntity>(pnlEntities), Notional);
            drawdownTable = ToDataTable(drawDownResults);
            AddPnlCharts();
        }

        private void AddPnlCharts()
        {
            var length = YearlyPnlTable.Rows.Count;
            double pnl = YearlyPnlTable.Rows[length - 1]["pnl"].To<double>();
            double sharpe = YearlyPnlTable.Rows[length - 1]["sp1"].To<double>();
            int numTrades = YearlyPnlTable.Rows[length - 1]["numTrades"].To<int>();
            
            Title5 = $"{Ticker1},{Ticker2}: P&L (Total Pnl= {pnl}, Sharpe= {sharpe}, NumTrades={numTrades}";
            LineSeries5.Clear();
            Series ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                Name = "PnL"
            };
            foreach(var p in PnLCollection)
            {
                ds.Points.AddXY(p.Date, p.PnLCum1);
            }
            LineSeries5.Add(ds);

            Title6 = $"{Ticker1},{Ticker2}: Drawdown";
            LineSeries6.Clear();
            ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Name = "MaxDrawdwon"
            };
            foreach(DataRow row in drawdownTable.Rows)
            {
                ds.Points.AddXY(row["Date"].To<DateTime>(), row["drawdown"].To<double>());
            }
            LineSeries6.Add(ds);
        }

        private DataTable ToDataTable(List<DrawDownResult> drawdowns)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("date", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("pnl", typeof(double)));
            dt.Columns.Add(new DataColumn("drawdown", typeof(double)));
            dt.Columns.Add(new DataColumn("drawup", typeof(double)));
            dt.Columns.Add(new DataColumn("pnlHold", typeof(double)));
            dt.Columns.Add(new DataColumn("drawdownHold", typeof(double)));
            dt.Columns.Add(new DataColumn("drawupHold", typeof(double)));
            foreach (var d in drawdowns)
            {
                var dr = dt.NewRow();
                dr["date"] = d.date;
                dr["pnl"] = d.pnl;
                dr["drawdown"] = d.drawdown;
                dr["drawup"] = d.drawup;
                dr["pnlHold"] = d.pnlHold;
                dr["drawdownHold"] = d.drawdownHold;
                dr["drawupHold"] = d.drawupHold;
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private static DataTable ToDataTable(List<(string ticker, string year, int numTrades, double pnl, double sp0, double pnl2, double sp1)> pnls)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ticker", typeof(string)));   
            dt.Columns.Add(new DataColumn("year", typeof(string)));   
            dt.Columns.Add(new DataColumn("numTrades", typeof(int)));   
            dt.Columns.Add(new DataColumn("pnl", typeof(double)));   
            dt.Columns.Add(new DataColumn("sp0", typeof(double)));   
            dt.Columns.Add(new DataColumn("pnl2", typeof(double)));   
            dt.Columns.Add(new DataColumn("sp1", typeof(double)));   

            foreach (var p in pnls)
            {
                var dr = dt.NewRow();
                dr["ticker"] = p.ticker;
                dr["year"] = p.year;
                dr["numTrades"] = p.numTrades;
                dr["pnl"] = p.pnl;
                dr["sp0"] = p.sp0;
                dr["pnl2"] = p.pnl2;
                dr["sp1"] = p.sp1;
                dt.Rows.Add(dr);
            }

            return dt;
        }

    }
}
