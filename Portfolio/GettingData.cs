using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp.Portfolio
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
        public class RecordConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Record).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var record = Activator.CreateInstance(objectType) as Record;
            if (reader.TokenType == JsonToken.StartArray)
            {
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Ticker = reader.Value as string;
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Date = DateTime.Parse(reader.Value as string);
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Item2 = reader.Value as string;
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Item3 = reader.Value as string;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item4 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item5 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item6 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item7 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item8 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item9 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item10 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item11 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item12 = (int)(long)reader.Value;
                if (reader.Read()) switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                        record.Price = (long)reader.Value;
                        break;
                    case JsonToken.Float:
                        record.Price = (double)reader.Value;
                        break;
                }
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item14 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item15 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item16 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item17 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item18 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Volume = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item20 = (int)(long)reader.Value;
                while (reader.Read() && reader.TokenType != JsonToken.EndArray) ;
            }
            return record;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }

    [JsonConverter(typeof(RecordConverter))]
    public class Record
    {
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public string Item2 { get; set; }
        public string Item3 { get; set; }
        public int Item4 { get; set; }
        public int Item5 { get; set; }
        public int Item6 { get; set; }
        public int Item7 { get; set; }
        public int Item8 { get; set; }
        public int Item9 { get; set; }
        public int Item10 { get; set; }
        public int Item11 { get; set; }
        public int Item12 { get; set; }
        public double Price { get; set; }
        public int Item14 { get; set; }
        public int Item15 { get; set; }
        public int Item16 { get; set; }
        public int Item17 { get; set; }
        public int Item18 { get; set; }
        public double Volume { get; set; }
        public int Item20 { get; set; }
    }

    public class Subset
    {
        public IList<Record> Data { get; } = new List<Record>();
    }

    public class Data
    {
        public Subset History { get; } = new Subset();
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
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return 0;
            }
        }

        public static async void GetStockStartPriceAsync(Stock stock, DateTime date)
        {
            stock.StartDate = date;
            stock.StartPrice = await GetStockDataAsync(stock.Name, date);
        }

        public static async void GetStockEndPriceAsync(Stock stock, DateTime date)
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