using Caliburn.Micro;
using QLNet;
using QuantBook.Models.FixedIncome;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace QuantBook.Ch10
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ZeroCouponViewModel : Screen
    {
        [ImportingConstructor]
        public ZeroCouponViewModel()
        {
            DisplayName = "02. Zero Coupon";

            ZcTable1 = new DataTable();
            ZcTable2 = new DataTable();
            LineSeriesCollection1 = new BindableCollection<Series>();
            LineSeriesCollection2 = new BindableCollection<Series>();
        }

        public BindableCollection<Series> LineSeriesCollection1 { get; set; }
        public BindableCollection<Series> LineSeriesCollection2 { get; set; }

        private DataTable zcTable1;

        public DataTable ZcTable1
        {
            get { return zcTable1; }
            set { zcTable1 = value; NotifyOfPropertyChange(() => ZcTable1); }
        }

        private DataTable zcTable2;

        public DataTable ZcTable2
        {
            get { return zcTable2; }
            set { zcTable2 = value; NotifyOfPropertyChange(() => ZcTable2); }
        }


        private double zSpread = 50;

        public double ZSpread
        {
            get { return zSpread = 50; }
            set { zSpread = value; NotifyOfPropertyChange(() => ZSpread); }
        }

        public void StartZeroCoupon0()
        {
            var dt = new DataTable();
            FillColumnHeaders(dt);

            Date evalDate = new Date(15, Month.Jan, 2015);
            Date[] maturities = new Date[]
            {
                new Date(15, Month.January, 2016),
                new Date(15, Month.January, 2017),
                new Date(15, Month.January, 2018),
                new Date(15, Month.January, 2019),
            };
            var faceAmount = 100.0;
            var coupons = new List<double> { 0.05, 0.055, 0.05, 0.06 };
            var bondPrices = new List<double> { 101.0, 101.5, 99.0, 100.0 };
            FillDataTable(dt, QuantLibFIHelper.ZeroCouponDirect(faceAmount, evalDate, coupons, bondPrices, maturities));
            ZcTable1 = dt;

            void FillDataTable(DataTable table, List<(DateTime maturity, double couponRate, double equivalentRate, double discountRate)> rows)
            {
                foreach (var row in rows)
                {
                    table.Rows.Add(row.maturity, row.couponRate, row.equivalentRate, row.discountRate);
                }
            }

            void FillColumnHeaders(DataTable table)
            {
                table.Columns.AddRange(new[]
                {
                    new DataColumn("Maturity", typeof(string)),
                    new DataColumn("Zero Coupon Rate: R", typeof(string)),
                    new DataColumn("Equivalent Rate: Rc", typeof(string)),
                    new DataColumn("Discount Rate: B", typeof(string))
                });
            }
        }

        public void StartZeroCoupon1()
        {
            const double faceAmount = 100.0;
            Date evalDate = new Date(15, Month.January, 2015);
            var depositRates = new double[] { 0.044, 0.045, 0.046, 0.047, 0.049, 0.051, 0.053 };
            var depositMaturities = new Period[]
            {
                new Period(1, TimeUnit.Days),
                new Period(1, TimeUnit.Months),
                new Period(2, TimeUnit.Months),
                new Period(3, TimeUnit.Months),
                new Period(6, TimeUnit.Months),
                new Period(9, TimeUnit.Months),
                new Period(12, TimeUnit.Months),
            };
            double[] bondCoupons = new double[] { 0.05, 0.06, 0.055, 0.05 };
            double[] bondPrices = new double[] { 99.55, 100.55, 99.5, 97.6 };
            var bondMaturities = new Period[]
            {
                new Period(14, TimeUnit.Months),
                new Period(21, TimeUnit.Months),
                new Period(2, TimeUnit.Years),
                new Period(3, TimeUnit.Years),
            };

            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();
            FillHeaders(dt1);
            FillHeaders(dt2);
            
            FillDataTable(dt1, QuantLibFIHelper.ZeroCouponBootstrap(faceAmount, evalDate, depositRates, depositMaturities, bondPrices, bondCoupons, bondMaturities, ResultType.FromInputMaturities));
            FillDataTable(dt2, QuantLibFIHelper.ZeroCouponBootstrap(faceAmount, evalDate, depositRates, depositMaturities, bondPrices, bondCoupons, bondMaturities, ResultType.MonthlyResults));

            ZcTable1 = dt1;
            ZcTable2 = dt2;

            AddCharts();           
        }

        private void AddCharts()
        {
            LineSeriesCollection1.Clear();            
            LineSeriesCollection1.Add(new Series()
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Zero Coupon Rate: R",
                Name = "R"
            });
            LineSeriesCollection1.Add(new Series()
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Equivalent Rate: Rc",
                Name = "Rc"
            });

            LineSeriesCollection2.Clear();
            LineSeriesCollection2.Add(new Series()
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Discount Rate: B",
                Name="B"
            });
        }

        private void AddChartsWithZSpread()
        {
            LineSeriesCollection1.Clear();
            LineSeriesCollection1.Add(new Series()
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Zero Coupon Rate: R",
                Name = "R"
            });
            LineSeriesCollection1.Add(new Series()
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Zero Coupon Rate: R with ZSpread",
                Name = "R with ZSpread"
            });

            LineSeriesCollection2.Clear();
            LineSeriesCollection2.Add(new Series()
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Discount Rate: B",
                Name= "B"
            });
            LineSeriesCollection2.Add(new Series()
            {
                ChartType = SeriesChartType.Line,
                XValueMember = "TimesToMaturity",
                YValueMembers = "Discount Rate: B with ZSpread",
                Name = "B with ZSpread"
            });
        }

        private static void FillDataTable(DataTable table, List<(Date maturity, double years, double zeroRate, double eqRate, double discount)> rows)
        {
            foreach (var row in rows)
            {
                table.Rows.Add(row.maturity, row.years, row.zeroRate, row.eqRate, row.discount);
            }
        }

        private static void FillHeaders(DataTable table)
        {
            table.Columns.AddRange(new[]
            {
                    new DataColumn("Maturity", typeof(string)),
                    new DataColumn("TimesToMaturity", typeof(string)),
                    new DataColumn("Zero Coupon Rate: R", typeof(string)),
                    new DataColumn("Equivalent Rate: Rc", typeof(string)),
                    new DataColumn("Discount Rate: B", typeof(string))
                });
        }

        private static void FillDataTableWithZSpread(DataTable table, List<(Date maturity, double years, double zeroRate, double zeroRateZSpreaded, double discount, double discountZSpreaded)> rows)
        {
            foreach (var row in rows)
            {
                table.Rows.Add(row.maturity, row.years, row.zeroRate, row.zeroRateZSpreaded, row.discount, row.discountZSpreaded);
            }
        }

        private static void FillHeadersWithZSpread(DataTable table)
        {
            table.Columns.AddRange(new[]
            {
                    new DataColumn("Maturity", typeof(string)),
                    new DataColumn("TimesToMaturity", typeof(string)),
                    new DataColumn("Zero Coupon Rate: R", typeof(string)),
                    new DataColumn("Zero Coupon Rate: R with ZSpread", typeof(string)),
                    new DataColumn("Discount Rate: B", typeof(string)),
                    new DataColumn("Discount Rate: B with ZSpread", typeof(string))
                });
        }

        public void StartInterbank()
        {
            var settlementDate = new Date(18, 2, 2015);
            var depositRates = new double[] { 0.001375, 0.001717, 0.002112, 0.002581 };
            var depositMaturities = new Period[]
            {
                new Period(1, TimeUnit.Weeks),
                new Period(1, TimeUnit.Months),
                new Period(2, TimeUnit.Months),
                new Period(3, TimeUnit.Months)
            };
            double[] futurePrices = new double[] { 99.725, 99.585, 99.385, 99.16, 98.93, 98.715 };
            double[] swapRates = new double[] { 0.0089268, 0.0123343, 0.0147985, 0.0165843, 0.0179191 };
            var swapMaturities = new Period[]
            {
                new Period(2, TimeUnit.Years),
                new Period(3, TimeUnit.Years),
                new Period(4, TimeUnit.Years),
                new Period(5, TimeUnit.Years),
                new Period(6, TimeUnit.Years)
            };
            DataTable dt = new DataTable();
            FillHeaders(dt);
            FillDataTable(dt, QuantLibFIHelper.InterbankZeroCoupon(settlementDate, depositRates, depositMaturities, futurePrices, swapRates, swapMaturities));

            ZcTable2 = dt;
            AddCharts();            
        }

        public void StartZSpread()
        {
            var depositRates = new double[] { 0.0525, 0.055 };
            var depositMaturities = new Period[]
            {
                new Period(6, TimeUnit.Months),
                new Period(12, TimeUnit.Months),
            };
            double[] bondCoupons = new double[] { 0.0575, 0.06, 0.0625, 0.065, 0.0675, 0.068, 0.07, 0.071, 0.0715, 0.072, 0.073, 0.0735, 0.074, 0.075, 0.076, 0.076, 0.077, 0.078 };
            double[] bondPrices = new double[bondCoupons.Length];
            for (int i = 0; i < bondCoupons.Length; i++)
            {
                bondPrices[i] = 100.0;
            }
            Period[] bondMaturities = new Period[bondCoupons.Length];
            for (int i = 0; i < bondCoupons.Length; i++)
            {
                bondMaturities[i] = new Period((i + 3) * 6, TimeUnit.Months);
            }
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            FillHeadersWithZSpread(dt1);
            FillHeadersWithZSpread(dt2);
            FillDataTableWithZSpread(dt1, QuantLibFIHelper.ZeroCouponBootstrapZspread(depositRates, depositMaturities, bondPrices, bondCoupons, bondMaturities, zSpread));
            FillDataTableWithZSpread(dt2, QuantLibFIHelper.ZeroCouponBootstrapZspread(depositRates, depositMaturities, bondPrices, bondCoupons, bondMaturities, zSpread));

            ZcTable1 = dt1;
            ZcTable2 = dt2;
            AddChartsWithZSpread();
        }
    }
}
