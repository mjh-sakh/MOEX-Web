using System;
using Xunit;
using MOEX.Portfolio;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using MOEX.Services;

namespace MOEX_TestsXUnit
{
    public class Tests
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly MoexService moexService = new MoexService(httpClient);

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

            var history = new List<StockRecord>();
            history.Add(new StockRecord(date1, price1));
            history.Add(new StockRecord(date2, price2));
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
        [InlineData("MOEX", 1, "FXRL", 2)]
        public async Task WalletTest(string name1, double value1, string name2, double value2)
        {
            var rand = new Random();
            var endDate = DateTime.Today.AddDays(rand.Next(-7, -3)).SetToWorkDay();
            var startDate = endDate.AddMonths(rand.Next(-14, -2)).SetToWorkDay();
            var interval = rand.Next(1, 6);
            var stock1 = new Stock(name1, value1);
            await moexService.GetStockStartPriceAsync(stock1, startDate);
            await moexService.GetStockEndPriceAsync(stock1, endDate);
            var stock2 = new Stock(name2, value2);
            await moexService.GetStockStartPriceAsync(stock2, startDate);
            await moexService.GetStockEndPriceAsync(stock2, endDate);

            var wallet = new Wallet();
            wallet.AddStock(stock2);
            Assert.Equal(endDate, wallet.EndWalletDate);
            Assert.Equal(startDate, wallet.StartWalletDate);

            wallet.AddStock(stock1);
            Assert.Equal(endDate, wallet.EndWalletDate);
            Assert.Equal(startDate, wallet.StartWalletDate);

        }
    }
}
