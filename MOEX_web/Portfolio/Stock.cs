using System;
using System.Collections.Generic;
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
                History.Add(new StockRecord(stock.StartDate, stock.StartPrice));
            }
            if (stock.EndPrice > 0)
            {
                this.EndDate = stock.EndDate;
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

        public async Task GetPricesAsync(bool onlyForEmpty = true)
        {
            //var runningRequests = new List<Task>();
            foreach (var record in History)
            {
                if (onlyForEmpty && record.Price <= 0) record.Price = await GettingData.GetStockDataAsync(Name, record.Date);
                if (!onlyForEmpty) record.Price = await GettingData.GetStockDataAsync(Name, record.Date);
            }
        }

    }
}
