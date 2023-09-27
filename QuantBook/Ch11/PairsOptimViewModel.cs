using Caliburn.Micro;
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

        private void AddPriceCharts(IEnumerable<PairSignalEntity> pair)
        {
            throw new NotImplementedException();
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
                var dt = OptimHelper.OptimPairsTrading(Ticker1, Ticker2, StartDate, EndDate, HedgeRatio, SelectedPairType, _events);
                OptimTable = dt;
            });
        }

        public async void SelectedCellChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                AddSignalChart();
                AddPnlCharts();
            });
        }

        private void AddPnlCharts()
        {
            throw new NotImplementedException();
        }

        private void AddSignalChart()
        {
            throw new NotImplementedException();
        }
    }
}
