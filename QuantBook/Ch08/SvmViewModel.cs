using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System;
using QuantBook.Models.MachineLearning;
using Accord.Math;
using System.Threading.Tasks;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using System.Windows;
using System.Collections.Generic;

namespace QuantBook.Ch08
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class SvmViewModel : Screen
    {
        private readonly IEventAggregator _events; 
        [ImportingConstructor]
        public SvmViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "02. SVM";
            StartDate = new DateTime(2010, 1, 3);
            EndDate = new DateTime(2015, 12, 31);
            TrainStartDate = new DateTime(2010, 1, 3);
            TrainEndDate = new DateTime(2014, 12, 31);

            LineSeriesCollection1 = new BindableCollection<Series>();
            LineSeriesCollection2 = new BindableCollection<Series>();
        }

        public BindableCollection<Series> LineSeriesCollection1 { get; set; }
        public BindableCollection<Series> LineSeriesCollection2 { get; set; }

        private string ticker = "AMAT";
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
        public DateTime EndDate
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

        private Visibility chartVisibility = Visibility.Hidden;
        public Visibility ChartVisibility
        {
            get { return chartVisibility; }
            set
            {
                chartVisibility = value;
                NotifyOfPropertyChange(() => ChartVisibility);
            }
        }

        private Visibility tableVisibility = Visibility.Visible;
        public Visibility TableVisibility
        {
            get { return tableVisibility; }
            set
            {
                tableVisibility = value;
                NotifyOfPropertyChange(() => TableVisibility);
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

        public async void LoadDataClassification()
        {
            await Task.Run(() =>
                {
                    DataTable dt = MLHelper.StockDataClassification(Ticker, StartDate, EndDate);
                    Table1 = dt;

                    Table2 = new DataTable();
                    Table3 = new DataTable();
                    Table4 = new DataTable();
                    LineSeriesCollection1.Clear();
                    LineSeriesCollection2.Clear();
                });
        }

        public async void LoadDataRegression()
        {
            await Task.Run(() =>
            {
                DataTable dt = MLHelper.StockDataRegression(Ticker, StartDate, EndDate);
                Table1 = dt;

                Table2 = new DataTable();
                Table3 = new DataTable();
                Table4 = new DataTable();
                LineSeriesCollection1.Clear();
                LineSeriesCollection2.Clear();
            });
        }

        public async void StartSvmClassification()
        {
            TableVisibility = Visibility.Visible;
            ChartVisibility = Visibility.Hidden;
            DataTable dt = QuantBook.Models.ModelHelper.CopyTable(Table1);
            await Task.Run(() =>
            {
                double[] dd = SvmClassification(dt);
                string ss = string.Format("Accuracy for training = {0}, Accuracy for prediction = {1}", Math.Round(dd[0], 4), Math.Round(dd[1], 4));
                _events.PublishOnUIThread(new QuantBook.Models.ModelEvents(new List<object> { ss }));
            });
        }

        public async void StartSvmRegression()
        {
            TableVisibility = Visibility.Hidden;
            ChartVisibility = Visibility.Visible;
            DataTable dt = QuantBook.Models.ModelHelper.CopyTable(Table1);
            await Task.Run(() =>
                {
                    SvmRegression(dt);
                });
        }


        private double[] SvmClassification(DataTable dtData)
        {
            int[] outputTrain;
            DataTable dtInputTrain = new DataTable();
            double[][] inputTrain = MLHelper.StockDataMLClassification(dtData, TrainEndDate, true, out dtInputTrain, out outputTrain);

            int[] outputPredict;
            DataTable dtInputPredict = new DataTable();
            double[][] inputPredict = MLHelper.StockDataMLClassification(dtData, TrainEndDate, false, out dtInputPredict, out outputPredict);

            IKernel kernel = new Gaussian();
            var machine = new MulticlassSupportVectorMachine(inputs: 4, kernel: kernel, classes: 3);
            var teacher = new MulticlassSupportVectorLearning(machine, inputTrain, outputTrain);
            teacher.Algorithm = (svm, classInputs, classOutputs, i, j) => new SequentialMinimalOptimization(svm, classInputs, classOutputs);
            double error = teacher.Run();           

            for (int i = 0; i < dtInputTrain.Rows.Count; i++)
            {
                int res = machine.Compute(inputTrain[i]);
                dtInputTrain.Rows[i]["Predicted"] = res;
            }

            for (int i = 0; i < dtInputPredict.Rows.Count; i++)
            {
                int res = machine.Compute(inputPredict[i]);
                dtInputPredict.Rows[i]["Predicted"] = res;
            }

            int correct0 = 0;
            int total0 = 0;
            for (int i = 0; i < dtInputTrain.Rows.Count; i++)
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
            for (int i = 0; i < dtInputPredict.Rows.Count; i++)
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
            for (int i = 0; i < dtInputTrain.Rows.Count; i++)
            {
                trainExpected[i] = dtInputTrain.Rows[i]["Expected"].To<int>();
                trainPredicted[i] = dtInputTrain.Rows[i]["Predicted"].To<int>();
            }

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

            return new double[] { accuracy0, accuracy };
        }

        

        private void SvmRegression(DataTable dtData)
        {
            double[] outputTrain;
            DataTable dtInputTrain = new DataTable();
            double[][] inputTrain = MLHelper.StockDataMLRegression(dtData, TrainEndDate, true, out dtInputTrain, out outputTrain);

            double[] outputPredict;
            DataTable dtInputPredict = new DataTable();
            double[][] inputPredict = MLHelper.StockDataMLRegression(dtData, TrainEndDate, false, out dtInputPredict, out outputPredict);

            IKernel kernel = new Linear();
            var machine = new KernelSupportVectorMachine(kernel, inputs: 4);
            var learn = new SequentialMinimalOptimizationRegression(machine, inputTrain, outputTrain);
            double error = learn.Run();

            for (int i = 0; i < dtInputTrain.Rows.Count; i++)
            {
                double res = machine.Compute(inputTrain[i]);
                dtInputTrain.Rows[i]["Predicted"] = res;
            }

            for (int i = 0; i < dtInputPredict.Rows.Count; i++)
            {
                double res = machine.Compute(inputPredict[i]);
                dtInputPredict.Rows[i]["Predicted"] = res;
            }

            Table1 = dtInputTrain;
            Table2 = dtInputPredict;

            AddCharts();
        }


        private void AddCharts()
        {
            Title1 = string.Format("{0}: Training Sample", Ticker);
            Title2 = string.Format("{0}: Prediction Sample", Ticker);
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();

            Series ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.XValueType = ChartValueType.Date;
            ds.YValueMembers = "Expected";
            ds.Name = "Expected";
            LineSeriesCollection1.Add(ds);
            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.XValueType = ChartValueType.Date;
            ds.YValueMembers = "Predicted";
            ds.Name = "Predicted";
            LineSeriesCollection1.Add(ds);

            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.XValueType = ChartValueType.Date;
            ds.YValueMembers = "Expected";
            ds.Name = "Expected";
            LineSeriesCollection2.Add(ds);
            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.XValueType = ChartValueType.Date;
            ds.YValueMembers = "Predicted";
            ds.Name = "Predicted";
            LineSeriesCollection2.Add(ds);
        }
    }
}
