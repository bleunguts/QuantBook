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
    public class ImpliedVolViewModel : Screen
    {
        private readonly IEventAggregator events;

        [ImportingConstructor]
        public ImpliedVolViewModel(IEventAggregator events)
        {
            this.events = events;
            DisplayName = "02. Implied Vol";
            InitializeModel();
            DataCollection = new BindableCollection<DataSeries3D>();
        }

        public BindableCollection<DataSeries3D> DataCollection { get; set; }

        private DataTable volInputTable;

        public DataTable VolInputTable
        {
            get { return volInputTable; }
            set { volInputTable = value; NotifyOfPropertyChange(() => VolInputTable); }
        }

        private DataTable volTable;

        public DataTable VolTable
        {
            get { return volTable; }
            set { volTable = value; NotifyOfPropertyChange(() => volTable); }
        }

        private double zmin = 0;

        public double Zmin
        {
            get { return zmin; }
            set { zmin = value; NotifyOfPropertyChange(() => Zmin); }
        }

        private double zmax = 1;

        public double Zmax
        {
            get { return zmax; }
            set { zmax = value; NotifyOfPropertyChange(() => Zmax); }
        }

        private string zLabel = "Vol";

        public string ZLabel
        {
            get { return zLabel; }
            set { zLabel = value; NotifyOfPropertyChange(() => ZLabel); }
        }

        private double zTick = 0.2;

        public double ZTick
        {
            get { return zTick; }
            set { zTick = value; NotifyOfPropertyChange(() => ZTick); }
        }

        private void InitializeModel()
        {
            VolTable = new DataTable();
            VolTable.Columns.AddRange(new[] {
                new DataColumn("Maturity", typeof(double)),
                new DataColumn("Option Price", typeof(double)),
                new DataColumn("Volatility", typeof(double))
            });

            VolInputTable = new DataTable();
            VolInputTable.Columns.AddRange(new[]
            {
                new DataColumn("Parameter", typeof(string)),
                new DataColumn("Value", typeof(string)),
                new DataColumn("Descripton", typeof(string)),                
            }) ;
            VolInputTable.Rows.Add("OptionType", "Call", "Call for a call option, Put for a put option");
            VolInputTable.Rows.Add("Spot", 10, "Current price of the underlying asset");
            VolInputTable.Rows.Add("Strike", 10.5, "Strike price");
            VolInputTable.Rows.Add("Rate", 0.1, "Interest rate");
            VolInputTable.Rows.Add("Carry", 0.06, "Cost of carry");
            VolInputTable.Rows.Add("Vol", 0.3, "Volatility");
        }

        public void CalculateVol()
        {
            (OptionType optionType, double spot, double strike, double rate, double carry, double vol) = FromUI();
            VolTable.Clear();
            double[] prices = new double[] { 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6 };
            for (int i = 0; i < 10; i++)
            {
                double maturity = (i + 1.0) / 10.0;
                double volatility = OptionHelper.BlackScholes_ImpliedVol(optionType, spot, strike, rate, carry, maturity, prices[i]);
                VolTable.Rows.Add(maturity, prices[i], volatility);
            }
            
        }     

        public void PlotVol()
        {
            ZLabel = "Vol";
            (OptionType optionType, double spot, double strike, double rate, double carry, double vol) = FromUI();
            DataCollection.Clear();
            DataSeries3D ds = new DataSeries3D() { LineColor = Brushes.Black };
            double[] z = OptionPlotHelper.PlotImpliedVol(ds, optionType, spot, 0.5, rate, carry);
            Zmin = Math.Round(z[0], 1);
            Zmax = Math.Round(z[1], 1);
            ZTick = Math.Round((z[1] - z[0]) / 5.0, 1);
            DataCollection.Add(ds);
        }

        (OptionType optionType, double spot, double strike, double rate, double carry, double vol) FromUI()
        {
            OptionType optionType = VolInputTable.Rows[0]["Value"].ToString() == "Call" ? OptionType.Call : OptionType.Put;
            double spot = Convert.ToDouble(VolInputTable.Rows[1]["Value"]);
            double strike = Convert.ToDouble(VolInputTable.Rows[2]["Value"]);
            double rate = Convert.ToDouble(VolInputTable.Rows[3]["Value"]);
            double carry = Convert.ToDouble(VolInputTable.Rows[4]["Value"]);
            double vol = Convert.ToDouble(VolInputTable.Rows[5]["Value"]);
            return (optionType, spot, strike, rate, carry, vol);
        }
    }
}
