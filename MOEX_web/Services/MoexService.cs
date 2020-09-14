using MOEX.Portfolio;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MOEX.Services
{
    /// <summary>
    /// The Moex service will provide the interaction between with Http layer and logic
    /// </summary>
    public class MoexService
    {
        private HttpClient Client { get; }

        /// <summary>
        /// Initializes thew instance of a Moex service from dependency injection.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> here is created automatically by dependency container.</param>
        public MoexService(HttpClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Client.BaseAddress = new Uri("http://iss.moex.com/iss/");
        }

        public async Task<double> GetStockDataAsync(string stockName, DateTime date)
        {
            DateTime fromDate = date;
            DateTime tillDate = date.AddDays(1);
            try
            {
                string requestHTTP = $"history/engines/stock/markets/shares/securities/{stockName}.json?from={fromDate:yyyy-MM-dd}&till={tillDate:yyyy-MM-dd}";

                Trace.WriteLine(new Uri(Client.BaseAddress, requestHTTP));
                var response = await Client.GetAsync(requestHTTP).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<Data>().ConfigureAwait(false);
                    return result.History.Data.FirstOrDefault(x => x.Price > 0)?.Price ?? -1;
                }
                else
                    throw new InvalidOperationException(response.ReasonPhrase);
            }
            catch (HttpRequestException e)
            {
                return -1;
            }
        }

        public async Task GetStockStartPriceAsync(Stock stock, DateTime date)
        {
            stock.StartDate = date;
            stock.StartPrice = await GetStockDataAsync(stock.Name, date).ConfigureAwait(false);
        }

        public async Task GetStockEndPriceAsync(Stock stock, DateTime date)
        {
            stock.EndDate = date;
            stock.EndPrice = await GetStockDataAsync(stock.Name, date).ConfigureAwait(false);
        }

        public async Task GetPricesAsync(StockWithHistory stock, bool onlyForEmpty = true)
        {
            if (stock is null)
                throw new ArgumentNullException(nameof(stock));
            
            //var runningRequests = new List<Task>();
            foreach (var record in stock.History)
            {
                if (onlyForEmpty && record.Price <= 0)
                    record.Price = await GetStockDataAsync(stock.Name, record.Date).ConfigureAwait(false);

                if (!onlyForEmpty)
                    record.Price = await GetStockDataAsync(stock.Name, record.Date).ConfigureAwait(false);
            }
        }
    }
}
