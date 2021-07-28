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

namespace QuantBook.Ch10
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class SimpleBondViewModel : Screen
    {
        [ImportingConstructor]
        public SimpleBondViewModel()
        {
            DisplayName = "01. Simple Bonds";
            IssueDate = new DateTime(2015, 12, 16);
            EvalDate = new DateTime(2015, 12, 16);
            SelectedFrequency = QLNet.Frequency.Annual;
            BondTable = new DataTable();
        }

        private DateTime evalDate;

        public DateTime EvalDate
        {
            get { return evalDate; }
            set { evalDate = value; NotifyOfPropertyChange(() => EvalDate); }
        }

        private DateTime issueDate;

        public DateTime IssueDate
        {
            get { return issueDate; }
            set { issueDate = value; NotifyOfPropertyChange(() => IssueDate); }
        }

        private int settlementDays = 1;

        public int SettlementDays
        {
            get { return settlementDays; }
            set { settlementDays = value; NotifyOfPropertyChange(() => SettlementDays); }
        }

        private double faceValue = 1000;

        public double FaceValue
        {
            get { return faceValue; }
            set { faceValue = value; NotifyOfPropertyChange(() => FaceValue); }
        }

        private double discountRate = 0.06;

        public double DiscountRate
        {
            get { return discountRate; }
            set { discountRate = value; NotifyOfPropertyChange(() => FaceValue); }
        }

        private int timesToMaturity = 10;

        public int TimesToMaturity
        {
            get { return timesToMaturity; }
            set { timesToMaturity = value; NotifyOfPropertyChange(() => TimesToMaturity); }
        }

        private string coupons = "0.05";

        public string Coupons
        {
            get { return coupons; }
            set { coupons = value; NotifyOfPropertyChange(() => Coupons); }
        }

        private DataTable bondTable;

        public DataTable BondTable
        {
            get { return bondTable; }
            set { bondTable = value;  NotifyOfPropertyChange(() => BondTable); }
        }

        private IEnumerable<Frequency> frequency;

        public IEnumerable<Frequency> Frequency
        {
            get { return Enum.GetValues(typeof(Frequency)).Cast<Frequency>(); }
            set { frequency = value; NotifyOfPropertyChange(() => Frequency);  }
        }

        private Frequency selectedFrequency;

        public Frequency SelectedFrequency
        {
            get { return selectedFrequency; }
            set { selectedFrequency = value; NotifyOfPropertyChange(() => SelectedFrequency); }
        }

        public void Start()
        {
            DateTime maturity = IssueDate.AddYears(TimesToMaturity);
            double[] coupons = Coupons.Split(',').Select(c => Convert.ToDouble(c)).ToArray();
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Name", typeof(string)),
                new DataColumn("Value", typeof(string))
            });

            (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) = QuantLibFIHelper.BondPrice(evalDate, issueDate, maturity, settlementDays, FaceValue, DiscountRate, coupons.First(), SelectedFrequency);

            dt.Rows.Add("Pricing Bond", "");
            dt.Rows.Add("Issue Date", issueDate);
            dt.Rows.Add("Evaluation Date", EvalDate);
            dt.Rows.Add("Times to Maturity in Years", TimesToMaturity);
            dt.Rows.Add("Face Value", FaceValue);
            dt.Rows.Add("Discount Rate", DiscountRate);
            dt.Rows.Add("Coupon", Coupons);
            dt.Rows.Add("Present Value", npv);
            dt.Rows.Add("Clean Price", cprice);
            dt.Rows.Add("Dirty Price", dprice);
            dt.Rows.Add("Accrued Value", accrued);
            dt.Rows.Add("YTM", ytm);
            BondTable = dt;
        }

        public void StartCurveRate()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new[]
        {
                new DataColumn("Name", typeof(string)),
                new DataColumn("Value", typeof(string))
            });

            (double? npv, double? cprice, double? dprice, double? accrued, double? ytm) = QuantLibFIHelper.BondPriceCurveRate();

            dt.Rows.Add("Pricing Bond", "USING QuantLib");
            dt.Rows.Add("Issue Date", "1/15/2015");
            dt.Rows.Add("Evaluation Date", "1/15/2015");
            dt.Rows.Add("Times to Maturity in Years", "2");
            dt.Rows.Add("Face Value", "100");
            dt.Rows.Add("Discount Rate", "0.004, 0.006, 0.0065, 0.007");
            dt.Rows.Add("Coupon", "5%");
            dt.Rows.Add("Present Value", npv);
            dt.Rows.Add("Clean Price", cprice);
            dt.Rows.Add("Dirty Price", dprice);
            dt.Rows.Add("Accrued Value", accrued);
            dt.Rows.Add("YTM", ytm);
            BondTable = dt;
        }
    }
}
