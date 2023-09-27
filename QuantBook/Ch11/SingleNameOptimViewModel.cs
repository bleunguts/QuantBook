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
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuantBook.Ch11
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class SingleNameOptimViewModel : Screen
    {
        private readonly IEventAggregator events;

        [ImportingConstructor]
        public SingleNameOptimViewModel(IEventAggregator events)
        {
            this.events = events;
            DisplayName = "02. Optimization";                                    
        }

        private string ticker = "IBM";
        private DateTime startDate = new DateTime(2010, 1, 3);
        private DateTime endDate = new DateTime(2015, 12, 31);
        private IEnumerable<PriceTypeEnum> priceType;
        private PriceTypeEnum selectedPriceType;
        private IEnumerable<SignalTypeEnum> signalType;
        private SignalTypeEnum selectedSignalType;
        private IEnumerable<StrategyTypeEnum> strategyType;
        private StrategyTypeEnum selectedStrategyType;
        private string title1 = string.Empty;
        private string title2 = string.Empty;
        private string yLabel1;
        private string yLabel2;
        private DataTable yearlyPnLTable;
        private DataTable optimTable;
        private bool isReinvest = false;
        DataTable drawdownTable = new DataTable();

        #region properties
        public BindableCollection<Series> LineSeriesCollection1 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<Series> LineSeriesCollection2 { get; set; } = new BindableCollection<Series>();
        public BindableCollection<SignalEntity> SignalCollection { get; set; } = new BindableCollection<SignalEntity>();            
        public BindableCollection<PnlEntity> PnLCollection { get; set; } = new BindableCollection<PnlEntity>();

        public string Ticker
        {
            get { return ticker; }
            set { ticker = value; NotifyOfPropertyChange(() => Ticker); }
        }

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value;  NotifyOfPropertyChange(() => StartDate); }
        }
        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; NotifyOfPropertyChange(() => EndDate); }
        }
        public IEnumerable<PriceTypeEnum> PriceType
        {
            get { return Enum.GetValues(typeof(PriceTypeEnum)).Cast<PriceTypeEnum>(); }
            set { priceType = value; NotifyOfPropertyChange(() => PriceType); }
        }
        public PriceTypeEnum SelectedPriceType
        {
            get { return selectedPriceType; }
            set { selectedPriceType = value; NotifyOfPropertyChange(() => SelectedPriceType); }
        }
        public IEnumerable<SignalTypeEnum> SignalType
        {
            get { return Enum.GetValues(typeof(SignalTypeEnum)).Cast<SignalTypeEnum>(); }
            set { signalType = value; NotifyOfPropertyChange(() => SignalType); }
        }
        public SignalTypeEnum SelectedSignalType
        {
            get { return selectedSignalType; }
            set { selectedSignalType = value; NotifyOfPropertyChange(() => SelectedSignalType); }
        }
        public IEnumerable<StrategyTypeEnum> StrategyType
        {
            get { return Enum.GetValues(typeof(StrategyTypeEnum)).Cast<StrategyTypeEnum>(); }
            set { strategyType = value; NotifyOfPropertyChange(() => StrategyType); }
        }
        public StrategyTypeEnum SelectedStrategyType
        {
            get { return selectedStrategyType; }
            set { selectedStrategyType = value; NotifyOfPropertyChange(() => SelectedStrategyType); }
        }
        public bool IsReinvest
        {
            get { return isReinvest; }
            set { isReinvest = value; NotifyOfPropertyChange(() => IsReinvest); }
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
        public string YLabel1
        {
            get { return yLabel1; }
            set { yLabel1 = value; NotifyOfPropertyChange(() => YLabel1); }
        }

        public string YLabel2
        {
            get { return yLabel2; }
            set { yLabel2 = value; NotifyOfPropertyChange(() => YLabel2); }
        }
        public DataTable YearlyPnLTable
        {
            get { return yearlyPnLTable; }
            set { yearlyPnLTable = value; NotifyOfPropertyChange(() => YearlyPnLTable); }
        }
        public DataTable OptimTable
        {
            get { return optimTable; }
            set { optimTable = value; NotifyOfPropertyChange(() => OptimTable); }
        }
        #endregion 

        public async void StartOptim()
        {
            await Task.Run(async () =>
            {
                PnLCollection.Clear();
                LineSeriesCollection1.Clear();
                LineSeriesCollection2.Clear();
                YearlyPnLTable = new DataTable();
                OptimTable = new DataTable();
                var data = await SignalHelper.GetStockData(Ticker, StartDate, EndDate, SelectedPriceType);
                var dt = new DataTable();
                MakeHeaders(dt);
                FillRows(dt, OptimHelper.OptimSingleName(data, SelectedSignalType, SelectedStrategyType, IsReinvest, modelEvents => events.PublishOnUIThread(modelEvents)));
                ModelHelper.DatatableSort(dt, "PnL DESC");
                OptimTable = dt;
            });

            void MakeHeaders(DataTable table)
            {
                table.Columns.AddRange(new[]
                                {
                    new DataColumn("Ticker", typeof(string)),
                    new DataColumn("MovingWindow", typeof(int)),
                    new DataColumn("SignalIn", typeof(double)),
                    new DataColumn("SignalOut", typeof(double)),
                    new DataColumn("NumTrades", typeof(int)),
                    new DataColumn("PnL", typeof(double)),
                    new DataColumn("Sharpe", typeof(double)),
                });
            }
            void FillRows(DataTable table, List<(string ticker, int bar, double zin, double zout, int numTrades, double pnlCum, double sharpe)> rows)
            {
                foreach(var row in rows)
                {
                    table.Rows.Add(row.ticker, row.bar, row.zin, row.zout, row.numTrades, row.pnlCum, row.sharpe);
                }
            }
        }       
        public async void SelectedCellChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            await Task.Run(async () =>
            {
                var selectedCells = e.AddedCells;
                DataRowView row = (DataRowView) selectedCells[selectedCells.Count - 1].Item;
                int movingWindow = row[1].To<int>();
                double signalIn = row[2].To<double>();
                double signalOut = row[3].To<double>();

                var data = await SignalHelper.GetStockData(Ticker, StartDate, EndDate, SelectedPriceType);
                var signal = SignalHelper.GetSignal(data, movingWindow, SelectedSignalType);
                SignalCollection.Clear();
                SignalCollection.AddRange(signal);
                var pnls = BacktestHelper.ComputeLongShortPnl(signal, 10_000.0, signalIn, signalOut, SelectedStrategyType, IsReinvest).ToList();
                PnLCollection.Clear();
                PnLCollection.AddRange(pnls);

                DataTable dt = new DataTable();
                dt.Columns.AddRange(new[]
                {
                new DataColumn("Ticker",typeof(string)),
                new DataColumn("Period",typeof(string)),
                new DataColumn("NumTrades",typeof(string)),
                new DataColumn("PnL",typeof(string)),
                new DataColumn("Sharpe",typeof(string)),
                new DataColumn("PnLHold",typeof(string)),
                new DataColumn("SharpeHold",typeof(string)),
                });
                foreach (var r in BacktestHelper.GetYearlyPnl(PnLCollection.ToList()))
                {
                    dt.Rows.Add(r.ticker, r.year, r.numTrades, r.pnl, r.sp0, r.pnl2, r.sp1);
                }
                YearlyPnLTable = dt;

                DataTable dt2 = new DataTable();
                dt2.Columns.AddRange(new[]
                {
                new DataColumn("Date", typeof(DateTime)),
                new DataColumn("PnL", typeof(double)),
                new DataColumn("Drawdown", typeof(double)),
                new DataColumn("MaxDrawdown", typeof(double)),
                new DataColumn("DrawUp", typeof(double)),
                new DataColumn("MaxDrawUp", typeof(double)),
                new DataColumn("PnLHold", typeof(double)),
                new DataColumn("DrawDownHold", typeof(double)),
                new DataColumn("MaxDrawDownHold", typeof(double)),
                new DataColumn("DrawupHold", typeof(double)),
                new DataColumn("MaxDrawupHold", typeof(double)),
                 });
                var results = BacktestHelper.GetDrawDown(PnLCollection.ToList(), 10_000.0);
                double maxDrawdown = results.Max(r => r.drawdown);
                double maxDrawup = results.Max(r => r.drawup);
                double maxDrawdownHold = results.Max(r => r.drawdownHold);
                double maxDrawupHold = results.Max(r => r.drawupHold);
                foreach (DrawDownResult r in results)
                {
                    dt2.Rows.Add(r.date, r.pnl, r.drawdown, maxDrawdown, r.drawup, maxDrawup, r.pnlHold, r.drawdownHold, maxDrawdownHold, r.drawupHold, maxDrawupHold);
                }

                drawdownTable = dt2;             

                AddPnLCharts();
            });
        }
        public void PlotPnL()
        {
            AddPnLCharts();
            Drawdown(true);
        }
        public void PlotPrice() => AddSignalCharts();
        public void DrawdownStrategy() => Drawdown(true);
        public void DrawdownHold() => Drawdown(false);
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
            foreach (var p in SignalCollection)
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
            foreach (var p in SignalCollection)
            {
                ds.Points.AddXY(p.Date, p.Signal);
            }
            LineSeriesCollection2.Add(ds);
        }
        private void AddPnLCharts()
        {
            DataRow latestPnlRow = YearlyPnLTable.Rows[YearlyPnLTable.Rows.Count - 1];
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
            foreach (var p in PnLCollection)
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
            foreach (var p in PnLCollection)
            {
                ds.Points.AddXY(p.Date, p.PnLCum);
            }
            LineSeriesCollection1.Add(ds);
            Drawdown(true);
        }
        private void Drawdown(bool isStrategy)
        {
            YLabel2 = "Drawdown (%)";
            Series ds = new Series();
            if (isStrategy)
            {
                Title2 = string.Format("{0}: Drawdown for Signal Type = {1}", Ticker, SelectedSignalType);
                LineSeriesCollection2.Clear();
                ds = new Series
                {
                    ChartType = SeriesChartType.Line,
                    Name = "Drawdown"
                };
                foreach (DataRow row in drawdownTable.Rows)
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
                foreach (DataRow row in drawdownTable.Rows)
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
                foreach (DataRow row in drawdownTable.Rows)
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
                foreach (DataRow row in drawdownTable.Rows)
                {
                    ds.Points.AddXY(row["Date"], row["MaxDrawdownHold"]);
                }
                LineSeriesCollection2.Add(ds);
            }
        }

    }
}
