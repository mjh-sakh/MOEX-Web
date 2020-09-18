using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MOEX.Portfolio
{
    public static class SavingData
    {
        public static async Task PushDataToFileAsync(List<Stock> stocks, string portfolioName)
        {
            string jsonString;
            jsonString = JsonConvert.SerializeObject(stocks);
            await File.WriteAllTextAsync(portfolioName, jsonString).ConfigureAwait(false);
        }

        public static async Task<List<Stock>> LoadDataFromFileAsync(string portfolioName)
        {
            string jsonString = await File.ReadAllTextAsync(portfolioName).ConfigureAwait(false);
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
            if (stock is null)
                throw new ArgumentNullException(nameof(stock));
            if (stock.StartDate == DateTime.MinValue || stock.EndDate == DateTime.MinValue)
                throw new ArgumentNullException(nameof(stock)); // TODO: need to change type of exception thrown 

            var balancingDate = stock.StartDate;
            var endDate = stock.EndDate;
            if (stock.History.Count != 0) // a bit ugly check if start and end dates are already in the set
            {
                balancingDate = stock.History[0].Date == stock.StartDate ? balancingDate.AddMonths(intervalMonths) : balancingDate;
                endDate = (stock.History[0].Date == endDate || (stock.History.Count > 1 && stock.History[^1].Date == endDate)) ?
                    endDate.AddSeconds(-1) : endDate;
            }
            while (balancingDate <= endDate)
            {
                stock.AddDate(balancingDate);
                if (balancingDate == stock.EndDate) break;
                balancingDate = balancingDate.AddMonths(intervalMonths);
                balancingDate = balancingDate > stock.EndDate ? stock.EndDate : balancingDate;
            }
            return stock;
        }

        public static Wallet AddDatesForBalancing(this Wallet wallet, int intervalMonths)
        {
            if (wallet is null)
                throw new ArgumentNullException(nameof(wallet));

            for (int i = 0; i < wallet.Stocks.Count; i++)
            {
                wallet.Stocks[i] = wallet.Stocks[i].AddDatesForBalancing(intervalMonths);
            }
            return wallet;
        }

        public static StockWithHistory AddDatesForBalancing(this Stock stock, int intervalMonths)
        {
            var newStock = new StockWithHistory(stock);
            AddDatesForBalancing(newStock, intervalMonths);
            return newStock;
        }

        public static void CalcStartValue(this Wallet wallet)
        {
            if (wallet is null)
                throw new ArgumentNullException(nameof(wallet));

            wallet.StartValue = 0;
            foreach (var stock in wallet.Stocks)
            {
                wallet.StartValue += stock.Value;
            }

        }

        public static void CalcEndValue(this Wallet wallet)
        {
            if (wallet is null)
                throw new ArgumentNullException(nameof(wallet));

            if (wallet.StartValue == 0) wallet.CalcStartValue();
            var values = new List<double>() { wallet.StartValue };
            var walletInterestRates = wallet.CalcInterestRatesForWallet();
            foreach (var i in walletInterestRates) values.Add(values[^1] * i);

            wallet.EndValue = values[^1];
            wallet.WriteHistory(values);
        }

        private static List<double> CalcInterestRatesForWallet(this Wallet wallet)
        {
            var rates = new List<double>();

            var targetWalletComposition = new List<double>();
            // fraction of Stock1.Value : S2.V : .. : SN.V should be restored after each rebalance
            foreach (var stockValue in wallet.CreateListOfStockValues()) targetWalletComposition.Add(stockValue / wallet.StartValue);
            var tmp = new double[1][];
            tmp[0] = targetWalletComposition.ToArray();
            var targetComposition = Matrix<double>.Build.DenseOfRows(tmp);
            var stocksInterstRates = wallet.CalcInterestRatesForStocks();
            // rates[matrix of size: 1 x (Stock[any].History.Count - 1)]  =
            //  = targetWalletComposition[matrix of size: 1 x Stocks.Count] dot prodcut stocksInterstRates[matrix of size: Stocks.Count x (Stock[any].History.Count - 1)]
            // rates.toList()

            rates = (targetComposition * stocksInterstRates).ToArray().OfType<double>().ToList();

            return rates;
        }

        private static Matrix<double> CalcInterestRatesForStocks(this Wallet wallet)
        {
            var interestRates = Matrix<double>.Build.DenseOfRows(wallet.Stocks.Select(s => s.CreateListOfPrices().Pairwise((prev, curr) => curr / prev)));
            return interestRates;
        }
    }
}