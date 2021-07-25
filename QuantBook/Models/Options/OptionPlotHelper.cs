using Chart3DControl;
using QuantBook.Ch09;
using System;
using System.Windows.Media.Media3D;

namespace QuantBook.Models.Options
{
    public enum GreekTypeEnum 
    { 
        Delta = 0,
        Gamma = 1,
        Theta = 2,
        Rho = 3,
        Vega = 4,
        Price = 5,
    };

    public static class OptionPlotHelper
    {
        public static double[] PlotGreeks(DataSeries3D ds, GreekTypeEnum greekType, OptionType optionType, double strike, double rate, double carry, double vol)
        {
            double xmin = 0.1;
            double xmax = 3.0;
            double ymin = 10;
            double ymax = 190;
            ds.XLimitMin = xmin;
            ds.YLimitMin = ymin;
            ds.XSpacing = 0.1;
            ds.YSpacing = 5;
            ds.XNumber = Convert.ToInt16((xmax - xmin) / ds.XSpacing) + 1;
            ds.YNumber = Convert.ToInt16((ymax - ymin) / ds.YSpacing) + 1;

            Point3D[,] pts = new Point3D[ds.XNumber, ds.YNumber];
            double zmin = 10_000;
            double zmax = -10_000;
            for (int i = 0; i < ds.XNumber; i++)
            {
                for (int j = 0; j < ds.YNumber; j++)
                {
                    double x = ds.XLimitMin + i * ds.XSpacing;
                    double y = ds.YLimitMin + j * ds.YSpacing;
                    double z = Double.NaN;
                    var spot = y;
                    var maturity = x;
                    switch (greekType) 
                    {                    
                        case GreekTypeEnum.Delta:
                            z = OptionHelper.BlackScholes_Delta(optionType, spot, strike, rate, carry, maturity, vol);
                            break;
                        case GreekTypeEnum.Gamma:
                            z = OptionHelper.BlackScholes_Gamma(spot, strike, rate, carry, maturity, vol);
                            break;
                        case GreekTypeEnum.Theta:
                            z = OptionHelper.BlackScholes_Theta(optionType, spot, strike, rate, carry, maturity, vol);
                            break;
                        case GreekTypeEnum.Rho:
                            z = OptionHelper.BlackScholes_Rho(optionType, spot, strike, rate, carry, maturity, vol);
                            break;
                        case GreekTypeEnum.Vega:
                            z = OptionHelper.BlackScholes_Vega(spot, strike, rate, carry, maturity, vol);
                            break;
                        case GreekTypeEnum.Price:
                            z = OptionHelper.BlackScholes(optionType, spot, strike, rate, carry, maturity, vol);
                            break;
                    }
                    if(!double.IsNaN(z))
                    {
                        pts[i, j] = new Point3D(x, y, z);
                        zmin = Math.Min(zmin, z);
                        zmax = Math.Max(zmax, z);
                    }
                }
            }
            ds.PointArray = pts;
            return new double[] { zmin, zmax };
        }

        public static double[] PlotImpliedVol(DataSeries3D ds, OptionType optionType, double spot, double price, double rate, double carry)
        {
            double xmin = 0.1;
            double xmax = 1.0;
            double ymin = 9.5;
            double ymax = 10.5;
            ds.XLimitMin = xmin;
            ds.YLimitMin = ymin;
            ds.XSpacing = 0.03;
            ds.YSpacing = 0.05;
            ds.XNumber = Convert.ToInt16((xmax - xmin) / ds.XSpacing) + 1;
            ds.YNumber = Convert.ToInt16((ymax - ymin) / ds.YSpacing) + 1;
            double zmin = 10_000;
            double zmax = -10_000;
            Point3D[,] pts = new Point3D[ds.XNumber, ds.YNumber];
            for (int i = 0; i < ds.XNumber; i++)
            {
                for (int j = 0; j < ds.YNumber; j++)
                {
                    double x = ds.XLimitMin + i * ds.XSpacing;
                    double y = ds.YLimitMin + j * ds.YSpacing;
                    double z = double.NaN;
                    z = OptionHelper.BlackScholes_ImpliedVol(optionType, spot, y, rate, carry, x, price);
                    if(!double.IsNaN(z))
                    {
                        pts[i, j] = new Point3D(x, y, z);
                        zmin = Math.Min(zmin, z);
                        zmax = Math.Max(zmax, z);
                    }
                }
            }
            ds.PointArray = pts;
            return new double[] { zmin, zmax };
        }
    }
}