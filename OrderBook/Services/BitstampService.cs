using Newtonsoft.Json;
using OrderBook.Domain.Entities;
using System.Globalization;

namespace OrderBook.Services
{
    public class BitstampService
    {
        private readonly HttpClient _httpClient;
        private Timer _timer;
        private const string OrderBookUrl = "https://www.bitstamp.net/api/v2/order_book/btceur/";
        private const int OrderBookFetchingFrequencyInMilliseconds = 1000;

        public event Action<OrderBookResponse> OnOrderBookUpdated;

        public BitstampService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void StartFetchingOrderBook()
        {
            _timer = new Timer(async _ => await FetchOrderBook(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private async Task FetchOrderBook()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(OrderBookUrl);
                var orderBookData = JsonConvert.DeserializeObject<OrderBookResponse>(response);

                orderBookData.BidsItems = orderBookData.Bids.Select(b =>
                {
                    decimal value;
                    decimal.TryParse(b[1], NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                    return new OrderBookItem { Name = b[0], Value = value };
                }).Take(10).OrderBy(b => decimal.Parse(b.Name)).ToList();

                orderBookData.AsksItems = orderBookData.Asks.Select(a =>
                {
                    decimal value;
                    decimal.TryParse(a[1], NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                    return new OrderBookItem { Name = a[0], Value = value };
                }).Take(10).ToList();

                OnOrderBookUpdated?.Invoke(orderBookData);
            }
            catch (Exception ex)
            {
                OnOrderBookUpdated?.Invoke(null);
            }
        }

        public void StopFetchingOrderBook()
        {
            _timer?.Dispose();
        }
    }
}
