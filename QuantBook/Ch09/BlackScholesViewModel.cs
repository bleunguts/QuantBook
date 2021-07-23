using Caliburn.Micro;
using Chart3DControl;
using QuantBook.Models.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace QuantBook.Ch09
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class BlackScholesViewModel : Screen
    {
        private readonly IEventAggregator events;

        [ImportingConstructor]
        public BlackScholesViewModel(IEventAggregator events)
        {
            this.events = events;
            DisplayName = "01. Black-Scholes";
            InitializeModel();
            DataCollection = new BindableCollection<DataSeries3D>();
        }

        public BindableCollection<DataSeries3D> DataCollection { get; set; }

        private DataTable optionInputTable;

        public DataTable OptionInputTable
        {
            get { return optionInputTable; }
            set { optionInputTable = value; NotifyOfPropertyChange(() => OptionInputTable); }
        }

        private DataTable optionTable;

        public DataTable OptionTable
        {
            get { return optionTable; }
            set { optionTable = value; NotifyOfPropertyChange(() => OptionTable); }
        }

        private double zmin;

        public double Zmin
        {
            get { return zmin; }
            set { zmin = value; NotifyOfPropertyChange(() => Zmin); }
        }

        private double zmax;

        public double Zmax
        {
            get { return zmax; }
            set { zmax = value; }
        }

        private string zLabel;

        public string ZLabel
        {
            get { return zLabel; }
            set { zLabel = value; }
        }

        private double zTick;

        public double ZTick
        {
            get { return zTick; }
            set { zTick = value; }
        }

        private void InitializeModel()
        {
            OptionTable = new DataTable();
            OptionTable.Columns.AddRange(new[]
            {
                new DataColumn("Maturity", typeof(double)),
                new DataColumn("Price", typeof(double)),
                new DataColumn("Delta", typeof(double)),
                new DataColumn("Gamma", typeof(double)),
                new DataColumn("Theta", typeof(double)),
                new DataColumn("Rho", typeof(double)),
                new DataColumn("Vega", typeof(double)),

            });
            OptionInputTable = new DataTable();
            OptionInputTable.Columns.AddRange(new[]
            {
                new DataColumn("Parameter", typeof(string)),
                new DataColumn("Value", typeof(string)),
                new DataColumn("Description", typeof(string)),
            });
            OptionInputTable.Rows.Add("OptionType", "Call", "Call for a call option, Put for a putoption");
            OptionInputTable.Rows.Add("Spot", 100, "Current price of the underlying asset");
            OptionInputTable.Rows.Add("Strike", 100, "Strike price");
            OptionInputTable.Rows.Add("Rate", 0.1, "Interest rate");
            OptionInputTable.Rows.Add("Carry", 0.04, "Cost of carry");
            OptionInputTable.Rows.Add("Vol", 0.3, "Volatility");
        }

        public void CalculatePrice()
        {
            OptionType optionType = optionInputTable.Rows[0]["Value"].ToString() == "Call" ? OptionType.CALL : OptionType.PUT;
            double spot = Convert.ToDouble(OptionInputTable.Rows[1]["Value"]);
            double strike = Convert.ToDouble(OptionInputTable.Rows[2]["Value"]);
            double rate = Convert.ToDouble(OptionInputTable.Rows[3]["Value"]);
            double carry = Convert.ToDouble(OptionInputTable.Rows[4]["Value"]);
            double vol = Convert.ToDouble(OptionInputTable.Rows[5]["Value"]);
            OptionTable.Clear();
            for (int i = 0; i < 10; i++)
            {
                // break out into 10 time slices until maturity
                double maturity = (i + 1.0) / 10.0;
                
                //price & greeks
                double price = OptionHelper.BlackScholes(optionType, spot, strike, rate, carry, maturity, vol);
                double delta = OptionHelper.BlackScholes_Delta(optionType, spot, strike, rate, carry, maturity, vol);
                double gamma = OptionHelper.BlackScholes_Gamma(spot, strike, rate, carry, maturity, vol);
                double theta = OptionHelper.BlackScholes_Theta(optionType, spot, strike, rate, carry, maturity, vol);
                double rho = OptionHelper.BlackScholes_Theta(optionType, spot, strike, rate, carry, maturity, vol);
                double vega = OptionHelper.BlackScholes_Vega(spot, strike, rate, carry, maturity, vol);
                OptionTable.Rows.Add(maturity, price, delta, gamma, theta, rho, vega);
            }
        }

        public void PlotPrice()
        {
            ZLabel = "Price";
            OptionType optionType = optionInputTable.Rows[0]["Value"].ToString() == "Call" ? OptionType.CALL : OptionType.PUT;
            double spot = Convert.ToDouble(OptionInputTable.Rows[1]["Value"]);
            double strike = Convert.ToDouble(OptionInputTable.Rows[2]["Value"]);
            double rate = Convert.ToDouble(OptionInputTable.Rows[3]["Value"]);
            double carry = Convert.ToDouble(OptionInputTable.Rows[4]["Value"]);
            double vol = Convert.ToDouble(OptionInputTable.Rows[5]["Value"]);
            DataCollection.Clear();
            var ds = new DataSeries3D();
            ds.LineColor = Brushes.Black;
            double[] z = OptionPlotHelper.PlotGreeks(ds, GreekTypeEnum.Price, optionType, strike, rate, carry, vol);
            Zmin = Math.Round(z[0], 1);
            Zmax = Math.Round(z[1], 1);
            ZTick = Math.Round(z[1] - z[0] / 5.0, 1);
            DataCollection.Add(ds);
        }
    }    
}
