using Newtonsoft.Json;
using OrderBook.Domain;
using OrderBook.Domain.Entities;
using OrderBook.Domain.Enums;
using OrderBook.Responses;
using System.Globalization;

namespace OrderBook.Services
{
    public class BitstampService
    {
        private readonly HttpClient _httpClient;
        private readonly OrderBookDbContext _dbContext;
        private Timer _timer;
        private const string OrderBookUrl = "https://www.bitstamp.net/api/v2/order_book/btceur/";
        private const int OrderBookFetchingFrequencyInMilliseconds = 1000;
        private const int OrderBookItemsToTake = 100;

        public event Action<OrderBookResponse> OnOrderBookUpdated;

        public BitstampService(HttpClient httpClient, OrderBookDbContext dbContext)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
        }

        public void StartFetchingOrderBook()
        {
            _timer = new Timer(async _ => await FetchOrderBook(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(OrderBookFetchingFrequencyInMilliseconds));
        }

        public async Task FetchOrderBook()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(OrderBookUrl);
                var orderBookData = JsonConvert.DeserializeObject<OrderBookResponse>(response);

                var bids = orderBookData.Bids.Select(b =>
                {
                    decimal value;
                    decimal.TryParse(b[1], NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                    return new OrderBookItem
                    {
                        Name = b[0],
                        Value = value,
                        Timestamp = DateTime.UtcNow,
                        Type = OrderBookItemType.Bid,
                    };
                }).Take(OrderBookItemsToTake).OrderBy(b => decimal.Parse(b.Name)).ToList();

                var asks = orderBookData.Asks.Select(a =>
                {
                    decimal value;
                    decimal.TryParse(a[1], NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                    return new OrderBookItem
                    {
                        Name = a[0],
                        Value = value,
                        Timestamp = DateTime.UtcNow,
                        Type = OrderBookItemType.Ask
                    };
                }).Take(OrderBookItemsToTake).ToList();

                _dbContext.OrderBookItems.AddRange(bids.Concat(asks));
                await _dbContext.SaveChangesAsync();

                orderBookData.BidsItems = bids;
                orderBookData.AsksItems = asks;

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
