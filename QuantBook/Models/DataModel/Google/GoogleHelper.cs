using System;
using System.Linq;
using System.Collections.Generic;
using Caliburn.Micro;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using QuantBook.Models.DataModel.Google;

namespace QuantBook.Models.Google
{
    public class GoogleHelper
    {
        private const string url_hist = "http://www.google.com/finance/getprices?q={0}&i={1}&p={2}&f=d,c,h,l,o,v";
        private const string url_quote = "http://www.google.com/finance/info?infotype=infoquoteall&q={0}";

        public static BindableCollection<GoogleStockPrice> GetGoogleStockData(string ticker, DateTime startDate, DateTime endDate, GoogleDataTypeEnum dataType, IEventAggregator events = null)
        {
            int interval = 0;
            if (dataType == GoogleDataTypeEnum.EOD)
                interval = 86400;
            else if (dataType == GoogleDataTypeEnum.BarMinute)
                interval = 60;
            else if (dataType == GoogleDataTypeEnum.Bar5Minutes)
                interval = 5 * 60;
            else if (dataType == GoogleDataTypeEnum.Bar10Minutes)
                interval = 10 * 60;
            else if (dataType == GoogleDataTypeEnum.Bar15Minutes)
                interval = 15 * 60;
            else if (dataType == GoogleDataTypeEnum.Bar30Minutes)
                interval = 30 * 60;
            else if (dataType == GoogleDataTypeEnum.Bar60Minutes)
                interval = 60 * 60;
            return GetGoogleStockData(ticker, startDate, endDate, interval, events);
        }

        private static BindableCollection<GoogleStockPrice> GetGoogleStockData(string ticker, DateTime startDate, DateTime endDate, int interval, IEventAggregator events = null)
        {
            string period = GetPeriod(startDate, endDate);
            string url = string.Format(url_hist, ticker, interval, period);
            if (string.IsNullOrEmpty(url))
                return null;

            string data;
            using (WebClient client = new WebClient())
            {
                data = client.DownloadString(url);
            }

            BindableCollection<GoogleStockPrice> stock = new BindableCollection<GoogleStockPrice>();
            string message = string.Empty;
            using (MemoryStream ms = new MemoryStream(System.Text.Encoding.Default.GetBytes(data)))
            {
                ProcessStockData(ticker, startDate, endDate, data, stock, ms, interval, out message);
            }

            int days = ModelHelper.get_number_workdays(Convert.ToDateTime(stock[0].Date).Date, Convert.ToDateTime(stock[stock.Count - 1].Date).Date);

            message += string.Format(" Number of week days = {0}, number of records = {1}", days, stock.Count);
            List<object> x = new List<object>();
            x.Add(message);
            if (events != null)
                events.PublishOnUIThread(new ModelEvents(x));
            return stock;
        }


