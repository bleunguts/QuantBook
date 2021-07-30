using System;
using System.Collections.Generic;
using Caliburn.Micro;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace QuantBook.Models.Isda
{
    public class IsdaHelper
    {
        private const string url_isda = "https://www.markit.com/news/InterestRates_{0}_{1}.zip";

        public static BindableCollection<IsdaRate> GetIsdaRates(string currency, DateTime startDate, DateTime endDate, IEventAggregator events = null)
        {
            BindableCollection<IsdaRate> res = new BindableCollection<IsdaRate>();

            DateTime date = startDate;
            List<object> objList = new List<object>();

            int totalRuns = Utilities.get_number_calendar_days(startDate, endDate);
            objList.Add("Ready...");
            objList.Add(0);
            objList.Add(totalRuns);
            objList.Add(0);

            int count = 0;
            while (date <= endDate)
            {
                var rates = GetIsdaRatesSingleDay(currency, date);
                if(rates!=null)
                {
                    if (rates.Count > 0)
                        res.AddRange(rates);
                }

                if (events != null)
                {
                    objList[0] = string.Format("Total Runs = {0}, Count = {1}, Currency = {2}, Date = {3}", totalRuns, count, currency, date);
                    objList[3] = count;
                    events.PublishOnUIThread(new ModelEvents(objList));
                }

                date = date.AddDays(1);
                count++;
            }

            if (events != null)
            {
                objList[0] = "Ready...";
                objList[1] = 0;
                objList[2] = 1;
                objList[3] = 0;
                events.PublishOnUIThread(new ModelEvents(objList));
            }

            return res;
        }

        private static BindableCollection<IsdaRate> GetIsdaRatesSingleDay(string currency, DateTime date)
        {
            BindableCollection<IsdaRate> rates = new BindableCollection<IsdaRate>();

            string url = string.Format(url_isda, currency, date.ToString("yyyyMMdd"));
            string zpath = @"..\..\Models\DataModel\Isda\Zip\";
            string zfile = zpath + currency + "_" + date.ToString("yyyyMMdd") + ".zip";

            string xfile = zpath + "InterestRates_" + currency + "_" + date.ToString("yyyyMMdd") + ".xml";
           
            foreach (string file in Directory.GetFiles(zpath))
                File.Delete(file);           

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(url, zfile);
            }

            if (!File.Exists(zfile))
                return rates;

            try
            {
                ZipFile.ExtractToDirectory(zfile, zpath);
            }
            catch { }

            if (!File.Exists(xfile))
                return rates;

            XDocument doc = XDocument.Load(xfile);
            XElement asof = doc.Root.Element("effectiveasof");
            XElement badday = doc.Root.Element("baddayconvention");

           
            //Process deposits:
            XElement deposits = doc.Root.Element("deposits");
            string dayConvention = deposits.Element("daycountconvention").Value;
            string calendar = deposits.Element("calendars").Element("calendar").Value;
            string[] ss = deposits.Element("snaptime").Value.Split('T');
            string ts = ss[0] + " " + ss[1].Split('Z')[0];
            DateTime snapDate = Convert.ToDateTime(ts);
            string spotDate =deposits.Element("spotdate").Value;
            foreach(var e in deposits.Elements())
            {
                if(e.Name.ToString() == "curvepoint")
                {
                    rates.Add(new IsdaRate
                    {
                        Currency=currency,
                        EffectiveAsOf = asof.Value,
                        BadDayConvention = badday.Value,
                        Calendar = calendar,
                        SnapTime = snapDate,
                        SpotDate = spotDate,
                        Maturity = e.Element("maturitydate").Value,
                        DayCountConvention = dayConvention,
                        Tenor = e.Element("tenor").Value,
                        Rate = e.Element("parrate").Value
                    });
                }
            }

            //Process swaps:
            XElement swaps = doc.Root.Element("swaps");
            dayConvention = swaps.Element("floatingdaycountconvention").Value;
            calendar = swaps.Element("calendars").Element("calendar").Value;
            string fixDayConvention = swaps.Element("fixeddaycountconvention").Value;
            ss = swaps.Element("snaptime").Value.Split('T');
            ts = ss[0] + " " + ss[1].Split('Z')[0];
            snapDate = Convert.ToDateTime(ts);
            spotDate = swaps.Element("spotdate").Value;
            string floatPay = swaps.Element("floatingpaymentfrequency").Value;
            string fixPay = swaps.Element("fixedpaymentfrequency").Value;

            foreach (var e in swaps.Elements())
            {
                if (e.Name.ToString() == "curvepoint")
                {
                    rates.Add(new IsdaRate
                    {
                        Currency = currency,
                        EffectiveAsOf = asof.Value,
                        BadDayConvention = badday.Value,
                        Calendar = calendar,
                        SnapTime = snapDate,
                        SpotDate = spotDate,
                        Maturity = e.Element("maturitydate").Value,
                        DayCountConvention = dayConvention,
                        FixedDayCountConvention = fixDayConvention,
                        FloatingPaymentFrequency = floatPay,
                        FixedPaymentFrequency = fixPay,
                        Tenor = e.Element("tenor").Value,
                        Rate = e.Element("parrate").Value
                    });
                }
            }

            return rates;
        }
        
    }
}
