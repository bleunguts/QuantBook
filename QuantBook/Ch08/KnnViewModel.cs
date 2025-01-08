using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System;
using QuantBook.Models.MachineLearning;
using Accord.Math;
using Accord.MachineLearning;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuantBook.Ch08
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class KnnViewModel : Screen
    {
        private readonly IEventAggregator _events; 
        [ImportingConstructor]
        public KnnViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "01. KNN Stocks";            
            StartDate = new DateTime(2015, 12, 27);
            EndDate = new DateTime(2016, 12, 27);
            TrainStartDate = new DateTime(2015, 12, 27);
            TrainEndDate = new DateTime(2016, 6, 1); // Train half data, leave half for testdata

            LineSeriesCollection1 = new BindableCollection<Series>();
            LineSeriesCollection2 = new BindableCollection<Series>();
        }

        public BindableCollection<Series> LineSeriesCollection1 { get; set; }
        public BindableCollection<Series> LineSeriesCollection2 { get; set; }

        private string ticker = "AAPL";
        public string Ticker
        {
            get { return ticker; }
            set
            {
                ticker = value;
                NotifyOfPropertyChange(() => Ticker);
            }
        }

        private DateTime startDate;
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                NotifyOfPropertyChange(() => StartDate);
            }
        }

        private DateTime endDate;
        public DateTime  EndDate
        {
            get { return endDate; }
            set
            {
                endDate = value;
                NotifyOfPropertyChange(() => EndDate);
            }
        }

        private DataTable table1 = new DataTable();
        public DataTable Table1
        {
            get { return table1; }
            set
            {
                table1 = value;
                NotifyOfPropertyChange(() => Table1);
            }
        }

        private DataTable table2 = new DataTable();
        public DataTable Table2
        {
            get { return table2; }
            set
            {
                table2 = value;
                NotifyOfPropertyChange(() => Table2);
            }
        }

        private DataTable table3 = new DataTable();
        public DataTable Table3
        {
            get { return table3; }
            set
            {
                table3 = value;
                NotifyOfPropertyChange(() => Table3);
            }
        }

        private DataTable table4 = new DataTable();
        public DataTable Table4
        {
            get { return table4; }
            set
            {
                table4 = value;
                NotifyOfPropertyChange(() => Table4);
            }
        }

        private DateTime trainStartDate;
        public DateTime TrainStartDate
        {
            get { return trainStartDate; }
            set
            {
                trainStartDate = value;
                NotifyOfPropertyChange(() => TrainStartDate);
            }
        }

        private DateTime trainEndDate;
        public DateTime TrainEndDate
        {
            get { return trainEndDate; }
            set
            {
                trainEndDate = value;
                NotifyOfPropertyChange(() => TrainEndDate);
            }
        }

        private string title1 = string.Empty;
        public string Title1
        {
            get { return title1; }
            set
            {
                title1 = value;
                NotifyOfPropertyChange(() => Title1);
            }
        }

        private string title2 = string.Empty;
        public string Title2
        {
            get { return title2; }
            set
            {
                title2 = value;
                NotifyOfPropertyChange(() => Title2);
            }
        }

        private int kNumber = 4;
        public int KNumber
        {
            get { return kNumber; }
            set
            {
                kNumber = value;
                NotifyOfPropertyChange(() => KNumber);
            }
        }




        public async void LoadData()
        {
            await Task.Run(() =>
                {
                    DataTable dt = MLHelper.StockDataClassification(Ticker, StartDate, EndDate);
                    Table1 = dt;

                    Table2 = new DataTable();
                    LineSeriesCollection1.Clear();
                    LineSeriesCollection2.Clear();
                });
        }

        public void StartKnn()
        {
            double[] dd = KnnClassification(Table1, KNumber, true);
            string ss = string.Format("Accuracy for training = {0}, Accuracy for prediction = {1}", Math.Round(dd[0], 4), Math.Round(dd[1], 4));
            _events.PublishOnUIThread(new QuantBook.Models.ModelEvents(new List<object> { ss }));
        }


        public async void StartKnnAll()
        {
            await Task.Run(() =>
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("K", typeof(int));
                    dt.Columns.Add("AccuracyTrain", typeof(double));
                    dt.Columns.Add("AccuracyPredict", typeof(double));
                    for (int k = 3; k < 30; k++)
                    {
                        bool isConfusionMatrix = false;
                        if (k == 4)
                            isConfusionMatrix = true;
                        double[] correct = KnnClassification(Table1, k, isConfusionMatrix);
                        dt.Rows.Add(k, correct[0], correct[1]);
                    }
                    Table2 = dt;
                    AddCharts();
                });
        }


        private double[] KnnClassification(DataTable dtData, int k, bool isConfusionMatrix)
        {
            int[] outputTrain;
            DataTable dtInputTrain = new DataTable();
            double[][] inputTrain = MLHelper.StockDataMLClassification(dtData, TrainEndDate, true, out dtInputTrain, out outputTrain);           

            int[] outputPredict;
            DataTable dtInputPredict = new DataTable();
            double[][] inputPredict = MLHelper.StockDataMLClassification(dtData, TrainEndDate, false, out dtInputPredict, out outputPredict);

            KNearestNeighbors kn = new KNearestNeighbors(k, 3, inputTrain, outputTrain);

            for (int i = 0; i < dtInputTrain.Rows.Count; i++)
            {
                int res = kn.Compute(inputTrain[i]);
                dtInputTrain.Rows[i]["Predicted"] = res;
            }

            for (int i = 0; i < dtInputPredict.Rows.Count; i++)
            {
                int res = kn.Compute(inputPredict[i]);
                dtInputPredict.Rows[i]["Predicted"] = res;
            }

            int correct0 = 0;
            int total0 = 0;
            for (int i = 0; i <dtInputTrain.Rows.Count; i++)
            {
                int i1 = dtInputTrain.Rows[i]["Expected"].To<int>();
                int i2 = dtInputTrain.Rows[i]["Predicted"].To<int>();
                if (i1 == i2)
                    correct0++;
                total0++;
            }
            double accuracy0 = 1.0 * correct0 / (1.0 * total0);

            int correct = 0;
            int total = 0;
            for (int i =0; i < dtInputPredict.Rows.Count; i++)
            {
                int i1 = dtInputPredict.Rows[i]["Expected"].To<int>();
                int i2 = dtInputPredict.Rows[i]["Predicted"].To<int>();
                if (i1 == i2)
                    correct++;
                total++;
            }
            double accuracy = 1.0 * correct / (1.0 * total);

            int[] trainExpected = new int[total0];
            int[] trainPredicted = new int[total0];
            for (int i = 0; i <dtInputTrain.Rows.Count; i++)
            {
                trainExpected[i] = dtInputTrain.Rows[i]["Expected"].To<int>();
                trainPredicted[i] = dtInputTrain.Rows[i]["Predicted"].To<int>();
            }

            if (isConfusionMatrix)
            {
                DataTable dtPredict = new DataTable();
                dtPredict.Columns.Add("Date", typeof(DateTime));
                dtPredict.Columns.Add("Expected", typeof(int));
                dtPredict.Columns.Add("Predicted", typeof(int));
                for (int i = 0; i < dtInputPredict.Rows.Count; i++)
                    dtPredict.Rows.Add(dtInputPredict.Rows[i]["Date"], dtInputPredict.Rows[i]["Expected"], dtInputPredict.Rows[i]["Predicted"]);
                Table2 = dtPredict;

                DataTable dtConfusion1 = MLHelper.GetConfusionMatrix(3, dtInputTrain);
                Table3 = dtConfusion1;
                DataTable dtConfusion2 = MLHelper.GetConfusionMatrix(3, dtInputPredict);
                Table4 = dtConfusion2;
            }

            return new double[] { accuracy0, accuracy };
        }

       
        private void AddCharts()
        {
            Title1 = string.Format("{0}: Close Price", Ticker);
            Title2 = string.Format("{0}: KNN Accuracy", Ticker);
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();

            Series ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.XValueType = ChartValueType.Date;
            ds.YValueMembers = "Close";
            LineSeriesCollection1.Add(ds);

            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "K";
            ds.YValueMembers = "AccuracyPredict";
            ds.MarkerSize = 6;
            ds.MarkerStyle = MarkerStyle.Diamond;
            ds.Name = "Prediction";
            LineSeriesCollection2.Add(ds);
            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "K";
            ds.YValueMembers = "AccuracyTrain";
            ds.MarkerSize = 6;
            ds.MarkerStyle = MarkerStyle.Diamond;
            ds.Name = "Training";
            ds.YAxisType = AxisType.Secondary;
            LineSeriesCollection2.Add(ds);         
        }

    }
}
