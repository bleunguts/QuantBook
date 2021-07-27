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
    public class BarrierOptionViewModel : Screen
    {
        private readonly IEventAggregator events;

        [ImportingConstructor]
        public BarrierOptionViewModel(IEventAggregator events)
        {
            this.events = events;
            DisplayName = "04. Barrier Option";
            InitializeModel();
        }

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
            set { optionTable = value; NotifyOfPropertyChange(() => OptionTable);  }
        }


        private void InitializeModel()
        {
            OptionTable = new DataTable();
            OptionTable.Columns.AddRange(new[]
            {
                new DataColumn("Maturity", typeof(double)),
                new DataColumn("DownIn", typeof(double)),
                new DataColumn("DownOut", typeof(double)),
                new DataColumn("UpIn", typeof(double)),
                new DataColumn("UpOut", typeof(double)),
            });
            OptionInputTable = new DataTable();
            OptionInputTable.Columns.AddRange(new[]
            {
                new DataColumn("Parameter", typeof(string)),
                new DataColumn("Value", typeof(string)),
                new DataColumn("Description", typeof(string)),                
            });
            OptionInputTable.Rows.Add("OptionType", "Call", "Call for a call option, otherwise a put option");
            OptionInputTable.Rows.Add("Spot", 100, "Current price of the underlying asset");
            OptionInputTable.Rows.Add("Strike", 100, "Strike Price");
            OptionInputTable.Rows.Add("Rate", 0.1, "Interest rate");
            OptionInputTable.Rows.Add("DivYield", 0.06, "Continous dividend yield");
            OptionInputTable.Rows.Add("Vol", 0.3, "Volatility");
            OptionInputTable.Rows.Add("Barrier", 90, "Barrier");
            OptionInputTable.Rows.Add("Rebate", 0, "Paid off if barrier is not knocked in during its life");
        }

        public void CalculatePRice()
        {
            OptionTable.Clear();
            (OptionType optionType, double spot, double strike, double rate, double yield, double vol, double barrier, double rebate) = FromUI();

            for (int i = 1; i <= 10; i++)
            {
                double maturity = 0.1 * i;
                var value_downIn = OptionHelper.BarrierOptions(optionType, BarrierType.DownIn, spot, strike, rate, yield, maturity, vol, barrier, rebate);
                var value_upIn = OptionHelper.BarrierOptions(optionType, BarrierType.UpIn, spot, strike, rate, yield, maturity, vol, barrier, rebate);
                var value_downOut = OptionHelper.BarrierOptions(optionType, BarrierType.DownOut, spot, strike, rate, yield, maturity, vol, barrier, rebate);
                var value_upOut = OptionHelper.BarrierOptions(optionType, BarrierType.UpOut, spot, strike, rate, yield, maturity, vol, barrier, rebate);
                OptionTable.Rows.Add(maturity, value_downIn,  value_downOut, value_upIn, value_upOut);
            }
        }

        (OptionType optionType, double spot, double strike, double rate, double yield, double vol, double barrier, double rebate) FromUI()
        {
            OptionType optionType = OptionInputTable.Rows[0]["Value"].ToString() == "Call" ? OptionType.Call : OptionType.Put;
            double spot = Convert.ToDouble(OptionInputTable.Rows[1]["Value"]);
            double strike = Convert.ToDouble(OptionInputTable.Rows[2]["Value"]);
            double rate = Convert.ToDouble(OptionInputTable.Rows[3]["Value"]);
            double yield = Convert.ToDouble(OptionInputTable.Rows[4]["Value"]);
            double vol = Convert.ToDouble(OptionInputTable.Rows[5]["Value"]);
            double barrier = Convert.ToDouble(OptionInputTable.Rows[6]["Value"]);
            double rebate = Convert.ToDouble(OptionInputTable.Rows[7]["Value"]);
            return (optionType, spot, strike, rate, yield, vol, barrier, rebate);
        }
    }
}
