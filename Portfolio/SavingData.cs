using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BlazorApp.Portfolio
{
    public class SavingData
    {
        public static void PushDataToFile(List<Stock> stocks, string portfolioName)
        {
            string jsonString;
            jsonString = JsonConvert.SerializeObject(stocks);
            File.WriteAllText(portfolioName, jsonString);
        }

        public static List<Stock> LoadDataFromFile(string portfolioName)
        {
            return JsonConvert.DeserializeObject<List<Stock>>(File.ReadAllText(portfolioName));
        }
    }
}