        private static void ProcessStockData(string ticker, DateTime startDate, DateTime endDate, string data, BindableCollection<GoogleStockPrice> stock, Stream stream, int interval, out string message)
        {
            message = string.Empty;
            StreamReader sr = new StreamReader(stream);
            DateTime dateLast = new DateTime();
            DateTime datePrevious = new DateTime();
            bool processData = false;
            string line;
            int numLines = 0;
            bool processOneLine = false;

            while ((line = sr.ReadLine()) != null)
            {
                numLines++;

                if (!processData)
                {
                    if (line.StartsWith("a"))
                    {
                        processData = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (line.StartsWith("TIMEZONE_OFFSET="))
                {
                    continue;
                }

                string[] items = null;

                if (!GetItems(line, numLines, ref items, out message))
                {
                    return;
                }

                DateTime date = new DateTime();
                if (items[(int)Fields.date].StartsWith("a"))
                {
                    date = dateLast = ConvertUnixTimestamp(Convert.ToDouble(items[0].Substring(1)));
                }
                else
                {
                    int timeAdd;
                    if (!int.TryParse(items[0], out timeAdd))
                    {
                        message = string.Format("Invalid line {0}: {1}", numLines, line);
                        return;
                    }
                   
                    date = dateLast.AddSeconds(Convert.ToInt32(items[0]) * interval);
                }

                if (date.Date == datePrevious)
                {
                    continue;
                }

                if (date >= startDate && date <= endDate.AddHours(24))
                {
                    stock.Add(new GoogleStockPrice()
                    {
                        Ticker = ticker,
                        Date = date,
                        PriceOpen = items[(int)Fields.open],
                        PriceHigh = items[(int)Fields.high],
                        PriceLow = items[(int)Fields.low],
                        PriceClose = items[(int)Fields.close],
                        Volume = items[(int)Fields.volume]
                    });
                }

                datePrevious = date;

                if (!processOneLine)
                    processOneLine = true;
            }

            if (!processOneLine)
            {
                message = "No Data.";
                return;
            }
        }

        private enum Fields
        {
            date = 0,
            close = 1,
            open = 4,
            high = 2,
            low = 3,
            volume = 5
        };

        private static string GetPeriod(DateTime startDate, DateTime endDate)
        {
            if (endDate.Date < startDate.Date)
            {
                throw new ArgumentException("The ending date can't be lower than the starting date.");
            }

            if (endDate.Year > startDate.Year)
            {
                return (endDate.Year - startDate.Year + 1) + "Y";
            }

            TimeSpan span = endDate - startDate;

            if (span.Days > 50)
            {
                return ((int)(span.Days / 25) + 1) + "M";
            }
            else
            {
                return (span.Days + 1) + "d";
            }
        }
   
        private static DateTime ConvertUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        private static bool GetItems(string line, int counterOfLines, ref string[] items, out string message)
        {
            message = String.Empty;
            if (String.IsNullOrEmpty(line))
                throw new ArgumentException("'line' empty or null.");

            items = line.Split(',');

            bool returnValue = items != null && items.Length == 6;
            if (!returnValue)
            {
                message = "Unexpected number of fields at line '" + counterOfLines + "'.";
            }
            return returnValue;
        }








        public static void GetGoogleStockQuote(BindableCollection<GoogleQuote> quotes)
        {
            string tks = string.Join(",", quotes.Select(x => x.Symbol).ToArray());
            string url = string.Format(url_quote, tks);

            string json = string.Empty;
            using (WebClient client = new WebClient())
            {
                try
                {
                    json = client.DownloadString(url);
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(json))
            {
                json = json.Replace("//", "");
                XDocument doc = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(new MemoryStream(Encoding.ASCII.GetBytes(json)), new XmlDictionaryReaderQuotas()));
                XElement results = doc.Element("root");
                foreach (var item in results.Elements("item"))
                {
                    try
                    {
                        var quote = from q in quotes where q.Symbol == item.Element("t").Value select q;
                        
                        foreach(var p in quote)
                        {
                            p.Name = item.Element("name").Value;                            
                            p.LastTradeTime = item.Element("lt").Value;
                            p.TradeTimeDetail = item.Element("lt_dts").Value;
                            p.LastQuotePrice = item.Element("l").Value;
                            p.LastTradePrice = item.Element("l_cur").Value;
                            p.Open = item.Element("op").Value;
                            p.PreviousClose = item.Element("pcls_fix").Value;
                            p.StockExchange = item.Element("e").Value;
                            p.Volume = item.Element("vo").Value;
                            p.DailyLow = item.Element("lo").Value;
                            p.DailyHigh = item.Element("hi").Value;
                            p.Change = item.Element("c").Value;
                            p.PercentChange = item.Element("cp").Value;
                            p.YearlyLow = item.Element("lo52").Value;
                            p.YearlyHigh = item.Element("hi52").Value;
                            p.LastUpdate = DateTime.Now.ToString();
                        }
                    }
                    catch { }
                }
            }
        }
    }

    




    public enum GoogleDataTypeEnum
    {
        EOD = 0,
        BarMinute = 1,
        Bar5Minutes = 2,
        Bar10Minutes = 3,
        Bar15Minutes = 4,
        Bar30Minutes = 5,
        Bar60Minutes = 6,
    }

    public class GoogleStockPrice
    {
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public string PriceOpen { get; set; }
        public string PriceHigh { get; set; }
        public string PriceLow { get; set; }
        public string PriceClose { get; set; }
        public string Volume { get; set; }
    }
}
