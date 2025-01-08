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
    public class NNClassificationViewModel : Screen
    {
        private readonly IEventAggregator _events; 
        [ImportingConstructor]
        public NNClassificationViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "03. ANN: Classification";
            StartDate = new DateTime(2015, 12, 27);
            EndDate = new DateTime(2016, 12, 27);
            TrainStartDate = new DateTime(2015, 12, 27);
            TrainEndDate = new DateTime(2016, 6, 1); // Train half data, leave half for testdata

            LineSeriesCollectionError = new BindableCollection<Series>();
        }

        public BindableCollection<Series> LineSeriesCollectionError { get; set; }

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

        private bool rprop = false;
        public bool RpropAlgorithm
        {
            get { return rprop; }
            set
            {
                rprop = value;
                NotifyOfPropertyChange(() => RpropAlgorithm);
            }
        }

        private bool lmAlgorithm = true;
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

        private int iterations = 500;
        public int Iterations
        {
            get { return iterations; }
            set
            {
                iterations = value;
                NotifyOfPropertyChange(() => Iterations);
            }
        }

        private double relativeError = 1.0e-8;
        public double RelativeError
        {
            get { return relativeError; }
            set
            {
                relativeError = value;
                NotifyOfPropertyChange(() => RelativeError);
            }
        }

        private int hiddenNeurons = 30;
        public int HiddenNeurons
        {
            get { return hiddenNeurons; }
            set
            {
                hiddenNeurons = value;
                NotifyOfPropertyChange(() => HiddenNeurons);
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

        private int numClasses = 3;
        public int NumClasses
        {
            get { return numClasses; }
            set
            {
                numClasses = value;
                NotifyOfPropertyChange(() => NumClasses);
            }
        }


        public async void LoadData()
        {
            Table2 = new DataTable();
            Table3 = new DataTable();
            Table4 = new DataTable();
            LineSeriesCollectionError.Clear();

            await Task.Run(() =>
                {
                    DataTable dt = MLHelper.StockDataClassification(Ticker, StartDate, EndDate);
                    Table1 = dt;                   
                });
        }

        public async void StartNN()
        {
            DataTable dt = QuantBook.Models.ModelHelper.CopyTable(Table1);
            _events.PublishOnUIThread(new QuantBook.Models.ModelEvents(new List<object> { string.Empty }));
            Table2 = new DataTable();
            Table3 = new DataTable();
            Table4 = new DataTable();
            await Task.Run(() =>
            {
                double[] dd;
                if (NumClasses == 2)
                    dd = NnClassification2(dt);
                else
                    dd = NnClassification3(dt);
                
                string ss = string.Format("Accuracy for training = {0}, Accuracy for prediction = {1}", Math.Round(dd[0], 4), Math.Round(dd[1], 4));
                _events.PublishOnUIThread(new QuantBook.Models.ModelEvents(new List<object> { ss }));
            });
        }

        public void Stop()
        {
            IsStop = true;
        }


        private double[] NnClassification3(DataTable dtData)
        {
            IsStop = false;
            int[] outputTrain;
            DataTable dtInputTrain = new DataTable();
            double[][] inputTrain = MLHelper.StockDataMLClassification(dtData, TrainEndDate, true, out dtInputTrain, out outputTrain);

            int[] outputPredict;
            DataTable dtInputPredict = new DataTable();
            double[][] inputPredict = MLHelper.StockDataMLClassification(dtData, TrainEndDate, false, out dtInputPredict, out outputPredict);

            double[][] outputs = Accord.Statistics.Tools.Expand(outputTrain, 3, -1, 1);
            var function = new BipolarSigmoidFunction(Alpha);
            var nn = new ActivationNetwork(function, 4, HiddenNeurons, 3);

            NguyenWidrow win = new NguyenWidrow(nn);
            win.Randomize();

            var lm = new LevenbergMarquardtLearning(nn);
            lm.LearningRate = LearningRate;
            var rprop = new ParallelResilientBackpropagationLearning(nn);

            double error = Double.PositiveInfinity;
            int samples = dtInputTrain.Rows.Count;
            int count = 0;
            LineSeriesCollectionError.Clear();
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Line;

          
            while (true)
            {
                double error0 = error;
                if (RpropAlgorithm)
                    error = rprop.RunEpoch(inputTrain, outputs) / samples;
                else
                    error = lm.RunEpoch(inputTrain, outputs) / samples;

                double relError = Math.Abs(error0 - error);

                Application.Current.Dispatcher.BeginInvoke(
                        (System.Action)delegate()
                        {
                            try
                            {
                                ds.Points.AddXY(count,error);
                                if (count % 10 == 0)
                                {
                                    LineSeriesCollectionError.Clear();
                                    LineSeriesCollectionError.Add(ds);
                                }
                            }
                            catch { }
                        });
                count++;
                if (count > Iterations || IsStop || relError < RelativeError)
                    break;
            }

            for (int i = 0; i < dtInputTrain.Rows.Count; i++)
            {
                int res;
                double[] ot = nn.Compute(inputTrain[i]);
                double response = ot.Max(out res);
                dtInputTrain.Rows[i]["Predicted"] = res;
            }

            for (int i = 0; i < dtInputPredict.Rows.Count; i++)
            {
                int res;
                double[] ot = nn.Compute(inputPredict[i]);
                double response = ot.Max(out res);
                dtInputTrain.Rows[i]["Predicted"] = res;
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

        private double[] NnClassification2(DataTable dtData1)
        {
            DataTable dtData = QuantBook.Models.ModelHelper.CopyTable(dtData1);
            foreach (DataRow r in dtData.Rows)
            {
                int value = r["Expected"].To<int>();
                if (value < 2)
                    r["Expected"] = 1;
                else
                    r["Expected"] = -1;
            }

            IsStop = false;
            int[] outputTrain1;
            DataTable dtInputTrain = new DataTable();
            double[][] inputTrain = MLHelper.StockDataMLClassification(dtData, TrainEndDate, true, out dtInputTrain, out outputTrain1);

            int[] outputPredict1;
            DataTable dtInputPredict = new DataTable();
            double[][] inputPredict = MLHelper.StockDataMLClassification(dtData, TrainEndDate, false, out dtInputPredict, out outputPredict1);

            double[][] outputTrain = (QuantBook.Models.ModelHelper.ArrayIntToDouble(outputTrain1)).Transpose().ToArray();
            double[][] outputPredict = (QuantBook.Models.ModelHelper.ArrayIntToDouble(outputPredict1)).Transpose().ToArray();

            var nn = new ActivationNetwork(new BipolarSigmoidFunction(Alpha), 4, HiddenNeurons, 1);
            NguyenWidrow win = new NguyenWidrow(nn);
            win.Randomize();

            var lm = new LevenbergMarquardtLearning(nn);
            lm.LearningRate = LearningRate;
            var rprop = new ParallelResilientBackpropagationLearning(nn);

            double error = Double.PositiveInfinity;
            int samples = dtInputTrain.Rows.Count;
            int count = 0;
            LineSeriesCollectionError.Clear();
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Line;

            while (true)
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
                                ds.Points.AddXY(count, error);
                                if (count % 20 == 0)
                                {
                                    LineSeriesCollectionError.Clear();
                                    LineSeriesCollectionError.Add(ds);
                                }
                            }
                            catch { }
                        });
                count++;
                if (count > Iterations || IsStop || relError < RelativeError)
                    break;
            }

            for (int i = 0; i < dtInputTrain.Rows.Count; i++)
            {
                int res = Math.Sign(nn.Compute(inputTrain[i])[0]);
                dtInputTrain.Rows[i]["Predicted"] = res;
            }

            for (int i = 0; i < dtInputPredict.Rows.Count; i++)
            {
                int res = Math.Sign(nn.Compute(inputPredict[i])[0]);
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

            DataTable dtConfusion1 = MLHelper.GetConfusionMatrix(dtInputTrain);
            Table3 = dtConfusion1;
            DataTable dtConfusion2 = MLHelper.GetConfusionMatrix(dtInputPredict);
            Table4 = dtConfusion2;

            return new double[] { accuracy0, accuracy };
        }










        /*private double[] NnClassificationTest()
        {
            IsStop = false;
            DataTable dtData = QuantBook.Models.ModelHelper.CsvToDatatable("ClassificationTest.csv");
            Table1 = dtData;

            string[] sourceColumns;
            double[,] sourceMatrix = dtData.ToMatrix(out sourceColumns);
            int samples = sourceMatrix.GetLength(0);
            double[][] inputs = sourceMatrix.Submatrix(null, 0, 1).ToArray();
            double[][] outputs = sourceMatrix.GetColumn(2).Transpose().ToArray();

            double[] a1 = new double[sourceMatrix.GetLength(0)];
            for (int i = 0; i < a1.Length; i++)
                a1[i] = (double)sourceMatrix[i, 2];

            double[][] aaa = a1.Transpose().ToArray();
            
            var nn = new ActivationNetwork(new BipolarSigmoidFunction(Alpha), 2, HiddenNeurons, 1);           
            NguyenWidrow win = new NguyenWidrow(nn);
            win.Randomize();            

            LevenbergMarquardtLearning teacher = new LevenbergMarquardtLearning(nn);
            teacher.LearningRate = LearningRate;
            
            double error = Double.PositiveInfinity;
            int count = 0;
            LineSeriesCollectionError.Clear();
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Line;

            List<object> objList = new List<object>();
            objList.Add("Ready...");
            objList.Add(0);
            objList.Add(Iterations);
            objList.Add(0);

            while (true)
            {
                error = teacher.RunEpoch(inputs, aaa) / samples;
                Application.Current.Dispatcher.BeginInvoke(
                        (System.Action)delegate()
                        {
                            try
                            {
                                ds.Points.AddXY(count, error);
                                if (count % 25 == 0)
                                {
                                    LineSeriesCollectionError.Clear();
                                    LineSeriesCollectionError.Add(ds);
                                }
                            }
                            catch { }
                        });
                count++;
                if (count > Iterations || IsStop)
                    break;
            }

            DataTable dt = QuantBook.Models.ModelHelper.CopyTable(dtData);
            dt.Columns["G"].ColumnName = "Expected";
            dt.Columns.Add("Predicted", typeof(int));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Predicted"] = System.Math.Sign(nn.Compute(inputs[i])[0]);


            int correct0 = 0;
            int total0 = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int i1 = dt.Rows[i]["Expected"].To<int>();
                int i2 = dt.Rows[i]["Predicted"].To<int>();
                if (i1 == i2)
                    correct0++;
                total0++;
            }
            double accuracy0 = 1.0 * correct0 / (1.0 * total0);

        
            Table2 = dt;

            DataTable dtConfusion1 = MLHelper.GetConfusionMatrix(dt);
            Table3 = dtConfusion1;

            return new double[] { accuracy0, 0 };
        }*/

    }
}
