using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using Caliburn.Micro;
using System;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text;

namespace QuantBook.Models.Google
{
    public static class GoogleOptions
    {
        public static XElement GetOptionData(string ticker, string expiry = null)
        {
            string json = string.Empty;
            using (var wc = new WebClient())
            {
                string url = string.Empty;

                if (!string.IsNullOrEmpty(expiry))
                {
                    string[] ss = expiry.Split('-');
                    int year = ss[0].To<int>();
                    int month = ss[1].To<int>();
                    int day = ss[2].To<int>();
                    url = string.Format("http://www.google.com/finance/option_chain?q={0}&expd={1}&expm={2}&expy={3}&output=json", ticker, day, month, year);
                }
                else
                {
                    url = string.Format("http://www.google.com/finance/option_chain?q={0}&output=json", ticker);
                }

                json = wc.DownloadString(url);
            }

            XElement element = null;
            if (!string.IsNullOrEmpty(json))
            {
                json = Regex.Replace(json, @"(\w+:)(\d+\.?\d*)", "$1\"$2\"");
                json = Regex.Replace(json, @"(\w+):", "\"$1\":");

                XDocument doc = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(new MemoryStream(Encoding.ASCII.GetBytes(json)), new XmlDictionaryReaderQuotas()));
                element = doc.Element("root");
            }
            return element;
        }

        public static BindableCollection<OptionExpiration> GetExpiries(XElement element)
        {
            BindableCollection<OptionExpiration> res = new BindableCollection<OptionExpiration>();
            foreach (var item in element.Element("expirations").Elements("item"))
            {
                string ss = string.Format("{0}-{1}-{2}", item.Element("y").Value, item.Element("m").Value, item.Element("d").Value);
                res.Add(new OptionExpiration(ss));
            }
            return res;
        }

        public static BindableCollection<OptionResults> GetOptionResults(XElement element, string ticker)
        {
            BindableCollection<OptionResults> res = new BindableCollection<OptionResults>();
          
            XElement puts =element.Element("puts");
            XElement calls =element.Element("calls");
            string stock = element.Element("underlying_price").Value;

            foreach(var item in calls.Elements("item"))
            {
                res.Add(new OptionResults
                {
                    Ticker = ticker,
                    Exchange = item.Element("e").Value,
                    StockPrice = stock,
                    Expiry = item.Element("expiry").Value.To<DateTime>(),
                    Strike = item.Element("strike").Value.To<decimal>(),
                    CallName = item.Element("s").Value,
                    CallPrice = item.Element("p").Value,
                    CallBid = item.Element("b").Value,
                    CallAsk = item.Element("a").Value,
                    CallVolume = item.Element("vol").Value,
                    CallOpenInt = item.Element("oi").Value,
                });
            }

            foreach(var item in puts.Elements("item"))
            {
                var strike = item.Element("strike").Value.To<decimal>();
                var cc = from c in res where c.Strike == strike select c;
                if(cc.Count()>0)
                {
                    foreach(var r in cc)
                    {
                        r.PutName = item.Element("s").Value;
                        r.PutPrice = item.Element("p").Value;
                        r.PutBid = item.Element("b").Value;
                        r.PutAsk = item.Element("a").Value;
                        r.PutVolume = item.Element("vol").Value;
                        r.PutOpenInt = item.Element("oi").Value;
                    }
                }
                else
                {
                    res.Add(new OptionResults
                    {
                        Ticker = ticker,
                        Exchange = item.Element("e").Value,
                        StockPrice = stock,
                        Expiry = item.Element("expiry").Value.To<DateTime>(),
                        Strike = item.Element("strike").Value.To<decimal>(),
                        PutName = item.Element("s").Value,
                        PutPrice = item.Element("p").Value,
                        PutBid = item.Element("b").Value,
                        PutAsk = item.Element("a").Value,
                        PutVolume = item.Element("vol").Value,
                        PutOpenInt = item.Element("oi").Value,
                    });
                }
            }
            return res;
        }
    }
    

    public class OptionResults
    {
        public string Ticker { get; set; }
        public string Exchange { get; set; }
        public string StockPrice { get; set; }
        public DateTime Expiry { get; set; }        
        public decimal Strike { get; set; }
        public string CallName { get; set; }
        public string CallPrice { get; set; }
        public string CallBid { get; set; }
        public string CallAsk { get; set; }
        public string CallVolume { get; set; }
        public string CallOpenInt { get; set; }
        public string PutName { get; set; }
        public string PutPrice { get; set; }
        public string PutBid { get; set; }
        public string PutAsk { get; set; }
        public string PutVolume { get; set; }
        public string PutOpenInt { get; set; }
    }

    public class OptionExpiration
    {
        public OptionExpiration(string optionExpiry)
        {
            this.OptionExpiry = optionExpiry;
        }
        public string OptionExpiry { get; set; }
    }
    
}