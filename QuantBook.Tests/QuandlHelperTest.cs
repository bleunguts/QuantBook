using NUnit.Framework;
using QuantBook.Models.DataModel.Quandl;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Tests
{
    public class QuandlHelperTest        
    {
        [Test]
        public async Task GetListOfDataTablesAsync()
        {
            var result = await QuandlHelper.GetQuandlDataAsync("WIKI");
            Console.WriteLine(result);            
        }

        [Test]
        public async Task GetSomeMarketDataAsync()
        {
            var dataTable = await QuandlHelper.GetQuandlDataAsync("FB", "WIKI", new DateTime(2018, 2, 28), new DateTime(2018, 5, 1));
            foreach(DataRow row in dataTable.Rows)
            {
                var data = $"{row["Date"]}, {row["Open"]}, {row["High"]}, {row["Low"]}, {row["Close"]}, {row["Volume"]}";
                Console.WriteLine(data);
            }
        }
    }
}
