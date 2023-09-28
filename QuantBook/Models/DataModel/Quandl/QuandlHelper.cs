using Quandl.NET;
using QuandlCS.Requests;
using QuandlCS.Types;
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
        private const string quandlKey = "gp_z7rn26KEP3uJFuuiw";

        public static DataTable GetQuandlData(string ticker, string dataSource, DateTime startDate, DateTime endDate)
        {
            QuandlDownloadRequest request = new QuandlDownloadRequest();
            request.APIKey = quandlKey;
            request.Datacode = new Datacode(dataSource, ticker);
            request.Format = FileFormats.CSV;
            request.Frequency = Frequencies.Daily;
            request.StartDate = startDate;
            request.EndDate = endDate;
            request.Sort = SortOrders.Ascending;

            string ss = request.ToRequestString().Replace("/v1/", "/v3/");

            DataTable dt = new DataTable();
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(ss, "my.csv");
                    dt = ModelHelper.CsvToDatatable("my.csv");
                    File.Delete("my.csv");
                }
                catch { }
            }
            return dt;
        }

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
            var client = new QuandlClient(quandlKey);
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
