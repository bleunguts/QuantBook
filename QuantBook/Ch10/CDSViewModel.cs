﻿using Caliburn.Micro;
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
        }
        public BindableCollection<Series> LineSeriesCollection1 { get; set; }
        public BindableCollection<Series> LineSeriesCollection2 { get; set; }

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
            throw new NotImplementedException("Can't get the quant lib to work for this one");
        }

    }
}
