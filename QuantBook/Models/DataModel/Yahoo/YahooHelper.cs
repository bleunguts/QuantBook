using Caliburn.Micro;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace QuantBook.Models.DataModel.Yahoo
{
    public static class YahooHelper
    {
        public static DataTable GetYahooHistStockDataTAble(string ticker, DateTime? startDate, DateTime? endDate)
        {
            string urlTemplate = @"http://ichart.finance.yahoo.com/table.csv?s=[symbol]&a=[startMonth]&b=[startDay]&c=[startYear]&d=[endMonth]&e=[endDay]&f=[endYear]&g=d&ignore=.csv";
            if (!endDate.HasValue) endDate = DateTime.Now;
            if (!startDate.HasValue) startDate = DateTime.Now.AddYears(-5);
            if (ticker == null || ticker.Length < 1)
            {
                throw new ArgumentException("Symbol invalid: " + ticker);
            }

            urlTemplate = urlTemplate.Replace("[symbol]", ticker);
            urlTemplate = urlTemplate.Replace("[startMonth]", startDate.Value.Month.ToString());
            urlTemplate = urlTemplate.Replace("[startDay]", startDate.Value.Day.ToString());
            urlTemplate = urlTemplate.Replace("[startYear]", startDate.Value.Year.ToString());
            urlTemplate = urlTemplate.Replace("[endMonth]", endDate.Value.Month.ToString());
            urlTemplate = urlTemplate.Replace("[endDay]", endDate.Value.Day.ToString());
            urlTemplate = urlTemplate.Replace("[endYear]", endDate.Value.Year.ToString());
            var history = string.Empty;
            using (var webClient = new WebClient())
            {
                try
                {
                    history = webClient.DownloadString(urlTemplate);
                }
                catch { }
            }

            DataTable dt = new DataTable();
            history = history.Replace("\r", "");
            var rows = history.Split('\n');
            rows[0].Split(',').Select(colName => dt.Columns.Add(colName));
            for (int i = 0; i < rows.Length - 1; i++)
            {
                var values = rows[i].Split(',');
                var row = dt.NewRow();
                if (!string.IsNullOrEmpty(values.First()))
                {
                    row.ItemArray = values;
                    dt.Rows.Add(row);
                }
            }
            var results = new DataTable();
            results.Columns.Add("Ticker", typeof(string));
            results.Columns.Add("Date", typeof(DateTime));
            results.Columns.Add("Open", typeof(decimal));
            results.Columns.Add("High", typeof(decimal));
            results.Columns.Add("Low", typeof(decimal));
            results.Columns.Add("Close", typeof(decimal));
            results.Columns.Add("Volume", typeof(decimal));
            results.Columns.Add("Adj Close", typeof(decimal));

            foreach (DataRow row in dt.Rows)
            {
                var date = DateTime.ParseExact(row["Date"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1);
                results.Rows.Add(ticker, date.ToShortDateString(),
                    Convert.ToDecimal(row["Open"]),
                    Convert.ToDecimal(row["High"]),
                    Convert.ToDecimal(row["Low"]),
                    Convert.ToDecimal(row["Close"]),
                    Convert.ToDecimal(row["Volume"]),
                    Convert.ToDecimal(row["Adj Close"]));
            }
            return results;
        }

        private const string url_quotes = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20yahoo.finance.quotes%20where%20symbol%20in%20({0})&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";

        public static DataTable GetQuotesTable(BindableCollection<StockQuote> stockQuotes)
        {
            var symbolList = string.Join("%2C", stockQuotes.Select(x => "%22" + x.Symbol + "%22").ToArray());
            var url = string.Format(url_quotes, symbolList);

            var doc = XDocument.Load(url);
            var results = doc.Root.Element("results");
            var ds = new DataSet();
            ds.ReadXml(new StringReader(results.ToString()));
            return ds.Tables[0];
        }

        public static void GetQuotes(BindableCollection<StockQuote> stockQuotes)
        {
            var symbolList = string.Join("%2C", stockQuotes.Select(x => "%22" + x.Symbol + "%22").ToArray());
            var url = string.Format(url_quotes, symbolList);
            var doc = XDocument.Load(url);
            ParseQuotes(stockQuotes, doc);
        }

        private static void ParseQuotes(BindableCollection<StockQuote> stockQuotes, XDocument doc)
        {
            var results = doc.Root.Element("results");

            foreach (var quote in stockQuotes)
            {
                var element = results.Elements("quote").First(XDocument => XDocument.Attribute("symbol").Value == quote.Symbol);
                quote.Ask = Convert.ToDecimal(element.Element("Ask").Value);
                quote.Bid = Convert.ToDecimal(element.Element("Bid").Value);
                quote.Change = Convert.ToDecimal(element.Element("Change").Value);
                quote.PercentChange = Convert.ToDecimal(element.Element("PercentChange").Value);
                quote.LastTradeTime = Convert.ToDateTime(element.Element("LastTradeTime").Value);
                quote.DailyLow = Convert.ToDecimal(element.Element("DailyLow").Value);
                quote.DailyHigh = Convert.ToDecimal(element.Element("DailyHigh").Value);
                quote.YearlyLow = Convert.ToDecimal(element.Element("YearlyLow").Value);
                quote.YearlyHigh = Convert.ToDecimal(element.Element("YearlyHigh").Value);
                quote.LastTradePrice = Convert.ToDecimal(element.Element("LastTradePriceOnly").Value);
                quote.Name = element.Element("Name").Value;
                quote.Open = Convert.ToDecimal(element.Element("Open").Value);
                quote.Volume = Convert.ToDecimal(element.Element("Volume").Value);
                quote.StockExchange = element.Element("StockExchange").Value;
                quote.LastUpdate = DateTime.Now;
            }
        }

        public static void SymbolInsert(Symbol symbol)
        {
            using (var db = new MyDbEntities())
            {
                try
                {
                    db.Symbols.AddObject(symbol);
                    db.SaveChanges();
                }
                catch
                {

                }
            }
        }

        public static void SymbolInsert(BindableCollection<Symbol> symbols)
        {
            using (var db = new MyDbEntities())
            {
                try
                {
                    foreach (var s in symbols)
                    {
                        db.Symbols.AddObject(s);
                    }
                    db.SaveChanges();
                }
                catch
                {

                }
            }
        }

        public static void SymbolInsert(string csvFile)
        {
            BindableCollection<Symbol> symbols = CsvToSymbolCollection(csvFile);
            SymbolInsert(symbols);
        }

        public static BindableCollection<Symbol> CsvToSymbolCollection(string csvFile)
        {
            DataTable dt = ModelHelper.CsvToDatatable(csvFile);
            BindableCollection<Symbol> symbols = new BindableCollection<Symbol>();
            foreach (DataRow row in dt.Rows)
            {
                symbols.Add(new Symbol
                {
                    Ticker = row["Ticker"].ToString(),
                    Region = row["Region"].ToString(),
                    Sector = row["Sector"].ToString()
                });
            }
            return symbols;
        }

        public static BindableCollection<Price> GetYahooHistStockData(int symbolId, string ticker, DateTime startDate, DateTime endDate)
        {
            var result = new BindableCollection<Price>();
            DataTable prices = GetYahooHistStockDataTAble(ticker, startDate, endDate);
            foreach (DataRow row in prices.Rows)
            {
                result.Add(new Price
                {
                    SymbolID = symbolId,
                    Date = Convert.ToDateTime(row["Date"]),
                    PriceOpen = Convert.ToDouble(row["Open"]),
                    PriceClose = Convert.ToDouble(row["Close"]),
                    PriceLow = Convert.ToDouble(row["Low"]),
                    PriceHigh = Convert.ToDouble(row["High"]),
                    PriceAdj = Convert.ToDouble(row["Adj Close"]),
                    Volume = Convert.ToDouble(row["Volume"]),
                });
            }
            return result;
        }

        public static void PriceInsert(Price price)
        {
            using (var db = new MyDbEntities())
            {
                try
                {
                    db.Prices.AddObject(price);
                    db.SaveChanges();
                }
                catch
                {

                }
            }
        }

        public static void PriceInsert(BindableCollection<Price> prices)
        {
            using (var db = new MyDbEntities())
            {
                try
                {
                    foreach (var p in prices)
                    {
                        var price = new Price();
                        price = p;
                        db.Prices.AddObject(price);
                    }
                }
                catch { }
            }
        }

        public static BindableCollection<Symbol> GetTickers()
        {
            var result = new BindableCollection<Symbol>();
            using (var db = new MyDbEntities())
            {
                try
                {
                    var query = from s in db.Symbols orderby s.Ticker select s;
                    result.AddRange(query);
                }
                catch
                {

                }
            }
            return result;
        }

        public static string IdToTicker(int symbolId)
        {
            string ticker = string.Empty;

            using (var db = new MyDbEntities())
            {
                var query = from s in db.Symbols
                            where s.SymbolID == symbolId
                            select s.Ticker;

                foreach (var q in query)
                    ticker = q.ToString();
            }
            return ticker;
        }

    }
}
