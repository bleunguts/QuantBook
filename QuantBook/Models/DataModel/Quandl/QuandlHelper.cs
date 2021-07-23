using Quandl.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.DataModel.Quandl
{
    public static class QuandlHelper
    {
        const string apiKey = "QgZEw__KgudKNApAxp3e";

        public static async Task<string> GetQuandlDataAsync(string dataTableCode)
        {
            var client = new QuandlClient(apiKey);
            using (var stream = await client.Tables.DownloadAsync(dataTableCode))            
            using (var text = new System.IO.StreamReader(stream))
            {
                return text.ReadToEnd();
            }
        }

        public static async Task<DataTable> GetQuandlDataAsync(string ticker, string dataTableCode, DateTime startDate, DateTime endDate)
        {
            var client = new QuandlClient(apiKey);
            var response = await client.Timeseries.GetDataAsync(dataTableCode, ticker, limit: 20, startDate: startDate, endDate: endDate);
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
