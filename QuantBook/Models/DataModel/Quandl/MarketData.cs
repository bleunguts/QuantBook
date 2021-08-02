using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.DataModel.Quandl
{
    public static class MarketData
    {
        public static async Task<IEnumerable<StockData>> GetStockData(string ticker, DateTime startDate, DateTime endDate)
        {
            List<StockData> res = new List<StockData>();
            DataTable dt = await QuandlHelper.GetQuandlDataAsync(ticker, "WIKI", startDate, endDate);
            if (dt.Rows.Count < 1)
                return res;

            dt = ModelHelper.DatatableSort(dt, "Date ASC");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    res.Add(new StockData
                    {
                        Ticker = ticker,
                        Date = row["Date"].To<DateTime>(),
                        Open = row["Open"].To<double>(),
                        High = row["High"].To<double>(),
                        Low = row["Low"].To<double>(),
                        Close = row["Close"].To<double>(),
                        Volume = row["Volume"].To<double>(),
                        ExDividend = row["Ex-Dividend"].To<double>(),
                        SplitRatio = row["Split Ratio"].To<double>(),
                        AdjOpen = row["Adj. Open"].To<double>(),
                        AdjHigh = row["Adj. High"].To<double>(),
                        AdjLow = row["Adj. Low"].To<double>(),
                        AdjClose = row["Adj. Close"].To<double>(),
                        AdjVolume = row["Adj. Volume"].To<double>()
                    }) ;
                }
            }
            return res;
        }
    }
}
