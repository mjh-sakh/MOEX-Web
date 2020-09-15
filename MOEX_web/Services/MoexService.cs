using MOEX.Portfolio;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MOEX.Services
{
    /// <summary>
    /// The Moex service will provide the interaction between Http layer and logic
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

        /// <summary>
        /// Build the query string for the API calls.
        /// </summary>
        /// <param name="parameters">The sequence of name/value tuples to build the query string for.</param>
        /// <returns>The query string to add to the query portion of API urls.</returns>
        private static string BuildQuery(params (string name, object value)[] parameters)
        {
            var query = new StringBuilder();
            if (parameters.Length > 0)
            {
                query.Append('?'); // Query character is added automatically, no need to add it on caller
                foreach (var (name, value) in parameters)
                {
                    if (query.Length > 1)
                        query.Append('&');
                    var formattedValue = value switch // Add here custom formats for other types if required
                    {
                        DateTime dateTime => $"{dateTime:yyyy-MM-dd}",
                        _ => value?.ToString()
                    };
                    query.Append($"{name}={formattedValue}");
                }
            }
            return query.ToString(); // Always encode part of Urls
        }

        public async Task<double> GetStockDataAsync(string stockName, DateTime? startsWith = null, DateTime? endsWith = null)
        {
            var query = BuildQuery(("from", startsWith), ("till", endsWith ?? startsWith?.AddDays(1)));
            var response = await Client.GetAsync(new Uri($"history/engines/stock/markets/shares/securities/{stockName}.json{query}", UriKind.Relative)).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<Data>().ConfigureAwait(false);
                return result.History.Data.FirstOrDefault(x => x.Price > 0)?.Price ?? -1;
            }
            else
                throw new InvalidOperationException(response.ReasonPhrase);
        }

        public async Task GetStockStartPriceAsync(Stock stock, DateTime date)
        {
            if (stock is null)
                throw new ArgumentNullException(nameof(stock));

            stock.StartDate = date;
            stock.StartPrice = await GetStockDataAsync(stock.Name, date).ConfigureAwait(false);
        }

        public async Task GetStockEndPriceAsync(Stock stock, DateTime date)
        {
            if (stock is null)
                throw new ArgumentNullException(nameof(stock));

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
