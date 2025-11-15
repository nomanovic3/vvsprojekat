using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TestProject1.TestData
{
    public static class CsvDataLoader
    {
        public static IEnumerable<object[]> GetTestData<T>(string fileName) where T : class
        {
            
            string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, fileName);

            // Konfiguracija za CsvHelper 
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                Delimiter = ",", //  zarez kao delimiter
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                
                return csv.GetRecords<T>()
                          .Select(record => new object[] { record }) // Zamotava svaki red u object[] za MSTest
                          .ToList();
            }
        }
    }
}