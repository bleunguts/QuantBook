using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.ChartModel
{
    public enum MyIndicatorTypeEnum
    {
        SimpleLinearRegression = 0,
        Simple2DPca,
    }

    public enum ChartBackgroundColor
    {
        Blue = 0,
        Green = 1,
        Red = 2,
        White = 3,
    }

    public enum ChartTypeEnum
    {
        MyChart = 0,
        MyChart2 = 1,
        MyChart3 = 2,
    }

    public enum RegressionType
    {
        None = 0,
        SimpleLinear = 1,
        SimplePca = 2,
        Both = 3,
    }

    public class OhlcIndexEntity
    {
        public int Index { get; set; }
        public double PriceOpen { get; set; }
        public double PriceHigh { get; set; }
        public double PriceLow { get; set; }
        public double PriceClose { get; set; }
    }
}
