using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace MOEX.Portfolio
{
    public static class SavingData
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

    public static class Balancing
        // HACK: not sure how to deal with balancing stocks with different start and end dates
    {
        /// <summary>
        /// Add dates between Start and End dates of the stock which will be later used for balancing.
        /// Dates are added from start day by adding specified number of months. 
        /// </summary>
        /// <param name="stock">Stock where dates are added</param>
        /// <param name="intervalMonths">Number of months between dates</param>
        /// <returns>StockWithHistory with extra dates</returns>
        public static StockWithHistory AddDatesForBalancing(this StockWithHistory stock, int intervalMonths)
        {
            // TODO: check if there are in-between dates already in the class, need decision either to wipe them or somehow mark it
            var balancingDate = stock.StartDate.AddMonths(intervalMonths);
            while (balancingDate < stock.EndDate)
            {
                stock.AddDate(balancingDate);
                balancingDate = balancingDate.AddMonths(intervalMonths);
            }
            return stock;
        }

        public static StockWithHistory AddDatesForBalancing(this Stock stock, int intervalMonths)
        {
            var newStock = new StockWithHistory(stock);
            AddDatesForBalancing(newStock, intervalMonths);
            return newStock;
        }

    }
}

        // public class Wallet
        // {
        //     public double StartValue { get; private set; }
        //     public DateTime StartValueDate { get; private set; }
        //     public double EndValue { get; private set; }
        //     public DateTime EndValueDate { get; private set; }
        //     public Dictionary<string, double> Stocks { get; private set; }

        //     public Wallet()
        //     {
        //         Stocks = new Dictionary<string, double>();
        //     }

        //     public void AddStock(string stockName, double size)
        //     {
        //         Stocks.Add(stockName, size);
        //         CalcStartValue();
        //     }

        //     private void CalcStartValue()
        //     {
        //         StartValue = 0;
        //         foreach ((_, double size) in Stocks)
        //         {
        //             StartValue += size;
        //         }
        //     }

        //     public async Task CalcEndValue(DateTime startDay, DateTime endDay)
        //     {
        //         StartValueDate = startDay;

        //         EndValueDate = endDay;

        //         EndValue = 0;
        //         foreach ((string stockName, double size) in Stocks)
        //         {
        //             Console.WriteLine($"Start date: {startDay}");
        //             var startPrice = await GetStockData(stockName, startDay);
        //             Console.WriteLine($"{stockName}: start price {startPrice}");
        //             Console.WriteLine($"End date: {endDay}");
        //             var endPrice = await GetStockData(stockName, endDay);
        //             Console.WriteLine($"{stockName}: end price {endPrice}");
        //             EndValue += endPrice / startPrice * size;
        //         }
        //     }
        // }

        // static async Task Main(string[] args)
        // {
        //     // DateTime startDay;
        //     // DateTime endDay;
        //     // Wallet wallet = new Wallet();

        //     // var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\wallet.txt"; //for final
        //     // // var path = @"..\..\..\wallet.txt"; //for debugging vs
        //     // // var path = @"wallet.txt"; //for debugging vscode
        //     // using (var file = new StreamReader(path))
        //     // {
        //     //     startDay = DateTime.Parse(file.ReadLine());
        //     //     endDay = DateTime.Parse(file.ReadLine());
        //     //     while (file.ReadLine() is string line)
        //     //     {
        //     //         string[] stock = line.Split(" ");
        //     //         wallet.AddStock(stock[0], double.Parse(stock[1]));
        //     //     }
        //     // }

        //     // await wallet.CalcEndValue(startDay, endDay);

        //     // Console.WriteLine($"Start value: {wallet.StartValue:N1}");
        //     // Console.WriteLine($"End value: {wallet.EndValue:N1}");
        //     // Console.WriteLine($"Change:\t{wallet.EndValue / wallet.StartValue:P1}");
        // }