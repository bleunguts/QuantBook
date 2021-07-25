﻿using Caliburn.Micro;
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
    public class AmericanBAWViewModel : Screen
    {
        private readonly IEventAggregator events;

        [ImportingConstructor]
        public AmericanBAWViewModel(IEventAggregator events)
        {
            this.events = events;
            DisplayName = "03. American BAW";
            InitializeModel();
        }

        private DataTable inputTable;

        public DataTable InputTable
        {
            get { return inputTable; }
            set { inputTable = value; NotifyOfPropertyChange(() => InputTable); }
        }

        private DataTable optionTable;

        public DataTable OptionTable
        {
            get { return optionTable; }
            set { optionTable = value; NotifyOfPropertyChange(() => OptionTable); }
        }        

        private void InitializeModel()
        {            
            OptionTable = new DataTable();
            OptionTable.Columns.AddRange(new[]
            {
                new DataColumn("BAWValue", typeof(float)),
                new DataColumn("Calculated", typeof(float)),
            });

            InputTable = new DataTable();
            InputTable.Columns.AddRange(new[]
            {
                new DataColumn("OptionType", typeof(string)),
                new DataColumn("Strike", typeof(float)),
                new DataColumn("Spot", typeof(float)),
                new DataColumn("DivYield", typeof(float)),
                new DataColumn("Rate", typeof(float)),
                new DataColumn("Maturity", typeof(float)),
                new DataColumn("Vol", typeof(float)),
                new DataColumn("BAWValue", typeof(float)),
            }) ;
            InputTable.Rows.Add("Call", 100.00, 90.00, 0.10, 0.10, 0.10, 0.15, 0.0206);
            InputTable.Rows.Add("Call", 100.00, 100.00, 0.10, 0.10, 0.10, 0.15, 1.8771);
            InputTable.Rows.Add("Call", 100.00, 110.00, 0.10, 0.10, 0.10, 0.15, 10.0089);
            InputTable.Rows.Add("Call", 100.00, 90.00, 0.10, 0.10, 0.10, 0.25, 0.3159);
            InputTable.Rows.Add("Call", 100.00, 100.00, 0.10, 0.10, 0.10, 0.25, 3.1280);
            InputTable.Rows.Add("Call", 100.00, 110.00, 0.10, 0.10, 0.10, 0.25, 10.3919);
            InputTable.Rows.Add("Call", 100.00, 90.00, 0.10, 0.10, 0.10, 0.35, 0.9495);
            InputTable.Rows.Add("Call", 100.00, 100.00, 0.10, 0.10, 0.10, 0.35, 4.3777);
            InputTable.Rows.Add("Call", 100.00, 110.00, 0.10, 0.10, 0.10, 0.35, 11.1679);
            InputTable.Rows.Add("Call", 100.00, 90.00, 0.10, 0.10, 0.50, 0.15, 0.8208);
            InputTable.Rows.Add("Call", 100.00, 100.00, 0.10, 0.10, 0.50, 0.15, 4.0842);
            InputTable.Rows.Add("Call", 100.00, 110.00, 0.10, 0.10, 0.50, 0.15, 10.8087);
            InputTable.Rows.Add("Call", 100.00, 90.00, 0.10, 0.10, 0.50, 0.25, 2.7437);
            InputTable.Rows.Add("Call", 100.00, 100.00, 0.10, 0.10, 0.50, 0.25, 6.8015);
            InputTable.Rows.Add("Call", 100.00, 110.00, 0.10, 0.10, 0.50, 0.25, 13.0170);
            InputTable.Rows.Add("Call", 100.00, 90.00, 0.10, 0.10, 0.50, 0.35, 5.0063);
            InputTable.Rows.Add("Call", 100.00, 100.00, 0.10, 0.10, 0.50, 0.35, 9.5106);
            InputTable.Rows.Add("Call", 100.00, 110.00, 0.10, 0.10, 0.50, 0.35, 15.5689);          

            InputTable.Rows.Add("Put", 100.00, 90.00, 0.10, 0.10, 0.10, 0.15, 10.0000);
            InputTable.Rows.Add("Put", 100.00, 100.00, 0.10, 0.10, 0.10, 0.15, 1.8770);
            InputTable.Rows.Add("Put", 100.00, 110.00, 0.10, 0.10, 0.10, 0.15, 0.0410);
            InputTable.Rows.Add("Put", 100.00, 90.00, 0.10, 0.10, 0.10, 0.25, 10.2533);
            InputTable.Rows.Add("Put", 100.00, 100.00, 0.10, 0.10, 0.10, 0.25, 3.1277);
            InputTable.Rows.Add("Put", 100.00, 110.00, 0.10, 0.10, 0.10, 0.25, 0.4562);
            InputTable.Rows.Add("Put", 100.00, 90.00, 0.10, 0.10, 0.10, 0.35, 10.8787);
            InputTable.Rows.Add("Put", 100.00, 100.00, 0.10, 0.10, 0.10, 0.35, 4.3777);
            InputTable.Rows.Add("Put", 100.00, 110.00, 0.10, 0.10, 0.10, 0.35, 1.2402);

            InputTable.Rows.Add("Put", 100.00, 90.00, 0.10, 0.10, 0.50, 0.15, 10.5595);
            InputTable.Rows.Add("Put", 100.00, 100.00, 0.10, 0.10, 0.50, 0.15, 4.0842);
            InputTable.Rows.Add("Put", 100.00, 110.00, 0.10, 0.10, 0.50, 0.15, 1.0822);
            InputTable.Rows.Add("Put", 100.00, 90.00, 0.10, 0.10, 0.50, 0.25, 12.4419);
            InputTable.Rows.Add("Put", 100.00, 100.00, 0.10, 0.10, 0.50, 0.25, 6.8014);
            InputTable.Rows.Add("Put", 100.00, 110.00, 0.10, 0.10, 0.50, 0.25, 3.3226);
            InputTable.Rows.Add("Put", 100.00, 90.00, 0.10, 0.10, 0.50, 0.35, 14.6945);
            InputTable.Rows.Add("Put", 100.00, 100.00, 0.10, 0.10, 0.50, 0.35, 9.5104);
            InputTable.Rows.Add("Put", 100.00, 110.00, 0.10, 0.10, 0.50, 0.35, 5.8223);            
        }

        public void CalculatePrice()
        {
            OptionTable.Clear();
            foreach(DataRow row in InputTable.Rows)
            {
                var optionType = row["OptionType"].ToString() == "Call" ? OptionType.CALL : OptionType.PUT;
                var price = OptionHelper.American_BaroneAdesiWhaley(
                        optionType,
                        Convert.ToDouble(row["Spot"]),
                        Convert.ToDouble(row["Strike"]),
                        Convert.ToDouble(row["Rate"]),
                        Convert.ToDouble(row["DivYield"]),
                        Convert.ToDouble(row["Maturity"]),
                        Convert.ToDouble(row["Vol"]));
                OptionTable.Rows.Add(row["BAWValue"], price);                    
            }
        }
    }
}
