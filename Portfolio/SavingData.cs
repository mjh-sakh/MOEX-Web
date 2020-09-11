using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MOEX_web.Portfolio
{
    public class SavingData
    {
        public static async Task PushDataToFileAsync(List<Stock> stocks, string portfolioName)
        {
            string jsonString;
            jsonString = JsonConvert.SerializeObject(stocks);
            await File.WriteAllTextAsync(portfolioName, jsonString);
        }

        public static async Task<List<Stock>> LoadDataFromFileAsync(string portfolioName)
        {
            string jsonString = await File.ReadAllTextAsync(portfolioName);
            return JsonConvert.DeserializeObject<List<Stock>>(jsonString);
        }
    }
}
