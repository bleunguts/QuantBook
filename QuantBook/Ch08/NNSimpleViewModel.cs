using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System;
using QuantBook.Models.MachineLearning;
using Accord.Math;
using System.Threading.Tasks;
using System.Windows;
using AForge.Neuro;
using Accord.Neuro;
using Accord.Neuro.Learning;

namespace QuantBook.Ch08
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class NNSimpleViewModel : Screen
    {
        public NNSimpleViewModel()
        {
            DisplayName = "04. ANN: Example";
            LineSeriesCollectionError = new BindableCollection<Series>();
            LineSeriesCollection1 = new BindableCollection<Series>();
        }
        public BindableCollection<Series> LineSeriesCollectionError { get; set; }
        public BindableCollection<Series> LineSeriesCollection1 { get; set; }

        private double[] minMax = null;
        private string[] inputColumnNames = null;

        private int windowSize = 5;
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

        private bool originalInput = true;
        public bool OriginalInput
        {
            get { return originalInput; }
            set
            {
                originalInput = value;
                NotifyOfPropertyChange(() => OriginalInput);
            }
        }

        private bool normalizedInput = false;
        public bool NormalizedInput
        {
            get { return normalizedInput; }
            set
            {
                normalizedInput = value;
                NotifyOfPropertyChange(() => NormalizedInput);
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

        private int iterations = 100;
        public int Iterations
        {
            get { return iterations; }
            set
            {
                iterations = value;
                NotifyOfPropertyChange(() => Iterations);
            }
        }

        private double relativeError = 1.0e-7;
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

        public void LoadData()
        {
            GetData();
        }

        public async void StartNN()
        {
            LineSeriesCollection1.Clear();
            if(Table1.Rows.Count<1)
            {
                System.Windows.MessageBox.Show("Please load Data first.");
                return;
            }

            await Task.Run(() =>
            {
                NNRegression();
            });

        }

        public void Stop()
        {
            IsStop = true;
        }

        private void GetData()
        {
            DataTable dtData = QuantBook.Models.ModelHelper.CsvToDatatable("RegressionTest.csv");
            dtData.Columns.Add("YNorm", typeof(double));
            double min = dtData.Compute("Min(Y)", "").To<double>();
            double max = dtData.Compute("Max(Y)", "").To<double>();
            minMax = new double[] { min, max };

            foreach(DataRow row in dtData.Rows)
            {
                row["YNorm"] = 2.0 * (row["Y"].To<double>() - min) / (max - min) - 1.0;
            }

            Table1 = dtData;

            DataTable dt = QuantBook.Models.ModelHelper.CopyTable(dtData);
            inputColumnNames = new string[WindowSize];
            for (int i = 0; i < WindowSize; i++)
            {
                inputColumnNames[i] = "Input" + (i + 1).ToString();
                dt.Columns.Add(inputColumnNames[i], typeof(double));
            }
            int n = dtData.Rows.Count;
            int inputSize = n - WindowSize - PredictionSize + 1;
            dt.Columns.Add("Expected", typeof(double));
            dt.Columns.Add("Predicted", typeof(double));
            dt.Columns.Add("PredictedOrig", typeof(double));

            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < WindowSize; j++)
                {
                    if (OriginalInput)
                        dt.Rows[i + WindowSize - 1 + PredictionSize]["Input" + (j + 1).ToString()] = dt.Rows[i + j]["Y"];
                    else
                        dt.Rows[i + WindowSize - 1 + PredictionSize]["Input" + (j + 1).ToString()] = dt.Rows[i + j]["YNorm"];
                }
                dt.Rows[i + WindowSize - 1 + PredictionSize]["Expected"] = dt.Rows[i + WindowSize - 1 + PredictionSize]["YNorm"];
            }

            Table2 = dt;
        }

        private void NNRegression()
        {
            IsStop = false;
            DataTable dt = QuantBook.Models.ModelHelper.CopyTable(Table2);
            DataRow[] rows = dt.Select("Expected is NOT NULL", "Date ASC");
            DataTable dt1 = dt.Clone();
            foreach (DataRow row in rows)
                dt1.ImportRow(row);

            int samples = rows.Length;
            double[][] input = dt1.DefaultView.ToTable(false, inputColumnNames).ToArray();
            double[][] output = dt1.DefaultView.ToTable(false, "Expected").ToArray();
           
            ActivationNetwork network = new ActivationNetwork(new BipolarSigmoidFunction(Alpha), WindowSize, WindowSize * 2, 1);
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

            while (!IsStop)
            {
                double error0 = error;
                if (RpropAlgorithm)
                    error = rprop.RunEpoch(input, output) / samples;
                else
                    error = lm.RunEpoch(input, output) / samples;
                double relError = Math.Abs(error0 - error);
                System.Threading.Thread.Sleep(100);

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

                iteration++;
                if (iteration > Iterations || relError < RelativeError)
                    break;
            }

            for (int i = 0; i < samples; i++)
            {
                double expected = network.Compute(input[i])[0];
                dt.Rows[i + WindowSize - 1 + PredictionSize]["Predicted"] = expected;
                dt.Rows[i + WindowSize - 1 + PredictionSize]["PredictedOrig"] = MLHelper.ConvertNNOutput(expected, minMax);
            }
            Table2 = dt;

            AddChart();
        }

       

        private void AddChart()
        {
            LineSeriesCollection1.Clear();
            Series ds = new Series();
            ds.ChartType = SeriesChartType.Point;
            ds.XValueMember = "Date";
            ds.YValueMembers = "Y";
            ds.Name = "Expected";
            LineSeriesCollection1.Add(ds);

            ds = new Series();
            ds.ChartType = SeriesChartType.Line;
            ds.XValueMember = "Date";
            ds.YValueMembers = "PredictedOrig";
            ds.Name = "Predicted";
            LineSeriesCollection1.Add(ds);
        }

    }
}
