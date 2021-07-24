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

        // Approximation of cumulative normal distributuion function
        public static double CummulativeNormal(double x)
        {
            if (x < 0)
                return 1.0 - CummulativeNormal(-x);

            double k = 1.0 / (1.0 + 0.2316419 * x);
            return 1.0 - ONEOVERSQRT2PI * Math.Exp(-0.5 * x * x) *
                    ((((1.330274429 * k - 1.821255978) * k + 1.781477937) * k - 0.356563782) * k + 0.319381530) * k;
        }
        
        // Standard Normal Density function        
        private static double NormalDensity(double z) => Math.Exp(-z * z * 0.5) / Math.Sqrt(2.0 * PI);

        static double d1Func(double spot, double strike, double carry, double volatility, double maturity) =>          
            (Math.Log(spot / strike) + (carry + (volatility * volatility) / 2) * maturity) 
                        / 
            (volatility * Math.Sqrt(maturity));

        static double d2Func(double d1, double volatility, double maturity) => 
            d1 - volatility * Math.Sqrt(maturity);

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
            double d1 = d1Func(spot, strike, carry, volatility, maturity);
            double d2 = d2Func(d1, volatility, maturity);
                    
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
        }
      
        public static double BlackScholes_Delta(OptionType optionType, double spot, double strike, double rate, double carry, double maturity, double volatility)
        {
            double d1 = d1Func(spot, strike, carry, volatility, maturity);

            double? option = null;
            switch (optionType)
            {
                case OptionType.PUT:
                    option = Math.Exp((carry - rate) * maturity) * (CummulativeNormal(d1) - 1.0);
                    break;
                case OptionType.CALL:
                    option = Math.Exp((carry - rate) * maturity) * (CummulativeNormal(d1));
                    break;
            }
            return option.Value;
        }
  

        public static double BlackScholes_Gamma(double spot, double strike, double rate, double carry, double maturity, double volatility)
        {
            double d1 = d1Func(spot, strike, carry, volatility, maturity);

            double option = NormalDensity(d1) * Math.Exp((carry - rate) * maturity) / (spot * volatility * Math.Sqrt(maturity));
            return option;
        }

        public static double BlackScholes_Theta(OptionType optionType, double spot, double strike, double rate, double carry, double maturity, double volatility)
        {
            double d1 = d1Func(spot, strike, carry, volatility, maturity);
            double d2 = d2Func(d1, volatility, maturity);

            double? option = null;
            switch (optionType)
            {
                case OptionType.PUT:
                    var p1 = (spot * Math.Exp((carry - rate) * maturity) * NormalDensity(d1) * volatility) / (2 * Math.Sqrt(maturity));
                    var p2 = (carry - rate) * spot * Math.Exp((carry - rate) * maturity) * CummulativeNormal(-d1);
                    var p3 = rate * strike * Math.Exp(-rate * maturity) * CummulativeNormal(-d2);
                    option = -p1 + p2 + p3;
                    break;
                case OptionType.CALL:
                    var c1 = (spot * Math.Exp((carry - rate) * maturity) * NormalDensity(d1) * volatility) / (2 * Math.Sqrt(maturity));
                    var c2 = (carry - rate) * spot * Math.Exp((carry - rate) * maturity) * CummulativeNormal(d1);
                    var c3 = rate * strike * Math.Exp(-rate * maturity) * CummulativeNormal(d2);
                    option = -c1 - c2 - c3;
                    break;
            }
            return option.Value;
        }

        public static double BlackScholes_Rho(OptionType optionType, double spot, double strike, double rate, double carry, double maturity, double volatility)
        {
            double d1 = d1Func(spot, strike, carry, volatility, maturity);
            double d2 = d2Func(d1, volatility, maturity);

            // carry == 0 means option on a futures contract simple formula in that case
            if (carry == 0)
            {
                return -maturity * BlackScholes(optionType, spot, strike, rate, 0, maturity, volatility);
            }

            // otherwise
            double? option = null;
            switch (optionType)
            {
                case OptionType.PUT:
                    option = -maturity * strike * Math.Exp(-rate * maturity) * CummulativeNormal(-d2);                
                    break;                    
                case OptionType.CALL:
                    option = maturity * strike * Math.Exp(-rate * maturity) * CummulativeNormal(d2);
                    break;
            }
            
            return option.Value;
        }        

        public static double BlackScholes_Vega(double spot, double strike, double rate, double carry, double maturity, double vol)
        {
            double d1 = d1Func(spot, strike, carry, vol, maturity);
            return spot * Math.Exp((carry - rate) * maturity) * CummulativeNormal(d1) * Math.Sqrt(maturity);
        }
    }
}
