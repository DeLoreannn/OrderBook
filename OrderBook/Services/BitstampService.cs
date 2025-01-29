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

        public event Action<OrderBookUpdate> OnOrderBookUpdated;
        private readonly SemaphoreSlim _dbContextLock = new SemaphoreSlim(1, 1);

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
                await _dbContextLock.WaitAsync();

                var response = await _httpClient.GetStringAsync(OrderBookUrl);
                var orderBookData = JsonConvert.DeserializeObject<OrderBookResponse>(response);
                var timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(orderBookData.Timestamp)).UtcDateTime;

                var bids = orderBookData.Bids.Select(b =>
                {
                    decimal.TryParse(b[0], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);
                    decimal.TryParse(b[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount);
                    return new OrderBookItem
                    {
                        Price = price,
                        Amount = amount,
                        Timestamp = timestamp,
                        Type = OrderBookItemType.Bid,
                    };
                }).Take(OrderBookItemsToTake).OrderBy(b => b.Price).ToList();

                var asks = orderBookData.Asks.Select(a =>
                {
                    decimal.TryParse(a[0], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);
                    decimal.TryParse(a[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount);
                    return new OrderBookItem
                    {
                        Price = price,
                        Amount = amount,
                        Timestamp = timestamp,
                        Type = OrderBookItemType.Ask
                    };
                }).Take(OrderBookItemsToTake).ToList();

                _dbContext.OrderBookItems.AddRange(bids.Concat(asks));
                await _dbContext.SaveChangesAsync();

                OnOrderBookUpdated?.Invoke(new OrderBookUpdate
                {
                    Bids = bids,
                    Asks = asks,
                });
            }
            catch (Exception ex)
            {
                OnOrderBookUpdated?.Invoke(null);
            }
            finally
            {
                _dbContextLock.Release();
            }
        }

        public void StopFetchingOrderBook()
        {
            _timer?.Dispose();
        }
    }
}
