using System;
using QuantBook.Models.DataModel;
using System.Data;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Statistics.Analysis;
using AForge.Neuro;
using Accord.Neuro;
using Accord.Neuro.Learning;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace QuantBook.Models.MachineLearning
{
    public static class MLHelper
    {
        public static DataTable StockDataClassification(string ticker, DateTime startDate, DateTime endDate)
        {
            var data = Dal.GetStockData(ticker, startDate, endDate, DataSourceEnum.Random);
            DataTable res = new DataTable();
            res.Columns.Add("Date", typeof(DateTime));
            res.Columns.Add("Open", typeof(double));
            res.Columns.Add("High", typeof(double));
            res.Columns.Add("Low", typeof(double));
            res.Columns.Add("Close", typeof(double));
            res.Columns.Add("Expected", typeof(int));
            res.Columns.Add("Predicted", typeof(int));

            foreach (var p in data)
                res.Rows.Add(p.Date, p.AdjOpen, p.AdjHigh, p.AdjLow, p.AdjClose);

            for (int i = 1; i < res.Rows.Count; i++)
            {
                double p0 = res.Rows[i - 1]["Close"].To<double>();
                double p1 = res.Rows[i]["Close"].To<double>();
                int cl;
                if (p1 > p0)
                    cl = 1;
                else if (p1 == p0)
                    cl = 0;
                else
                    cl = 2;
                res.Rows[i - 1]["Expected"] = cl;
            }
            res.Rows.RemoveAt(res.Rows.Count - 1);
            return res;
        }

        public static DataTable GetStockData(string ticker, DateTime startDate, DateTime endDate)
        {
            var data = Dal.GetStockData(ticker, startDate, endDate, DataSourceEnum.Random);
            DataTable res = new DataTable();
            res.Columns.Add("Date", typeof(DateTime));
            res.Columns.Add("Open", typeof(double));
            res.Columns.Add("High", typeof(double));
            res.Columns.Add("Low", typeof(double));
            res.Columns.Add("Close", typeof(double));
           
            foreach (var p in data)
                res.Rows.Add(p.Date, p.AdjOpen, p.AdjHigh, p.AdjLow, p.AdjClose);
            return res;
        }

        private static DataTable ProcessNNData(DataTable dtData, out double[] minMax)
        {
            DataTable res = QuantBook.Models.ModelHelper.CopyTable(dtData);
            double[] min = new double[4];
            double[] max = new double[4];
            for (int i = 1; i < 5; i++)
            {
                min[i - 1] = res.Compute("Min(" + res.Columns[i].ColumnName + ")", "").To<double>();
                max[i - 1] = res.Compute("Max(" + res.Columns[i].ColumnName + ")", "").To<double>();
            }

            foreach (DataRow row in res.Rows)
            {
                for (int i = 1; i < 5; i++)
                    row[i] = 2.0 * (row[i].To<double>() - min[i - 1]) / (max[i - 1] - min[i - 1]) - 1.0;
            }

            minMax = new[] { min[3], max[3] };
            return res;
        }

        public static DataTable NNRegressionData(DataTable dtData, int windowSize, int predictionSize, out double[] minMax, out string[] inputColumnNames)
        {
            DataTable dt = ProcessNNData(dtData, out minMax);

            inputColumnNames = new string[4 * windowSize];
            for (int i = 0; i < windowSize; i++)
            {
                inputColumnNames[4 * i] = "Open" + (i + 1).ToString();
                inputColumnNames[4 * i + 1] = "High" + (i + 1).ToString();
                inputColumnNames[4 * i + 2] = "Low" + (i + 1).ToString();
                inputColumnNames[4 * i + 3] = "Close" + (i + 1).ToString();
            }
            foreach (string ss in inputColumnNames)
                dt.Columns.Add(ss, typeof(double));

            dt.Columns.Add("Expected", typeof(double));
            dt.Columns.Add("Predicted", typeof(double));
            dt.Columns.Add("ExpectedOrig", typeof(double));
            dt.Columns.Add("PredictedOrig", typeof(double));
            int inputSize = dtData.Rows.Count - windowSize - predictionSize + 1;


            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < windowSize; j++)
                {
                    dt.Rows[i + windowSize - 1 + predictionSize][inputColumnNames[4 * j]] = dt.Rows[i + j]["Open"];
                    dt.Rows[i + windowSize - 1 + predictionSize][inputColumnNames[4 * j + 1]] = dt.Rows[i + j]["High"];
                    dt.Rows[i + windowSize - 1 + predictionSize][inputColumnNames[4 * j + 2]] = dt.Rows[i + j]["Low"];
                    dt.Rows[i + windowSize - 1 + predictionSize][inputColumnNames[4 * j + 3]] = dt.Rows[i + j]["Close"];
                }
                dt.Rows[i + windowSize - 1 + predictionSize]["Expected"] =  dt.Rows[i + windowSize - 1 + predictionSize]["Close"];
            }

                return dt;
        }

       

        public static double ConvertNNOutput(double pricePredicted, double[] minMax)
        {
            return minMax[0] + 0.5 * (pricePredicted + 1.0) * (minMax[1] - minMax[0]);
        }

        public static double GetRMSE(DataTable dt, string colExpected, string colPredicted)
        {
            double avg = dt.Compute("Avg(" + colExpected + ")", "").To<double>();
            double mse = 0;
            foreach(DataRow row in dt.Rows)
            {
                double expected = row[colExpected].To<double>();
                double predicted = row[colPredicted].To<double>();
                mse += (expected - predicted) * (expected - predicted);
            }

            return Math.Sqrt(mse / dt.Rows.Count) / avg;
        }

        public static DataTable StockDataRegression(string ticker, DateTime startDate, DateTime endDate)
        {
            var data = Dal.GetStockData(ticker, startDate, endDate, DataSourceEnum.Random);
            DataTable res = new DataTable();
            res.Columns.Add("Date", typeof(DateTime));
            res.Columns.Add("Open", typeof(double));
            res.Columns.Add("High", typeof(double));
            res.Columns.Add("Low", typeof(double));
            res.Columns.Add("Close", typeof(double));
            res.Columns.Add("Expected", typeof(double));
            res.Columns.Add("Predicted", typeof(double));

            foreach (var p in data)
                res.Rows.Add(p.Date, p.AdjOpen, p.AdjHigh, p.AdjLow, p.AdjClose);

            for (int i = 1; i < res.Rows.Count; i++)
            {
                res.Rows[i - 1]["Expected"] = res.Rows[i]["Close"];
            }

            res.Rows.RemoveAt(res.Rows.Count - 1);
            return res;
        }

        public static double[][] StockDataMLClassification(DataTable dtData, DateTime trainEndDate, bool isTraining, out DataTable dtInput, out int[] outputs)
        {
            int num = 0;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (Convert.ToDateTime(dtData.Rows[i]["Date"]) <= trainEndDate)
                    num = i;
            }

            DataTable dt1 = dtData.Clone();
            DataTable dt2 = dtData.Clone();

            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (i <= num)
                    dt1.ImportRow(dtData.Rows[i]);
                else
                    dt2.ImportRow(dtData.Rows[i]);
            }

            dtInput = new DataTable();
            if (isTraining)
                dtInput = dt1;
            else
                dtInput = dt2;

            string[] names = new string[4];
            for (int i = 1; i <= 4; i++)
                names[i - 1] = dtInput.Columns[i].ColumnName;
            DataTable dtb = dtInput.DefaultView.ToTable(false, names);

            double[][] inputs = dtb.ToArray();
            outputs = new int[dtb.Rows.Count];
            for (int i = 0; i < dtInput.Rows.Count; i++)
                outputs[i] = dtInput.Rows[i]["Expected"].To<int>();

            return inputs;
        }

        public static double[][] StockDataMLRegression(DataTable dtData, DateTime trainEndDate, bool isTraining, out DataTable dtInput, out double[] outputs)
        {
            int num = 0;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (Convert.ToDateTime(dtData.Rows[i]["Date"]) <= trainEndDate)
                    num = i;
            }

            DataTable dt1 = dtData.Clone();
            DataTable dt2 = dtData.Clone();

            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (i <= num)
                    dt1.ImportRow(dtData.Rows[i]);
                else
                    dt2.ImportRow(dtData.Rows[i]);
            }

            dtInput = new DataTable();
            if (isTraining)
                dtInput = dt1;
            else
                dtInput = dt2;

            string[] names = new string[4];
            for (int i = 1; i <= 4; i++)
                names[i - 1] = dtInput.Columns[i].ColumnName;
            DataTable dtb = dtInput.DefaultView.ToTable(false, names);

            double[][] inputs = dtb.ToArray();
            outputs = new double[dtb.Rows.Count];
            for (int i = 0; i < dtInput.Rows.Count; i++)
                outputs[i] = dtInput.Rows[i]["Expected"].To<double>();

            return inputs;
        }
      

       

        public static DataTable GetConfusionMatrix(int numClasses, DataTable dt)
        {
            int[] expected = new int[dt.Rows.Count];
            int[] predicted = new int[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                expected[i] = dt.Rows[i]["Expected"].To<int>();
                predicted[i] = dt.Rows[i]["Predicted"].To<int>();
            }

            GeneralConfusionMatrix gcm = new GeneralConfusionMatrix(numClasses, expected, predicted);
            var m = gcm.Matrix;

            DataTable res = new DataTable();
            res.Columns.Add("Name", typeof(string));
            for (int i = 0; i < numClasses; i++)
                res.Columns.Add("C" + i.ToString(), typeof(int));
            res.Columns.Add("Total", typeof(int));

            for (int i = 0; i < numClasses; i++)
            {
                res.Rows.Add("R" + i.ToString());
                int n = res.Rows.Count - 1;
                int tot = 0;
                for (int j = 1; j <= numClasses; j++)
                {
                    res.Rows[n][j] = m[i, j - 1];
                    tot += m[i, j - 1];
                }
                res.Rows[n][numClasses + 1] = tot;
            }

            res.Rows.Add("Total");
            int n1 = res.Rows.Count - 1;
            for (int i = 1; i < res.Columns.Count; i++)
            {
                res.Rows[n1][i] = (res.Compute(string.Format("SUM({0})", res.Columns[i].ColumnName), "")).To<int>();
            }
            return res;
        }

        public static DataTable GetConfusionMatrix(DataTable dt)
        {
            int[] expected = new int[dt.Rows.Count];
            int[] predicted = new int[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                expected[i] = dt.Rows[i]["Expected"].To<int>();
                predicted[i] = dt.Rows[i]["Predicted"].To<int>();
            }

            ConfusionMatrix gcm = new ConfusionMatrix(expected, predicted, 1, -1);
            var m = gcm.Matrix.Transpose();

            int numClasses = 2;

            DataTable res = new DataTable();
            res.Columns.Add("Name", typeof(string));
            for (int i = 0; i < numClasses; i++)
                res.Columns.Add("C" + i.ToString(), typeof(int));
            res.Columns.Add("Total", typeof(int));

            for (int i = 0; i < numClasses; i++)
            {
                res.Rows.Add("R" + i.ToString());
                int n = res.Rows.Count - 1;
                int tot = 0;
                for (int j = 1; j <= numClasses; j++)
                {
                    res.Rows[n][j] = m[i, j - 1];
                    tot += m[i, j - 1];
                }
                res.Rows[n][numClasses + 1] = tot;
            }

            res.Rows.Add("Total");
            int n1 = res.Rows.Count - 1;
            for (int i = 1; i < res.Columns.Count; i++)
            {
                res.Rows[n1][i] = (res.Compute(string.Format("SUM({0})", res.Columns[i].ColumnName), "")).To<int>();
            }
            return res;
        }


        public static DataTable GetConfusionMatrix1(int numClasses, int numTraining, DataTable dt, string trainingOrPredict)
        {
            DataTable dt1 = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (trainingOrPredict.ToUpper() == "TRAINING")
                {
                    if (i <= numTraining)
                        dt1.ImportRow(dt.Rows[i]);
                }
                else if (trainingOrPredict.ToUpper() == "PREDICT")
                {
                    if (i > numTraining)
                        dt1.ImportRow(dt.Rows[i]);
                }
            }

            int[] expected = new int[dt1.Rows.Count];
            int[] predicted = new int[dt1.Rows.Count];
            for(int i =0;i<dt1.Rows.Count;i++)
            {
                expected[i] = dt1.Rows[i]["Classification"].To<int>();
                predicted[i] = dt1.Rows[i]["Predict"].To<int>();
            }

            GeneralConfusionMatrix gcm = new GeneralConfusionMatrix(numClasses, expected, predicted);
            var m =gcm.Matrix;

            DataTable res = new DataTable();
            res.Columns.Add("Name", typeof(string));
            for (int i = 0; i < numClasses; i++)
                res.Columns.Add("C" + i.ToString(), typeof(int));
            res.Columns.Add("Total", typeof(int));

            for(int i=0;i<numClasses;i++)
            {
                res.Rows.Add("R" + i.ToString());
                int n = res.Rows.Count - 1;
                int tot = 0;
                for (int j = 1; j <= numClasses; j++)
                {
                    res.Rows[n][j] = m[i, j - 1];
                    tot += m[i, j - 1];
                }
                res.Rows[n][numClasses + 1] = tot;
            }

            res.Rows.Add("Total");
            int n1 = res.Rows.Count - 1;
            for(int i = 1;i<res.Columns.Count;i++)
            {
                res.Rows[n1][i] = (res.Compute(string.Format("SUM({0})", res.Columns[i].ColumnName), "")).To<int>(); 
            }
            return res;
        }

        public static void SaveAnn(SerializeANN ann, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, ann);
            }
        }     
   
        public static SerializeANN LoadAnn(string fileName)
        {
            SerializeANN ann = new SerializeANN();
            using(FileStream fs = new FileStream(fileName,FileMode.Open))
            {
                var bf = new BinaryFormatter();
                ann = (SerializeANN)bf.Deserialize(fs);
            }
            return ann;
        }
    }


    [Serializable()]
    public class SerializeANN 
    {
        public ActivationNetwork Network { get; set; }
        public DataTable TrainTable { get; set; }
        public DataTable PredictionTable { get; set; }
        public double[] MinMax { get; set; }
        public string[] InputNames { get; set; }
    }
}
