using Caliburn.Micro;
using QuantBook.Models.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Ch09
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class QlEuropeanOptionViewModel : Screen
    {
        [ImportingConstructor]
        public QlEuropeanOptionViewModel()
        {
            DisplayName = "05. QL: European";
            InitializeModel();
        }

        private DataTable optionInputTable;

        public DataTable OptionInputTable
        {
            get { return optionInputTable; }
            set { optionInputTable = value; NotifyOfPropertyChange(() => OptionInputTable); }
        }

        private DataTable volInputTable;

        public DataTable VolInputTable
        {
            get { return volInputTable; }
            set { volInputTable = value; NotifyOfPropertyChange(() => VolInputTable);  }
        }

        private DataTable optionTable;

        public DataTable OptionTable
        {
            get { return optionTable; }
            set { optionTable = value; NotifyOfPropertyChange(() => OptionTable); }
        }

        private DataTable volTable;

        public DataTable VolTable
        {
            get { return volTable; }
            set { volTable = value; }
        }

        private IEnumerable<EuropeanEngineType> engineType;

        public IEnumerable<EuropeanEngineType> EngineType
        {
            get { return Enum.GetValues(typeof(EuropeanEngineType)).Cast<EuropeanEngineType>(); }
            set { engineType = value; NotifyOfPropertyChange(() => EngineType); }
        }

        private EuropeanEngineType selectedEngineType;

        public EuropeanEngineType SelectedEngineType
        {
            get { return selectedEngineType; }
            set { selectedEngineType = value; NotifyOfPropertyChange(() => SelectedEngineType); }
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
                new DataColumn("Vega", typeof(double))
            });
            VolTable = new DataTable();
            VolTable.Columns.AddRange(new[]
            {
                new DataColumn("Maturity", typeof(double)),
                new DataColumn("Option Price", typeof(double)),
                new DataColumn("Volatility", typeof(double)),
            });

            OptionInputTable = new DataTable();
            OptionInputTable.Columns.AddRange(new[]
            {
                new DataColumn("Parameter", typeof(string)),
                new DataColumn("Value", typeof(string)),
                new DataColumn("Description", typeof(string)),                
            });
            OptionInputTable.Rows.Add("OptionType", "Call", "Call for a call option otherwise a put option");
            OptionInputTable.Rows.Add("Spot", 100, "Spot Price");
            OptionInputTable.Rows.Add("Strike", 100, "Strike Price");
            OptionInputTable.Rows.Add("Rate", 0.1, "Interest Rate");
            OptionInputTable.Rows.Add("Carry", 0.04, "Cost of carry");
            OptionInputTable.Rows.Add("Vol", 0.3, "Volatility");

            VolInputTable = new DataTable();
            VolInputTable.Columns.AddRange(new[]
            {
                new DataColumn("Parameter", typeof(string)),
                new DataColumn("Value", typeof(string)),
                new DataColumn("Description", typeof(string)),
            });
            VolInputTable.Rows.Add("OptionType", "Call", "Call for a call option otherwise a put option");
            VolInputTable.Rows.Add("Spot", 100, "Spot Price");
            VolInputTable.Rows.Add("Strike", 100, "Strike Price");
            VolInputTable.Rows.Add("Rate", 0.1, "Interest Rate");
            VolInputTable.Rows.Add("Carry", 0.04, "Cost of carry");
            VolInputTable.Rows.Add("Vol", 0.3, "Volatility");
        }

        public void CalculatePrice()
        {
            var optionType = OptionInputTable.Rows[0]["Value"].ToString() == "Call" ? OptionType.Call : OptionType.Put;
            var spot = Convert.ToDouble(OptionInputTable.Rows[1]["Value"]);
            var strike = Convert.ToDouble(OptionInputTable.Rows[2]["Value"]);
            var rate = Convert.ToDouble(OptionInputTable.Rows[3]["Value"]);
            var carry = Convert.ToDouble(OptionInputTable.Rows[4]["Value"]);
            var vol = Convert.ToDouble(OptionInputTable.Rows[5]["Value"]);

            OptionTable.Clear();
            for (int i = 0; i < 10; i++)
            {
                double maturity = (i + 1.0) / 10.0;
                var (value, delta, gamma, theta, rho, vega) = QuantLibHelper.EuropeanOption(optionType, DateTime.Today, maturity, strike, spot, rate - carry, rate, vol, SelectedEngineType);
                OptionTable.Rows.Add(maturity, value, delta, gamma, theta, rho, vega);
            }
        }

        public void CalculateVol()
        {
            var optionType = VolInputTable.Rows[0]["Value"].ToString() == "Call" ? OptionType.Call : OptionType.Put;
            var spot = Convert.ToDouble(VolInputTable.Rows[1]["Value"]);
            var strike = Convert.ToDouble(VolInputTable.Rows[2]["Value"]);
            var rate = Convert.ToDouble(VolInputTable.Rows[3]["Value"]);
            var carry = Convert.ToDouble(VolInputTable.Rows[4]["Value"]);
            VolTable.Clear();
            var (price, _, _, _, _, _) = QuantLibHelper.EuropeanOption(optionType, DateTime.Today, 1, strike, spot, rate - carry, rate, 0.3, EuropeanEngineType.Analytic);
            var prices = new double[] { 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };
            for (int i = 0; i < 10; i++)
            {
                var quotedPrice = prices[i] + price;
                double maturity = (i + 1.0) / 10.0;
                var vol = QuantLibHelper.EuropeanOptionImpliedVol(optionType, DateTime.Today, maturity, strike, spot, rate - carry, rate, quotedPrice);
                VolTable.Rows.Add(maturity, quotedPrice, vol);
            }
        }
    }
}
