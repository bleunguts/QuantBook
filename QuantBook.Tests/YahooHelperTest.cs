using NUnit.Framework;
using QuantBook.Models.DataModel.Yahoo;
using QuantBook.Models;
using System.Data;
using System.IO;
using System.Reflection;

namespace QuantBook.Tests
{
    /// <summary>
    /// If you are looking at this and realize why it doesnt work its because Yahoo discountued there finance api
    /// there is an alternative
    /// </summary>
    public class YahooHelperTest
    {
        string TickerFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + @"\StockTickers.csv";

        [Test]
        public void CsvToDatableUsingOdbcTextDriverWorks()
        {
            //var csvFile = @"C:\Dev\Projects\QuantBook\QuantBook\QuantBook.Tests\bin\Debug\net472";
            var csvFile = @"C:\temp\StockTickers.csv";
            DataTable dt = ModelHelper.CsvToDatatable(csvFile);
            Assert.Greater(dt.Rows.Count, 0);
            foreach(DataRow row in dt.Rows)
            {
                Assert.IsNotNull(row["Ticker"].ToString());
                Assert.IsNotNull(row["Region"].ToString());
                Assert.IsNotNull(row["Sector"].ToString());

            }
            Assert.Pass("If it reached here we are all good.");
        }
    }
}