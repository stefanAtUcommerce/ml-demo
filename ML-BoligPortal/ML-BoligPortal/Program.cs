using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;

namespace ML_BoligPortal
{
    class Program
    {
        private static readonly DateTime CurrentDate = DateTime.Now;

        static async Task Main()
        {
            // get and save raw data
            //var crawler = new WebCrawler(new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler { AllowAutoRedirect = false }));
            //var rawData = await crawler.GatherData();
            //SaveData(rawData, true);

            // get raw and save clean data
            //var cleanData = GetData()
            //    .Where(p => p.PropertyType.Contains("Apartment"))
            //    .ToList();
            //SaveData(cleanData, false);
        }

        static void SaveData(List<RawProperty> data, bool isRawData)
        {
            var appendRawName = isRawData ? "raw-" : string.Empty;
            using var csvWriter = new CsvWriter(new StreamWriter(File.OpenWrite($"../../../bolig-portal-{appendRawName}data-{CurrentDate:yyyy-MM-dd}.csv"), Encoding.UTF8), CultureInfo.CurrentCulture);
            csvWriter.WriteRecords(data);
            File.WriteAllText($"../../../bolig-portal-{appendRawName}data-{CurrentDate:yyyy-MM-dd}.json", JsonConvert.SerializeObject(data));
        }

        static List<RawProperty> GetData()
        {
            using var csvReader = new CsvReader(new StreamReader(File.OpenRead($"../../../bolig-portal-raw-data-{CurrentDate:yyyy-MM-dd}.csv"), Encoding.UTF8), CultureInfo.CurrentCulture);
            return csvReader.GetRecords<RawProperty>()
                .ToList();
        }
    }
}
