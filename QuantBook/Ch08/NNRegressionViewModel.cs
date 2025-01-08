using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System;
using QuantBook.Models.MachineLearning;
using Accord.Math;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using AForge.Neuro;
using Accord.Neuro;
using Accord.Neuro.Learning;

namespace QuantBook.Ch08
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class NNRegressionViewModel : Screen
    {
        private readonly IEventAggregator _events; 
        [ImportingConstructor]
        public NNRegressionViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "05. ANN: Regression";
            StartDate = new DateTime(2010, 1, 3);
            EndDate = new DateTime(2015, 12, 31);
            TrainStartDate = new DateTime(2010, 1, 3);
            TrainEndDate = new DateTime(2014, 12, 31);
                      
            LineSeriesCollectionError = new BindableCollection<Series>();
            LineSeriesCollection1 = new BindableCollection<Series>();
            LineSeriesCollection2 = new BindableCollection<Series>();
        }

        public BindableCollection<Series> LineSeriesCollectionError { get; set; }
        public BindableCollection<Series> LineSeriesCollection1 { get; set; }
        public BindableCollection<Series> LineSeriesCollection2 { get; set; }

        private double[] minMax = null;
        private string[] inputColumnNames = null;
        ActivationNetwork network = null;

        private string ticker = "IBM";
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

        private int windowSize = 2;
        public int WindowSize
        {
            get { return windowSize; }
            set
            {
                windowSize = value;
                NotifyOfPropertyChange(() => WindowSize);
            }
        }

        private int predictionSize = 1;
        public int PredictionSize
        {
            get { return predictionSize; }
            set
            {
                predictionSize = value;
                NotifyOfPropertyChange(() => PredictionSize);
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

        private bool isStop = false;
        public bool IsStop
        {
            get { return isStop; }
            set
            {
                isStop = value;
                NotifyOfPropertyChange(() => IsStop);
            }
        }

        private bool rprop = true;
        public bool RpropAlgorithm
        {
            get { return rprop; }
            set
            {
                rprop = value;
                NotifyOfPropertyChange(() => RpropAlgorithm);
            }
        }

        private bool lmAlgorithm = false;
        public bool LMAlgorithm
        {
            get { return lmAlgorithm; }
            set
            {
                lmAlgorithm = value;
                NotifyOfPropertyChange(() => LMAlgorithm);
            }
        }

        private double learningRate = 0.1;
        public double LearningRate
        {
            get { return learningRate; }
            set
            {
                learningRate = value;
                NotifyOfPropertyChange(() => LearningRate);
            }
        }

        private int iterations =1000;
        public int Iterations
        {
            get { return iterations; }
            set
            {
                iterations = value;
                NotifyOfPropertyChange(() => Iterations);
            }
        }

        private double relativeError = 1.0e-10;
        public double RelativeError
        {
            get { return relativeError; }
            set
            {
                relativeError = value;
                NotifyOfPropertyChange(() => RelativeError);
            }
        }

        private double alpha = 2.0;
        public double Alpha
        {
            get { return alpha; }
            set
            {
                alpha = value;
                NotifyOfPropertyChange(() => Alpha);
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

        private string saveFileName = "MyNetwork.ann";
        public string SaveFileName
        {
            get { return saveFileName; }
            set
            {
                saveFileName = value;
                NotifyOfPropertyChange(() => SaveFileName);
            }
        }

        private string loadFileName = "MyNetwork.ann";
        public string LoadFileName
        {
            get { return loadFileName; }
            set
            {
                loadFileName = value;
                NotifyOfPropertyChange(() => LoadFileName);
            }
        }


        public async void LoadData()
        {
            Table1 = new DataTable();
            Table2 = new DataTable();
            Table3 = new DataTable();
            Table4 = new DataTable();
            LineSeriesCollectionError.Clear();
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();

            minMax = new double[2];
            await Task.Run(() =>
                {
                    DataTable dt1 = MLHelper.GetStockData(Ticker, StartDate, EndDate);
                    DataTable dt2 = MLHelper.NNRegressionData(dt1, WindowSize, PredictionSize, out minMax, out inputColumnNames);                   
                    Table1 = dt1;
                    Table2 = dt2;
                   
                });
        }

        public async void StartNN()
        {
            LineSeriesCollectionError.Clear();
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();

            await Task.Run(() =>
            {               
                NnRegression();
            });

        }

        public void Stop()
        {
            IsStop = true;
        }



        DataTable dtTrain = new DataTable();
        DataTable dtPredict = new DataTable();
        double mseTrain = 0;
        double msePredict = 0;
        private void NnRegression()
        {
            IsStop = false;
            dtTrain = Table2.Clone();
            dtPredict = Table2.Clone();
            for (int i = WindowSize + PredictionSize -1; i < Table2.Rows.Count; i++)
            {
                DateTime date = Table2.Rows[i]["Date"].To<DateTime>();

                if (date >= TrainStartDate && date <= TrainEndDate)
                    dtTrain.ImportRow(Table2.Rows[i]);
                else if (date > TrainEndDate)
                    dtPredict.ImportRow(Table2.Rows[i]);
            }

            int samples = dtTrain.Rows.Count;
            double[][] inputTrain = dtTrain.DefaultView.ToTable(false, inputColumnNames).ToArray();
            double[][] outputTrain = dtTrain.DefaultView.ToTable(false, "Expected").ToArray();
            double[][] inputPredict = dtPredict.DefaultView.ToTable(false, inputColumnNames).ToArray();

            network = new ActivationNetwork(new BipolarSigmoidFunction(Alpha), 4 * WindowSize, 4 * WindowSize * 2, 1);
            NguyenWidrow win = new NguyenWidrow(network);
            win.Randomize();            
           
            var lm = new LevenbergMarquardtLearning(network);
            lm.LearningRate = LearningRate;
            var rprop = new ParallelResilientBackpropagationLearning(network);
            rprop.Reset(LearningRate);

            int iteration = 0;
            double error = Double.PositiveInfinity;
            LineSeriesCollectionError.Clear();
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Line;

            List<object> objList = new List<object>();
            objList.Add("Ready...");
            objList.Add(0);
            objList.Add(Iterations + 1);
            objList.Add(0);

            while (!IsStop)
            {
                double error0 = error;
                if (RpropAlgorithm)
                    error = rprop.RunEpoch(inputTrain, outputTrain) / samples;
                else
                    error = lm.RunEpoch(inputTrain, outputTrain) / samples;
                double relError = Math.Abs(error0 - error);

                Application.Current.Dispatcher.BeginInvoke(
                        (System.Action)delegate()
                        {
                            try
                            {
                                ds.Points.AddXY(iteration, error);
                                if (iteration % 10 == 0)
                                {
                                    LineSeriesCollectionError.Clear();
                                    LineSeriesCollectionError.Add(ds);
                                }
                            }
                            catch { }
                        });

                objList[0] = string.Format("Total Runs = {0}, i={1}", Iterations, iteration);
                objList[3] = iteration;
                _events.PublishOnUIThread(new QuantBook.Models.ModelEvents(objList));

                iteration++;
                if (iteration > Iterations || relError < RelativeError)
                    break;
            }

            objList[0] = "Ready...";
            objList[1] = 0;
            objList[2] = 1;
            objList[3] = 0;
            _events.PublishOnUIThread(new QuantBook.Models.ModelEvents(objList));

            for (int i = 0; i < samples; i++)
            {
                double predicted = network.Compute(inputTrain[i])[0];
                dtTrain.Rows[i]["Predicted"] = predicted;
                dtTrain.Rows[i]["ExpectedOrig"] = MLHelper.ConvertNNOutput(dtTrain.Rows[i]["Expected"].To<double>(), minMax);
                dtTrain.Rows[i]["PredictedOrig"] = MLHelper.ConvertNNOutput(predicted, minMax);
            }

            for (int i = 0; i < dtPredict.Rows.Count; i++)
            {
                double predicted = network.Compute(inputPredict[i])[0];
                dtPredict.Rows[i]["Predicted"] = predicted;
                dtPredict.Rows[i]["ExpectedOrig"] = MLHelper.ConvertNNOutput(dtPredict.Rows[i]["Expected"].To<double>(), minMax);
                dtPredict.Rows[i]["PredictedOrig"] = MLHelper.ConvertNNOutput(predicted, minMax);
            }

            mseTrain = MLHelper.GetRMSE(dtTrain, "ExpectedOrig", "PredictedOrig");
            msePredict = MLHelper.GetRMSE(dtPredict, "ExpectedOrig", "PredictedOrig");
            Table3 = dtTrain;
            Table4 = dtPredict;
            Table1 = QuantBook.Models.ModelHelper.CopyTable(dtPredict);
            AddCharts();
        }

        private void AddCharts()
        {
            Title1 = string.Format("{0}: Results for Training Data Set (RMSE = {1})", Ticker, Math.Round(mseTrain, 4));
            LineSeriesCollection1.Clear();
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.YValueMembers = "ExpectedOrig";
            ds.Name = "Expected";
            LineSeriesCollection1.Add(ds);

            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.YValueMembers = "PredictedOrig";
            ds.Name = "Predicted";
            LineSeriesCollection1.Add(ds);

            Title2 = string.Format("{0}: Results for Prediction Data Set (RMSE = {1})", Ticker, Math.Round(msePredict, 4));
            LineSeriesCollection2.Clear();
            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.YValueMembers = "ExpectedOrig";
            ds.Name = "Expected";
            LineSeriesCollection2.Add(ds);

            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.YValueMembers = "PredictedOrig";
            ds.Name = "Predicted";
            LineSeriesCollection2.Add(ds);
        }

        public void SaveANN()
        {
            SerializeANN ann = new SerializeANN();
            ann.Network = network;
            ann.TrainTable = dtTrain;
            ann.PredictionTable = dtPredict;
            ann.MinMax = minMax;
            ann.InputNames = inputColumnNames;
            MLHelper.SaveAnn(ann,SaveFileName);           
            MessageBox.Show("The trained nerual network is saved to a file successfully");
        }

        public void LoadANN()
        {
            LineSeriesCollectionError.Clear();
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();
            network = null;

            SerializeANN ann = MLHelper.LoadAnn(LoadFileName);

            double[][] inputTrain = ann.TrainTable.DefaultView.ToTable(false, ann.InputNames).ToArray();
            double[][] outputTrain = ann.TrainTable.DefaultView.ToTable(false, "Expected").ToArray();
            double[][] inputPredict =ann.PredictionTable.DefaultView.ToTable(false, ann.InputNames).ToArray();

            for (int i = 0; i < ann.TrainTable.Rows.Count; i++)
            {
                double predicted = ann.Network.Compute(inputTrain[i])[0];
                ann.TrainTable.Rows[i]["Predicted"] = predicted;
                ann.TrainTable.Rows[i]["ExpectedOrig"] = MLHelper.ConvertNNOutput(ann.TrainTable.Rows[i]["Expected"].To<double>(), ann.MinMax);
                ann.TrainTable.Rows[i]["PredictedOrig"] = MLHelper.ConvertNNOutput(predicted, ann.MinMax);
            }

            for (int i = 0; i < ann.PredictionTable.Rows.Count; i++)
            {
                double predicted = ann.Network.Compute(inputPredict[i])[0];
                ann.PredictionTable.Rows[i]["Predicted"] = predicted;
                ann.PredictionTable.Rows[i]["ExpectedOrig"] = MLHelper.ConvertNNOutput(ann.PredictionTable.Rows[i]["Expected"].To<double>(), ann.MinMax);
                ann.PredictionTable.Rows[i]["PredictedOrig"] = MLHelper.ConvertNNOutput(predicted, ann.MinMax);
            }

            mseTrain = MLHelper.GetRMSE(ann.TrainTable, "ExpectedOrig", "PredictedOrig");
            msePredict = MLHelper.GetRMSE(ann.PredictionTable, "ExpectedOrig", "PredictedOrig");
            Table3 = ann.TrainTable;
            Table4 = ann.PredictionTable;
            AddCharts();
        }

    }
}
