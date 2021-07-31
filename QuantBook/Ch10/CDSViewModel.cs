using Caliburn.Micro;
using QLNet;
using QuantBook.Models;
using QuantBook.Models.FixedIncome;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Screen = Caliburn.Micro.Screen;

namespace QuantBook.Ch10
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class CDSViewModel : Screen
    {
        [ImportingConstructor]
        public CDSViewModel()
        {
            DisplayName = "04. CDS Pricing";
            Table1 = new DataTable();
            Table2 = new DataTable();
            LineSeriesCollection1 = new BindableCollection<Series>();
            LineSeriesCollection2 = new BindableCollection<Series>();

            EvalDate = new DateTime(2015, 3, 20);
            Spreads = "34.93,53.60,72.02,106.39,129.39,139.46";
            Tenors = "1Y,2Y,3Y,5Y,7Y,10Y";
            RecoveryRate = 0.4;
            //InitializeCds1();
            InitializeCds2();
        }
        private void InitializeCds1()
        {
            EvalDate = new DateTime(2009, 6, 15);
            EffectiveDate = new DateTime(2009, 3, 20);
            Maturity = new DateTime(2014, 6, 20);
            Spreads = "10";
            Tenors = "5Y";
            RecoveryRate = 0.4;
            CdsCoupon = 100;
            Notional = 10_000;
            ProtectionSide = "Buyer";
        }

        private void InitializeCds2()
        {
            EvalDate = new DateTime(2015, 5, 15);
            EffectiveDate = new DateTime(2015, 3, 20);
            Maturity = new DateTime(2018, 6, 20);
            Spreads = "34.93,53.60,72.02,106.39,129.39,139.46";
            Tenors = "1Y,2Y,3Y,5Y,7Y,10Y";
            RecoveryRate = 0.4;
            CdsCoupon = 100;
            Notional = 10_000;
            ProtectionSide = "Buyer";
        }
        public BindableCollection<Series> LineSeriesCollection1 { get; set; }
        public BindableCollection<Series> LineSeriesCollection2 { get; set; }


        private string protectionSide;

        public string ProtectionSide
        {
            get { return protectionSide; }
            set { protectionSide = value; NotifyOfPropertyChange(() => ProtectionSide); }
        }

        private int notional;

        public int Notional
        {
            get { return notional; }
            set { notional = value; NotifyOfPropertyChange(() => Notional); }
        }


        private double cdsCoupon;

        public double CdsCoupon
        {
            get { return cdsCoupon; }
            set { cdsCoupon = value; NotifyOfPropertyChange(() => CdsCoupon); }
        }


        private DateTime maturity;

        public DateTime Maturity
        {
            get { return maturity; }
            set { maturity = value; NotifyOfPropertyChange(() => Maturity); }
        }


        private DateTime effectiveDate;

        public DateTime EffectiveDate
        {
            get { return effectiveDate; }
            set { effectiveDate = value; NotifyOfPropertyChange(() => EffectiveDate); }
        }


        private DateTime evalDate;

        public DateTime EvalDate
        {
            get { return evalDate; }
            set { evalDate = value; NotifyOfPropertyChange(() => EvalDate); }
        }

        private string spreads;

        public string Spreads
        {
            get { return spreads; }
            set { spreads = value; NotifyOfPropertyChange(() => Spreads); }
        }

        private string tenors;

        public string Tenors
        {
            get { return tenors; }
            set { tenors = value; NotifyOfPropertyChange(() => Tenors); }
        }

        private double recoveryRate;

        public double RecoveryRate
        {
            get { return recoveryRate; }
            set { recoveryRate = value; NotifyOfPropertyChange(() => RecoveryRate); }
        }

        private DataTable table1;

        public DataTable Table1
        {
            get { return table1; }
            set { table1 = value; NotifyOfPropertyChange(() => Table1); }
        }

        private DataTable table2;

        public DataTable Table2
        {
            get { return table2; }
            set { table2 = value; NotifyOfPropertyChange(() => table2); }
        }

        private string title1 = string.Empty;

        public string Title1
        {
            get { return title1; }
            set { title1 = value; NotifyOfPropertyChange(() => Title1); }
        }

        private string xLabel1 = string.Empty;

        public string XLabel1
        {
            get { return xLabel1; }
            set { xLabel1 = value; NotifyOfPropertyChange(() => XLabel1); }
        }

        private string yLabel1 = string.Empty;

        public string YLabel1
        {
            get { return yLabel1; }
            set { yLabel1 = value; NotifyOfPropertyChange(() => YLabel1); }
        }

        private string title2 = string.Empty;

        public string Title2
        {
            get { return title2; }
            set { title2 = value; NotifyOfPropertyChange(() => Title2); }
        }

        private string xLabel2 = string.Empty;

        public string XLabel2
        {
            get { return xLabel2; }
            set { xLabel2 = value; NotifyOfPropertyChange(() => XLabel2); }
        }

        private string yLabel2 = string.Empty;

        public string YLabel2
        {
            get { return yLabel2; }
            set { yLabel2 = value; NotifyOfPropertyChange(() => YLabel2); }
        }

        public void HazardRate()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();
            FillHeader(dt1);
            FillHeader(dt2);

            double[] spreads = new[] { 34.93, 53.60, 72.02, 106.39, 129.39, 139.46 }; 
            string[] tenors = new[] { "1Y", "2Y", "3Y", "5Y", "7Y", "10Y" };

            FillTable(dt1, QuantLibFIHelper.CdsHazardRate(EvalDate, spreads, tenors, 0.4, ResultType.FromInputMaturities));
            FillTable(dt2, QuantLibFIHelper.CdsHazardRate(EvalDate, spreads, tenors, 0.4, ResultType.MonthlyResults));

            Table1 = dt1;
            Table2 = dt2;
            AddCharts();

            void FillHeader(DataTable dt)
            {
                dt.Columns.AddRange(new[]
                {
                    new DataColumn("Maturity", typeof(string)),
                    new DataColumn("TimesToMaturity", typeof(string)),
                    new DataColumn("Hazard Rate (%)", typeof(string)),
                    new DataColumn("Survival Probability (%)", typeof(string)),
                    new DataColumn("Default Probability (%)", typeof(string)),                    
                });              
            }

            void FillTable(DataTable dt, List<(Date evalDate, double timesToMaturity, double hazardRate, double survivalProbability, double defaultProbability)> rows)
            {
                foreach(var row in rows)
                {
                    dt.Rows.Add(row.evalDate, row.timesToMaturity, row.hazardRate, row.survivalProbability, row.defaultProbability);
                }
            }
        }

        private void AddCharts()
        {
            Title1 = "Hazard Rate";
            XLabel1 = "Maturity (Years)";
            YLabel1 = "Hazard Rate (%)";
            Title2 = "Survival Probability";
            XLabel2 = "Maturity (Years)"; 
            YLabel2 = "Survival Probability (%)";            
            LineSeriesCollection1.Clear();
            LineSeriesCollection1.Add(new Series
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Hazard Rate (%)"
            });
            LineSeriesCollection2.Clear();
            LineSeriesCollection2.Add(new Series
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Survival Probability (%)",
                Name= "Survival Probability (%)"

            });
            LineSeriesCollection2.Add(new Series
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Default Probability (%)",
                Name = "Default Probability (%)",
            });
        }

        public void StartCdsPV()
        {
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Name", typeof(string)),
                new DataColumn("Value", typeof(string)),                
            });
            var result = QuantLibFIHelper.CdsPv(protectionSide.ToSide(), "USD", EvalDate, EffectiveDate, Maturity, RecoveryRate, Spreads.Split(','), Tenors.Split(','), Notional, Frequency.Quarterly, CdsCoupon); ;

            dt.Rows.Add("Maturity", Maturity);
            dt.Rows.Add("Coupoon", CdsCoupon);
            dt.Rows.Add("PresentValue", result.npv);
            dt.Rows.Add("FairSpread", result.fairSpread);
            dt.Rows.Add("HazardRate", result.hazardRate);
            dt.Rows.Add("DefaultProbability", result.defaultProbability);
            dt.Rows.Add("SurvivalProbability", result.survivalpProbability);

            Table1 = dt; 
        }


        public void StartCdsPrice()
        {
            LineSeriesCollection1.Clear();
            LineSeriesCollection2.Clear();

            var cds = QuantLibFIHelper.CdsPrice(ProtectionSide.ToSide(), "USD", EvalDate, EffectiveDate, Maturity, RecoveryRate, Spreads.Split(','), Tenors.Split(','), Frequency.Quarterly, CdsCoupon);
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Name", typeof(string)),
                new DataColumn("Value", typeof(string)),
            });
            dt.Rows.Add("Accrual", cds.accrual);
            dt.Rows.Add("Upfront", cds.upfront);
            dt.Rows.Add("CleanPrice", cds.cleanPrice);
            dt.Rows.Add("DirtyPrice", cds.dirtyPrice);
            dt.Rows.Add("RiskyAnnuity", cds.dv01);
            Table2 = dt;
        }

    }
}
