using System;
using Xunit;
using MOEX.Portfolio;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MOEX_TestsXUnit
{
    public class Tests
    {
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
            
            var date1 = new DateTime(2020, 7, 17);
            var date2 = new DateTime(2020, 8, 17);
            var date3 = new DateTime(2020, 6, 17);

            var stock = new Stock(name, value);
            var newStock = new StockWithHistory(stock);
            Assert.Equal(value, newStock.Value);
            Assert.Equal(name, newStock.Name);

            await GettingData.GetStockStartPriceAsync(stock, date1);
            await GettingData.GetStockEndPriceAsync(stock, date2);
            newStock = new StockWithHistory(stock);
            Assert.Equal(date1, newStock.History[0].Date);
            Assert.True(newStock.History[0].Price > 0);
            Assert.Equal(date2, newStock.History[1].Date);
            Assert.True(newStock.History[1].Price > 0);
            Assert.Equal(stock.StartDate, newStock.StartDate);
            Assert.Equal(stock.EndDate, newStock.EndDate);

            newStock.SortDates();
            Assert.Equal(newStock.History[0].Date, newStock.StartDate);
            Assert.Equal(newStock.History[^1].Date, newStock.EndDate);

            newStock.AddDate(date3);
            Assert.True(newStock.History[1].Date >= newStock.History[0].Date);
            Assert.True(newStock.History[2].Date >= newStock.History[1].Date);
            await newStock.GetPricesAsync();
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price != 0);
            }

            await newStock.GetPricesAsync(false);
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price != 0);
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
            await GettingData.GetStockStartPriceAsync(stock, DateTime.Today.AddDays(-365).SetToWorkDay());
            await GettingData.GetStockEndPriceAsync(stock, DateTime.Today.AddDays(-2).SetToWorkDay());

            var newStock = stock.AddDatesForBalancing(interval);
            Assert.True(newStock.History.Count > 2);
            
            await newStock.GetPricesAsync();
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price != 0);
            }

            await newStock.GetPricesAsync(false);
            foreach (var record in newStock.History)
            {
                Assert.True(record.Price != 0);
            }
        }

    }
}
