using Caliburn.Micro;
using QLNet;
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
    public class QlRealWorldOptionViewModel : Screen
    {
        [ImportingConstructor]
        public QlRealWorldOptionViewModel()
        {
            DisplayName = "09. QL: Real-World";
            OptionTable = new DataTable();
            OptionTable.Columns.AddRange(new[]
            {
                new DataColumn("Strike", typeof(double)),
                new DataColumn("Price", typeof(double)),
                new DataColumn("Delta", typeof(double)),
                new DataColumn("Gamma", typeof(double)),
                new DataColumn("Theta", typeof(double)),
                new DataColumn("Rho", typeof(double)),
                new DataColumn("Vega", typeof(double))
            });
        }

        private DataTable optionTable;

        public DataTable OptionTable
        {
            get { return optionTable; }
            set { optionTable = value; NotifyOfPropertyChange(() => OptionTable); }
        }

        private IEnumerable<AmericanEngineType> engineType;

        public IEnumerable<AmericanEngineType> EngineType
        {
            get { return Enum.GetValues(typeof(AmericanEngineType)).Cast<AmericanEngineType>(); }
            set { engineType = value; NotifyOfPropertyChange(() => EngineType); }
        }

        private AmericanEngineType selectedEngineType;

        public AmericanEngineType SelectedEngineType
        {
            get { return selectedEngineType; }
            set { selectedEngineType = value; NotifyOfPropertyChange(() => SelectedEngineType); }
        }

        public void CalculatePrice()
        {
            OptionTable.Clear();
            // Pricing INTC Calls expiring on Feb 21, 2014
            //var evalDate = new Date(15, Month.November, 2013);
            //var maturity = new Date(21, Month.February, 2014);
            //var exDivDate = new Date(5, Month.February, 2014);

            var evalDate = new Date(28, Month.Jul, 2021);
            var maturity = new Date(21, Month.Apr, 2022);
            var exDivDate = new Date(5, Month.Apr, 2022);

            double spot = 24.52;
            double[] strikes = new double[] { 22.0, 23.0, 24.0, 25.0, 26.0, 27.0, 28.0 };
            double dividend = 0.22;
            int dividendFrequency = 3; // Dividend paid quarterly
            double[] rates = new double[] { 0.001049, 0.0012925, 0.001675, 0.00207, 0.002381, 0.0035140, 0.005841 };
            double[] vols = new double[] { 0.23362, 0.21374, 02.0661, 0.20132, 0.19921, 0.19983, 0.20122 };

            var results = QuantLibHelper.AmericanOptionRealWorld(OptionType.Call, evalDate, maturity, spot, strikes, vols, rates, dividend, dividendFrequency, exDivDate, SelectedEngineType, 500);
            foreach(var result in results)
            {
                OptionTable.Rows.Add(result.strike, result.npv, result.delta, result.gamma, result.theta, result.rho, result.vega);
            }
        }



    }
}
