using System;
using System.Collections.Generic;

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
        public static IEnumerable<TSource> Pairwise<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
            where TSource : struct
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            TSource? previous = null;
            foreach (var element in source)
            {
                if (previous is TSource value)
                    yield return func(value, element);
                previous = element;
            }
        }

        public static void SortRecords(this List<StockRecord> records)
        {
            if (records is null)
                throw new ArgumentNullException(nameof(records));

            records.Sort((rec1, rec2) => rec1.Date.CompareTo(rec2.Date));
        }

        public static DateTime SetToWorkDay(this DateTime date)
        {
            //date = date.AddDays(-1);
            //switch (date.DayOfWeek)
            //{
            //    case DayOfWeek.Sunday:
            //        return date.AddDays(-2);
            //    case DayOfWeek.Saturday:
            //        return date.AddDays(-1);
            //    default:
            //        return date;
            //}

            date = date.DayOfWeek switch
            {
                DayOfWeek.Sunday => date.AddDays(-2),
                DayOfWeek.Saturday => date.AddDays(-1),
                _ => date,
            };
            return date;
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

        public static List<string> CreateListOfStocks(this Wallet wallet)
        {
            if (wallet is null)
                throw new ArgumentNullException(nameof(wallet));

            var list = new List<string>();
            foreach (var stock in wallet.Stocks)
            {
                list.Add(stock.Name);
            }
            return list;
        }

        public static List<double> CreateListOfStockValues(this Wallet wallet)
        {
            if (wallet is null)
                throw new ArgumentNullException(nameof(wallet));

            var list = new List<double>();
            foreach (var stock in wallet.Stocks)
            {
                list.Add(stock.Value);
            }
            return list;
        }

        public static List<double> CreateListOfPrices(this StockWithHistory stock)
        {
            if (stock is null)
                throw new ArgumentNullException(nameof(stock));

            var list = new List<double>();
            foreach (var record in stock.History)
            {
                list.Add(record.Price);
            }
            return list;
        }

    }

    public class StockWithHistory : Stock
    {
        public List<StockRecord> History { get; private set; } = new List<StockRecord>();

        public StockWithHistory(string name, double value) : base(name, value) { }
        public StockWithHistory(Stock stock) : base(stock.Name, stock.Value)
        {
            if (stock is null)
                throw new ArgumentNullException(nameof(stock));

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
        public List<StockWithHistory> Stocks { get; private set; } = new List<StockWithHistory>();
        public DateTime StartDate { get; private set; }
        public double StartValue { get; set; }
        public DateTime EndDate { get; private set; }
        public double EndValue { get; set; }
        public List<StockRecord> History { get; private set; }

        //public Wallet() { }
        public Wallet(Stock stock) => AddStock(stock);
        public Wallet(StockWithHistory stock) => AddStock(stock);
        public Wallet(List<StockWithHistory> stocks)
        {
            Stocks = stocks;
            //SetWalletDates();
        }

        public void AddStock(Stock stock) => AddStock(new StockWithHistory(stock));
        public void AddStock(StockWithHistory stock)
        {
            Stocks.Add(stock);
            //SetWalletDates();
        }

        public void SetWalletDates(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            foreach (var stock in Stocks)
            {
                stock.StartDate = startDate;
                stock.EndDate = endDate;
            }
        }

        public void WriteHistory(List<double> values)
        {
            // TODO: zip Dates and values
        }

    }
}