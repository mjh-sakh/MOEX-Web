using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace MOEX_web.Portfolio
{
    public class Stock
    {
        public string Name { get; }
        public double Value { get; set; }
        public DateTime StartDate { get; set; }
        public double StartPrice { get; set; }
        public DateTime EndDate { get; set; }
        public double EndPrice { get; set; }

        public Stock(string name, double value)
        {
            Name = name;
            Value = value;
        }
    }

    public class StockRecord
    {
        public DateTime Date { get; private set; }
        public double Price { get; set; }

        public StockRecord(DateTime date, double price)
        {
            Date = date;
            Price = price;
        }

    }
    public static class RecordExtensions
    {
        public static void SortRecords(this List<StockRecord> records)
        {
            records.Sort((rec1, rec2) => rec1.Date.CompareTo(rec2.Date));
        }

    }

    public class StockWithHistory : Stock
    {
        public List<StockRecord> History { get; private set; } = new List<StockRecord>();

        public StockWithHistory(string name, double value) : base(name, value) { }
        public StockWithHistory(Stock stock) : base(stock.Name, stock.Value)
        {
            if (stock.StartPrice > 0) History.Add(new StockRecord(stock.StartDate, stock.StartPrice));
            if (stock.EndPrice > 0) History.Add(new StockRecord(stock.EndDate, stock.EndPrice));
        }


        public void SortDates()
        {
            History.SortRecords();

            StartDate = History[0].Date;
            StartPrice = History[0].Price;
            EndDate = History[^1].Date;
            EndPrice = History[^1].Price;
        }

        public void AddDate(DateTime date)
        {
            History.Add(new StockRecord(date, 0));
            History.SortRecords();
        }

        public async Task GetPricesAsync(bool onlyForEmpty = true)
        {
            //var runningRequests = new List<Task>();
            foreach (var record in History)
            {
                if (onlyForEmpty && record.Price <= 1) record.Price = await GettingData.GetStockDataAsync(Name, record.Date);
                if (!onlyForEmpty) record.Price = await GettingData.GetStockDataAsync(Name, record.Date);
            }
        }

    }
    public class GettingData
    {
        public static async Task<double> GetStockDataAsync(string stockName, DateTime date)
        {
            var client = new HttpClient() { BaseAddress = new Uri("http://iss.moex.com/iss/") };
            DateTime fromDate = date;
            DateTime tillDate = date.AddDays(1);
            try
            {
                string requestHTTP = $"history/engines/stock/markets/shares/securities/{stockName}.json?from={fromDate:yyyy-MM-dd}&till={tillDate:yyyy-MM-dd}";
                Console.WriteLine(new Uri(client.BaseAddress, requestHTTP));
                var response = await client.GetAsync(requestHTTP);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<Data>();
                    return result.History.Data.FirstOrDefault(x => x.Price > 0)?.Price ?? -1;
                }
                else
                    throw new InvalidOperationException(response.ReasonPhrase);
            }
            catch (HttpRequestException e)
            {
                return 0;
            }
        }

        public static async Task GetStockStartPriceAsync(Stock stock, DateTime date)
        {
            stock.StartDate = date;
            stock.StartPrice = await GetStockDataAsync(stock.Name, date);
        }

        public static async Task GetStockEndPriceAsync(Stock stock, DateTime date)
        {
            stock.EndDate = date;
            stock.EndPrice = await GetStockDataAsync(stock.Name, date);
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
    }
}