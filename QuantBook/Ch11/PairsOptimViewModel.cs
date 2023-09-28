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
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuantBook.Ch11
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class PairsOptimViewModel: Screen
    {
        private readonly IEventAggregator _events;

        [ImportingConstructor]  
        public PairsOptimViewModel(IEventAggregator events) 
        {
            this._events = events;
            DisplayName = "04. Pairs Optimization";
        }
        private DateTime startDate = new DateTime(2010, 1, 3);
        private DateTime endDate = new DateTime(2015,12,31);
        private string ticker1 = "QQQ";
        private string ticker2 = "SPY";
        private int correlationWindow = 100;
        private IEnumerable<PairTypeEnum> pairType;
        private PairTypeEnum selectedPairType;
        private double hedgeRatio = 1.0;
        private DataTable optimTable = new DataTable();
        private string title1;
        private string title2;
        private string title3;
        private string title4;
        private string title5;
        private string title6;
        private DataTable yearlyPnLTable = null;
        DataTable drawdownTable = new DataTable();
        double[] betas;

        #region Properties
        public BindableCollection<Series> LineSeries1 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries2 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries3 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries4 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries5 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeries6 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<PairSignalEntity> PairCollection { get; set; } = new BindableCollection<PairSignalEntity>();
        public BindableCollection<PairPnlEntity> PnlCollection { get; set; } = new BindableCollection<PairPnlEntity>();
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
        public DataTable OptimTable
        {
            get { return optimTable; }
            set { optimTable = value; NotifyOfPropertyChange(() => OptimTable); }
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
        #endregion

        public void GetData()
        {
            PnlCollection.Clear();
            OptimTable = new DataTable();
            LineSeries1.Clear();
            LineSeries2.Clear();
            LineSeries3.Clear();
            LineSeries4.Clear();
            LineSeries5.Clear();
            LineSeries6.Clear();
            var pair = SignalHelper.GetPairCorrelation(Ticker1, Ticker2, StartDate, EndDate, CorrelationWindow, out betas);
            AddPriceCharts(pair);
        }    

        public async void StartOptim()
        {
            await Task.Run(() => 
            {
                PnlCollection.Clear();
                OptimTable = new DataTable();
                LineSeries4.Clear();
                LineSeries5.Clear();
                LineSeries6.Clear();

                List<(string ticker1, string ticker2, int movingWindow, double signalIn, double signalOut, int numTrades, double pnl, double sharpe)> pairsTrading = OptimHelper.OptimPairsTrading(Ticker1, Ticker2, StartDate, EndDate, HedgeRatio, SelectedPairType, _events).ToList();
                var dt = new DataTable();
                dt.Columns.Add("Ticker1", typeof(string));
                dt.Columns.Add("Ticker2", typeof(string));
                dt.Columns.Add("MovingWindow", typeof(int));
                dt.Columns.Add("SignalIn", typeof(double));
                dt.Columns.Add("SignalOut", typeof(double));
                dt.Columns.Add("NumTrades", typeof(int));
                dt.Columns.Add("Pnl", typeof(double));
                dt.Columns.Add("Sharpe", typeof(double));
                foreach(var pair in pairsTrading)
                {
                    var row = dt.NewRow();
                    row["Ticker1"] = pair.ticker1;
                    row["Ticker2"] = pair.ticker2;
                    row["MovingWindow"] = pair.movingWindow;
                    row["SignalIn"] = pair.signalIn;
                    row["SignalOut"] = pair.signalOut;
                    row["NumTrades"] = pair.numTrades;
                    row["Pnl"] = pair.pnl;
                    row["Sharpe"] = pair.sharpe;
                    dt.Rows.Add(row);
                }
                ModelHelper.DatatableSort(dt, "Pnl DESC");
                OptimTable = dt;
            });
        }

        public async void SelectedCellChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    var signalPairs = SignalHelper.GetPairCorrelation(Ticker1, Ticker2, StartDate, EndDate, CorrelationWindow, out betas);
                    if(signalPairs == null || !signalPairs.Any())
                    {
                        throw new ArgumentNullException(nameof(signalPairs), $"GetPairCorrelation returned no results for {Ticker1},{Ticker2},Start={StartDate},End={EndDate},CorrelationWindow={CorrelationWindow}");
                    }

                    DataRowView view = (DataRowView) e.AddedCells.Last().Item;
                    int movingWindow = view["MovingWindow"].To<int>();
                    var signal = SignalHelper.GetPairSignal(SelectedPairType, signalPairs.ToArray(), movingWindow);
                    if (signal == null || !signal.Any())
                    {
                        throw new ArgumentNullException(nameof(signal), $"GetPairSignal returned no results for PairType:{SelectedPairType} MovingWindow: {movingWindow} SignalPairsLength: {signalPairs.ToArray().Length}");
                    }
                    PairCollection.Clear();
                    PairCollection.AddRange(signal);

                    double signalIn = view["SignalIn"].To<double>();
                    double signalOut = view["SignalOut"].To<double>();
                    (IEnumerable<PairPnlEntity> pairEntities, IEnumerable<PnlEntity> pnlEntities) = BacktestHelper.ComputePnLPair(signal.ToArray(), 10000, signalIn, signalOut, HedgeRatio);
                    PnlCollection.Clear();
                    PnlCollection.AddRange(pairEntities);

                    DataTable dt = new DataTable();
                    List<(string ticker, string year, int numTrades, double pnl, double sp0, double pnl2, double sp1)> pnls = BacktestHelper.GetYearlyPnl(new List<PnlEntity>(pnlEntities));
                    dt.Columns.Add("ticker", typeof(string));
                    dt.Columns.Add("year", typeof(string));
                    dt.Columns.Add("numTrades", typeof(int));
                    dt.Columns.Add("pnl", typeof(double));
                    dt.Columns.Add("sp0", typeof(double));
                    dt.Columns.Add("pnl2", typeof(double));
                    dt.Columns.Add("sp1", typeof(double));
                    foreach (var pnl in pnls)
                    {
                        var row = dt.NewRow();
                        row["ticker"] = pnl.ticker;
                        row["year"] = pnl.year;
                        row["numTrades"] = pnl.numTrades;
                        row["pnl"] = pnl.pnl;
                        row["sp0"] = pnl.sp0;
                        row["pnl2"] = pnl.pnl2;
                        row["sp1"] = pnl.sp1;
                        dt.Rows.Add(row);
                    }
                    YearlyPnlTable = dt;

                    AddSignalChart();
                    AddPnlCharts();
                }
                catch
                {
                    throw;
                }
            });
        }
        private void AddPriceCharts(IEnumerable<PairSignalEntity> pair)
        {
            Title1 = $"{Ticker1},{Ticker2}: Stock Price";
            Title2 = $"{Ticker1},{Ticker2}: Correlation (Correl_Avg={Math.Round(betas[2], 3)} Correl_All = {Math.Round(betas[3], 3)}";
            Title3 = $"{Ticker1},{Ticker2}: Beta (Beta_Avg={Math.Round(betas[0], 3)} Beta_All = {Math.Round(betas[1], 3)}";

            LineSeries1.Clear();
            LineSeries2.Clear();
            LineSeries3.Clear();
            var pairList = pair.ToList();

            var ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Name = $"{Ticker1} Price",
            };
            pairList.ForEach(p => ds.Points.AddXY(p.Date, p.Price1));
            LineSeries1.Add(ds);

            ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                Name = $"{Ticker2} Price",
                YAxisType = AxisType.Secondary
            };
            pairList.ForEach(p => ds.Points.AddXY(p.Date, p.Price2));
            LineSeries1.Add(ds);

            ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Name = $"{Ticker2} Correlation",
            };
            pairList.ForEach(p => ds.Points.AddXY(p.Date, p.Correlation));
            LineSeries2.Add(ds);

            ds = new Series()
            {
                ChartType = SeriesChartType.Line
            };
            pairList.ForEach(p => ds.Points.AddXY(p.Date, p.Beta));
            LineSeries3.Add(ds);
        } 
        private void AddSignalChart()
        {
            Title4 = $"{Ticker1},{Ticker2}: Signal";
            LineSeries4.Clear();
            var ds = new Series()
            {
                ChartType = SeriesChartType.Line,
            };
            PairCollection.ToList().ForEach(p => ds.Points.AddXY(p.Date, p.Signal));
            LineSeries4.Add(ds);            
        }
        private void AddPnlCharts()
        {
            double pnl = 0.0;
            double sharpe = 0.0;
            int numTrades = 0;
            Title5 = $"{Ticker1},{Ticker2}: P&L (Total PnL = {pnl}, Sharpe = {sharpe}, NumTrades={numTrades}";
            LineSeries5.Clear();
            Series ds = new Series()
            {
                ChartType= SeriesChartType.Line,    
                Color = System.Drawing.Color.Red,
                 Name = "PnL"
            };
            PnlCollection.ToList().ForEach(p => ds.Points.AddXY(p.Date, p.PnLCum1));
            LineSeries5.Add(ds);

            Title6 = $"{Ticker1},{Ticker2}: Drawdown";
            LineSeries6.Clear();
            ds = new Series()
            {
                ChartType = SeriesChartType.Line,
                Name = "Maxdrawdown",
                Color = System.Drawing.Color.Red                
            };
            PnlCollection.ToList().ForEach(p => ds.Points.AddXY(p.Date, p.PnLCum1));
            LineSeries6.Add(ds);
        }

    }
}
