using Quandl.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.DataModel.Quandl
{
    public static class QuandlHelper
    {
        const string apiKey = "QgZEw__KgudKNApAxp3e";

        public static string GetQuandlDataAsync(string databaseCode)
        {
            var uri = new Uri($"https://www.quandl.com/api/v3/databases/{databaseCode}/metadata");
            var webclient = new WebClient();
            var data = webclient.DownloadData(uri);
            Console.WriteLine($"Link: {uri.AbsoluteUri.ToString()}");

            var zip = new ZipArchive(new MemoryStream(data));
            var filename = Path.GetRandomFileName();
            try
            {
                zip.Entries.First().ExtractToFile(filename);
                return File.ReadAllText(filename);
            }
            finally
            {
                var file = new FileInfo(filename);
                if (file.Exists)
                {
                    file.Delete();
                }
            }            
        }

        public static async Task<DataTable> GetQuandlDataAsync(string ticker, string databaseCode, DateTime startDate, DateTime endDate)
        {
            var client = new QuandlClient(apiKey);
            var response = await client.Timeseries.GetDataAsync(databaseCode, ticker, limit: 20, startDate: startDate, endDate: endDate);
            return TimeSeriesDataSetToDataTable(response.DatasetData.ColumnNames, response.DatasetData.Data);
        }

        private static DataTable TimeSeriesDataSetToDataTable(List<string> columnNames, List<object[]> data)
        {
            var dt = new DataTable();

            foreach (var columnName in columnNames)
            {
                dt.Columns.Add(columnName);
            }

            foreach (var row in data)
            {
                var newRow = dt.NewRow();
                for (int i = 0; i < columnNames.Count; i++)
                {
                    var columnName = columnNames[i];
                    var rowValue = row[i];
                    newRow[columnName] = rowValue;
                }
                dt.Rows.Add(newRow);
            }

            return dt;
        }
    }
}
