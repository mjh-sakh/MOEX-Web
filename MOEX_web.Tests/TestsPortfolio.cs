using System;
using System.Linq;
using Xunit;
using MOEX.Portfolio;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using MOEX.Services;
using MathNet.Numerics.LinearAlgebra;
using MOEX;

namespace MOEX_TestsXUnit
{
    public class Tests
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly MoexService moexService = new MoexService(httpClient);
        private static readonly Random rand = new Random(42);

        [Theory]
        [InlineData("MOEX", 1)]
        public void TestStockProperties(string name, double value)
        {

            var test = new StockWithHistory(name, value);

            Assert.Equal(value, test.Value);
            Assert.Equal(name, test.Name);
        }

        [Theory]
        [InlineData("MOEX", 1)]
        public async Task WillDataMatchInStockHistory(string name, double value)
        {

            var date1 = new DateTime(2020, 7, 17).SetToWorkDay();
            var date2 = new DateTime(2020, 8, 17).SetToWorkDay();
            var date3 = new DateTime(2020, 6, 17).SetToWorkDay();

            var stock = new Stock(name, value);
            var newStock = new StockWithHistory(stock);
            Assert.Equal(value, newStock.Value);
            Assert.Equal(name, newStock.Name);

            await moexService.GetStockStartPriceAsync(stock, date1);
            await moexService.GetStockEndPriceAsync(stock, date2);
            newStock = new StockWithHistory(stock);
            Assert.Equal(date1, newStock.History[0].Date);
            Assert.Equal(stock.StartDate, newStock.StartDate);
            Assert.Equal(stock.StartPrice, newStock.History[0].Price);
            Assert.Equal(stock.StartPrice, newStock.StartPrice);
            Assert.Equal(date2, newStock.History[1].Date);
            Assert.Equal(stock.EndDate, newStock.EndDate);
            Assert.Equal(stock.EndPrice, newStock.History[1].Price);
            Assert.Equal(stock.EndPrice, newStock.EndPrice);

            newStock.SortDates();
            Assert.Equal(newStock.History[0].Date, newStock.StartDate);
            Assert.Equal(stock.StartPrice, newStock.History[0].Price);
            Assert.Equal(stock.StartPrice, newStock.StartPrice);
            Assert.Equal(newStock.History[1].Date, newStock.EndDate);
            Assert.Equal(stock.EndPrice, newStock.EndPrice);
            Assert.Equal(stock.EndPrice, newStock.History[1].Price);

            newStock.AddDate(date3);
            Assert.True(newStock.History[1].Date >= newStock.History[0].Date);
            Assert.True(newStock.History[2].Date >= newStock.History[1].Date);
            await moexService.GetPricesAsync(newStock);
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price > 0);
            }

            await moexService.GetPricesAsync(newStock, false);
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price > 0);
            }
        }

        [Fact]
        public void SortRecordsTest()
        {
            var date1 = new DateTime(2020, 8, 17);
            double price1 = 50;
            var date2 = new DateTime(2020, 7, 17);
            double price2 = 100;

            var history = new List<StockRecord>
            {
                new StockRecord(date1, price1),
                new StockRecord(date2, price2)
            };
            history.SortRecords();
            //history.Sort((rec1, rec2) => rec1.Date.CompareTo(rec2.Date));
            Assert.True(history[0].Date < history[1].Date);
        }

        [Fact]
        public void DateAlter()
        {
            var testDate = new DateTime(2020, 09, 18); //Friday
            var date1 = testDate;

            date1 = date1.SetToWorkDay(); //Friday
            Assert.Equal(testDate, date1);

            date1 = date1.AddDays(-1).SetToWorkDay(); //Thursday
            Assert.Equal(testDate.AddDays(-1), date1);

            date1 = date1.AddDays(-1).SetToWorkDay(); //Wednesday
            Assert.Equal(testDate.AddDays(-2), date1);

            date1 = date1.AddDays(-1).SetToWorkDay(); //Tuesday
            Assert.Equal(testDate.AddDays(-3), date1);

            date1 = date1.AddDays(-1).SetToWorkDay(); //Monday
            Assert.Equal(testDate.AddDays(-4), date1);

            date1 = date1.AddDays(-1).SetToWorkDay(); //Sunday > Friday
            Assert.Equal(testDate.AddDays(-7), date1);

            date1 = date1.AddDays(1).SetToWorkDay(); //Saturday > Friday
            Assert.Equal(testDate.AddDays(-7), date1);

        }

        [Theory]
        [InlineData("MOEX", 1, 2)]
        public async Task BalancingTest(string name, double value, int interval)
        {
            var stock = new Stock(name, value);
            await moexService.GetStockStartPriceAsync(stock, DateTime.Today.AddDays(-365).SetToWorkDay());
            await moexService.GetStockEndPriceAsync(stock, DateTime.Today.AddDays(-2).SetToWorkDay());

            var newStock = stock.AddDatesForBalancing(interval);
            // TODO: test that no duplicates for start and end dates
            Assert.True(newStock.History.Count > 2);

            await moexService.GetPricesAsync(newStock);
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price != 0);
            }

            await moexService.GetPricesAsync(newStock, false);
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price != 0);
            }
        }

        [Theory]
        [InlineData()]
        public static void AddDatesForBalancing_WhenNoStartDate_ReturnsXXX()
        {
            // TODO: implement catch when stock does not yet have start or end dates

        }

        [Theory]
        [InlineData("FXUS", 1, 3)]
        public static async Task StockWithHistory_Extension_CreateListOfPrices_Test(string name, double value, int interval)
        {
            //arrange
            var stock = new StockWithHistory(name, value)
            {
                EndDate = DateTime.Today.AddDays(rand.Next(-7, -3)).SetToWorkDay()
            };
            stock.StartDate = stock.EndDate.AddMonths(rand.Next(-14, -2)).SetToWorkDay();
            stock = stock.AddDatesForBalancing(interval);
            await moexService.GetPricesAsync(stock);

            //act
            var list = stock.CreateListOfPrices();
            Assert.Equal(stock.History.Count, list.Count);
            Assert.Equal(stock.History[0].Price, list[0]);
            Assert.Equal(stock.History[^1].Price, list[^1]);
        }

        [Theory]
        [InlineData("MOEX", 1, "FXRL", 2)]
        public static void Wallet_TestingDifferentConstructors(string name1, double value1, string name2, double value2)
        {
            //arrange
            var stock1 = new Stock(name1, value1);
            var stock2 = new Stock(name2, value2);
            var stock3 = new StockWithHistory(stock1);
            var stocks = new List<StockWithHistory>() { stock3, new StockWithHistory(stock2) };

            //act-assert
            var wallet = new Wallet(stock1);
            Assert.Single(wallet.Stocks);
            Assert.Equal(typeof(StockWithHistory), wallet.Stocks[0].GetType());

            wallet = new Wallet(stock3);
            Assert.Single(wallet.Stocks);
            Assert.Equal(stock3, wallet.Stocks[0]);

            wallet.AddStock(stock2);
            Assert.Equal(2, wallet.Stocks.Count);

            wallet = new Wallet(stocks);
            Assert.Equal(stocks, wallet.Stocks);
        }
        
        public static Wallet CreateSampleWallet()
        {
            var wallet = new Wallet(new Stock("MOEX", .5));
            wallet.AddStock(new Stock("FXGD", 1));
            wallet.AddStock(new Stock("FXRL", 2));
            wallet.AddStock(new Stock("FXUS", 3));

            return wallet;
        }

        [Fact]
        //[InlineData("2020-03-12", "2020-07-13")]
        public static void Wallet_SetDatesTest()//string date1, string date2)
        {
            //arrange
            var endDate = DateTime.Today.AddDays(rand.Next(-7, -3)).SetToWorkDay();
            var startDate = endDate.AddMonths(rand.Next(-14, -2)).SetToWorkDay();
            var wallet = CreateSampleWallet();

            //act-assert
            wallet.SetDates(startDate, endDate);

            Assert.Equal(endDate, wallet.EndDate);
            Assert.Equal(startDate, wallet.StartDate);
            Assert.Equal(endDate, wallet.Stocks[0].EndDate);
            Assert.Equal(startDate, wallet.Stocks[^1].StartDate);

        }
        [Fact]
        public static void Wallet_CalcStartWalletValues_Runs()
        {
            //arrange 
            var wallet = CreateSampleWallet();

            //act
            wallet.CalcStartValue();

            //assert
            Assert.True(wallet.StartValue > 0);
        }

        [Fact]
        public static void Wallet_CreateListOfValues_Runs()
        {
            //arrange 
            var wallet = CreateSampleWallet();

            //act
            var list = wallet.CreateListOfStockValues();

            //assert
            Assert.Equal(list.Count, wallet.Stocks.Count);
            Assert.Equal(list[0], wallet.Stocks[0].Value);
        }

        [Fact]
        public static void AddDatesForBalancing_RunsForWallet()
        {
            //arrange
            var wallet = CreateSampleWallet();
            var endDate = DateTime.Today.AddDays(rand.Next(-7, -3)).SetToWorkDay();
            var startDate = endDate.AddMonths(rand.Next(-14, -5)).SetToWorkDay();
            wallet.SetDates(startDate, endDate);
            var interval = rand.Next(1, 4);

            //act
            wallet.AddDatesForBalancing(interval);

            // TODO: assert for wallet adding dates
            Assert.True(true);
        }

        [Fact]
        public static async Task GetPricesAsync_RunsForWallet()
        {
            //arrange
            var wallet = CreateSampleWallet();
            var endDate = DateTime.Today.AddDays(rand.Next(-7, -3)).SetToWorkDay();
            var startDate = endDate.AddMonths(rand.Next(-14, -5)).SetToWorkDay();
            wallet.SetDates(startDate, endDate);
            var interval = rand.Next(1, 4);
            wallet = wallet.AddDatesForBalancing(interval);

            //act
            await moexService.GetPricesAsync(wallet).ConfigureAwait(false);

            // TODO: assert for wallet getting prices
            Assert.True(true);
        }

        public static Wallet CreateSampleWalletWithPrices()
        {
            var wallet = CreateSampleWallet();
            var endDate = DateTime.Today.AddDays(rand.Next(-7, -3)).SetToWorkDay();
            var startDate = endDate.AddMonths(rand.Next(-14, -5)).SetToWorkDay();
            wallet.SetDates(startDate, endDate);
            var interval = rand.Next(1, 4);
            wallet = wallet.AddDatesForBalancing(interval);
            for (int iStock = 0; iStock < wallet.Stocks.Count; iStock++)
            {
                for (int jDate = 0; jDate < wallet.Stocks[0].History.Count; jDate++)
                {
                    wallet.Stocks[iStock].History[jDate].Price = (double)rand.Next(80, 120);
                }
            }
            return wallet;
        }

        [Fact]
        public static void Pariwise_Runs()
        {
            var wallet = CreateSampleWalletWithPrices();
            
            var interestRates = Matrix<double>.Build.DenseOfRows(wallet.Stocks.Select(s => s.CreateListOfPrices().Pairwise((x, y) => x / y)));

            Assert.True(true);
        }
        
        [Theory]
        [InlineData()]
        public static void Wallet_CalcEndValue_Runs()
        {
            //arrange
            var wallet = CreateSampleWalletWithPrices();

            //act
            wallet.CalcEndValue();

            //assert
            Assert.True(true);

        }

        [Fact]
        public static async Task Wallet_CalcEndValue_RunSingleStock()
        {
            //arrange
            var stock = new Stock("MOEX", 1);
            var wallet = new Wallet(stock);
            var startDate = DateTime.Today.AddDays(-150).SetToWorkDay();
            var endDate = DateTime.Today.AddDays(-2).SetToWorkDay();
            wallet.SetDates(startDate, endDate);
            await moexService.GetStockStartPriceAsync(stock, startDate);
            await moexService.GetStockEndPriceAsync(stock, endDate);
            await moexService.GetPricesAsync(wallet);

            //act
            wallet.CalcEndValue();

            //assert
            Assert.Equal(stock.EndPrice / stock.StartPrice, wallet.EndValue);

            //arrange
            wallet = wallet.AddDatesForBalancing(1);
            await moexService.GetPricesAsync(wallet);

            //act
            wallet.CalcEndValue();

            //assert
            Assert.Equal(stock.EndPrice / stock.StartPrice, wallet.EndValue, 10);
        }

        [Fact]
        public static async Task Wallet_CalcEndValue_RunsMultipleStocksWithBalancing()
        {
            var wallet = CreateSampleWallet();
            var startDate = DateTime.Today.AddDays(-150).SetToWorkDay();
            var endDate = DateTime.Today.AddDays(-2).SetToWorkDay();
            wallet.SetDates(startDate, endDate);
            wallet.AddDatesForBalancing(2);
            await moexService.GetPricesAsync(wallet);

            wallet.CalcEndValue();

            Assert.True(true);
        }

        [Theory]
        [InlineData(@"Data\portfolio.json")]
        public static async Task LoadingLoadDataFromFileAsync_Runs(string path)
        {
            var stocks = await SavingData.LoadDataFromFileAsync(path).ConfigureAwait(false);

            var wallet = new Wallet(stocks);

            Assert.True(true);
        }

        [Fact]
        public static void BigRun()
        {
            Program.Main(new string[0]);

            Assert.True(true);
        }
    }
}
