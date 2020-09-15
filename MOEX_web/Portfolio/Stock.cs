using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MOEX.Portfolio
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
    public static class StockExtension
    {
        public static void SortRecords(this List<StockRecord> records)
        {
            records.Sort((rec1, rec2) => rec1.Date.CompareTo(rec2.Date));
        }

        public static DateTime SetToWorkDay(this DateTime date)
        {
            //date = date.AddDays(-1);
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return date.AddDays(-2);
                case DayOfWeek.Saturday:
                    return date.AddDays(-1);
                default:
                    return date;
            }
        }

        public static bool IsWorkDay(this DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    return false;
                default:
                    return true;
            }
        }
    }

    public class StockWithHistory : Stock
    {
        public List<StockRecord> History { get; private set; } = new List<StockRecord>();

        public StockWithHistory(string name, double value) : base(name, value) { }
        public StockWithHistory(Stock stock) : base(stock.Name, stock.Value)
        {
            if (stock.StartPrice > 0)
            {
                this.StartDate = stock.StartDate;
                this.StartPrice = stock.StartPrice;
                History.Add(new StockRecord(stock.StartDate, stock.StartPrice));
            }
            if (stock.EndPrice > 0)
            {
                this.EndDate = stock.EndDate;
                this.EndPrice = stock.EndPrice;
                History.Add(new StockRecord(stock.EndDate, stock.EndPrice));
            }
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
            date = date.IsWorkDay() ? date : date.SetToWorkDay(); // should I do this check here or not?
            History.Add(new StockRecord(date, 0));
            History.SortRecords();
        }
    }

    public class Wallet
    {
        public List<StockWithHistory> WalletStocks { get; private set; } = new List<StockWithHistory>();
        public DateTime StartWalletDate { get; private set; }
        public double StartWalletValue { get; private set; }
        public DateTime EndWalletDate { get; private set; }
        public double EndWalletValue { get; private set; }

        public Wallet() { }
        public Wallet(Stock stock) => AddStock(stock);
        public Wallet(StockWithHistory stock) => AddStock(stock);
        public Wallet(List<StockWithHistory> stocks)
        {
            WalletStocks = stocks;
            SetWalletDates();
        }

        public void AddStock(Stock stock) => AddStock(new StockWithHistory(stock));
        public void AddStock(StockWithHistory stock)
        {
            WalletStocks.Add(stock);
            SetWalletDates();
        }

        private void SetWalletDates()
        {
            var earliestStartDate = DateTime.Today;
            var latestEndDate = new DateTime(1900, 1, 1);

            foreach (var stock in WalletStocks)
            {
                earliestStartDate = earliestStartDate > stock.StartDate ? stock.StartDate : earliestStartDate;
                latestEndDate = latestEndDate < stock.EndDate ? stock.EndDate : latestEndDate;
            }

            StartWalletDate = earliestStartDate;
            EndWalletDate = latestEndDate;

        }
    }
}