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

        public void StartZeroCoupon0()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Maturity", typeof(string)),
                new DataColumn("Zero Coupono Rate: R", typeof(string)),
                new DataColumn("Equivalent Rate: Rc", typeof(string)),
                new DataColumn("Discount Rate: B", typeof(string))
            });

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

            foreach (var row in QuantLibFIHelper.ZeroCouponDirect(faceAmount, coupons, bondPrices, evalDate, maturities))
            {
                dt.Rows.Add(row.maturity, row.couponRate, row.equivalentRate, row.discountRate);
            }
            ZcTable1 = dt;
        }
    }
}
