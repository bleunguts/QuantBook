using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.Options
{
    public enum OptionType { CALL, PUT };
    public class OptionHelper
    {
        private const double ONEOVERSQRT2PI = 0.39894228;
        private const double PI = 3.1415926;


        /// <summary>
        /// Generalized Black Scholes Model 
        /// Supports: 
        /// * Pricing European options on stocks
        /// * Stocks paying a continuous dividend yield
        /// * Options on Futures Contracts
        /// * Currency Options
        /// </summary>
        /// <param name="optionType"></param>
        /// <param name="spot">spot price</param>
        /// <param name="strike">strike price</param>
        /// <param name="rate">interest rate used by TVM calcs</param>
        /// <param name="carry">cost-of-carry rate. The net cost of holding a position e.g. storage costs for physicals, interest expenses, opportunity costs lost for taking this position over another</param>
        /// <param name="maturity">number of days to maturity</param>
        /// <param name="volatility">price volatility std.dev</param>
        /// <returns></returns>
        public static double BlackScholes(OptionType optionType, double spot, double strike, double rate, double carry, double maturity, double volatility)
        {            
            double d1, d2;
            
            d1 = ( Math.Log(spot / strike) + (carry + (volatility * volatility) / 2) ) / ( volatility * Math.Sqrt(maturity) );
            d2 = d1 - volatility * Math.Sqrt(maturity);

            double? option = null;
            switch(optionType)
            { 
                case OptionType.PUT:
                    option = ( strike * Math.Exp(-rate * maturity) * CummulativeNormal(-d2) ) - ( spot * Math.Exp((carry-rate) * maturity) * CummulativeNormal(-d1) );
                    break;
                case OptionType.CALL:
                    option = (spot * Math.Exp((carry - rate) * maturity) * CummulativeNormal(d1)) - (strike * Math.Exp(-rate * maturity) * CummulativeNormal(d2));
                    break;                
            }            
            return option.Value;

            // Approximation of cumulative normal distributuion function
            double CummulativeNormal(double x)
            {
                if (x < 0)
                    return 1.0 - CummulativeNormal(-x);

                double k = 1.0 / (1.0 + 0.2316419 * x);
                return 1.0 - ONEOVERSQRT2PI * Math.Exp(-0.5 * x * x) *
                        ((((1.330274429 * k - 1.821255978) * k + 1.781477937) * k - 0.356563782) * k + 0.319381530) * k;
            }
        }

        internal static double BlackScholes_Vega(double y, double strike, double rate, double carry, double x, double vol)
        {
            throw new NotImplementedException();
        }

        internal static double BlackScholes_Rho(OptionType optionType, double y, double strike, double rate, double carry, double x, double vol)
        {
            throw new NotImplementedException();
        }

        internal static double BlackScholes_Theta(OptionType optionType, double y, double strike, double rate, double carry, double x, double vol)
        {
            throw new NotImplementedException();
        }

        internal static double BlackScholes_Gamma(double y, double strike, double rate, double carry, double x, double vol)
        {
            throw new NotImplementedException();
        }

        internal static double BlackScholes_Delta(OptionType optionType, double y, double strike, double rate, double carry, double x, double vol)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Standard Normal Density function
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        private static double NormalDensity(double z)
        {
            return Math.Exp(-z * z * 0.5) / Math.Sqrt(2.0 * PI);
        }
    }
}
