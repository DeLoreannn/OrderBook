using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using OrderBook.Domain;
using OrderBook.Responses;
using OrderBook.Services;

public class BitstampServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly OrderBookDbContext _dbContext;
    private readonly BitstampService _bitstampService;

    public BitstampServiceTests()
    {
        var options = new DbContextOptionsBuilder<OrderBookDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _dbContext = new OrderBookDbContext(options);
        _mockHandler = new Mock<HttpMessageHandler>();
        var mockHttpClient = new HttpClient(_mockHandler.Object);
        _bitstampService = new BitstampService(mockHttpClient, _dbContext);
    }

    [Fact]
    public async Task FetchOrderBook_ValidResponse_UpdatesOrderBookAndTriggersEvent()
    {
        // Arrange
        var orderBookResponse = new OrderBookResponse
        {
            Bids = new string[][]
            {
            new string[] { "10000", "0.5" }
            },
            Asks = new string[][]
            {
            new string[] { "11000", "1.0" }
            },
            Timestamp = "1738146643"
        };
        var jsonResponse = JsonConvert.SerializeObject(orderBookResponse);

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(jsonResponse),
            });
        bool eventTriggered = false;
        OrderBookUpdate receivedOrderBook = null;

        _bitstampService.OnOrderBookUpdated += (updatedOrderBook) =>
        {
            eventTriggered = updatedOrderBook != null;
            receivedOrderBook = updatedOrderBook;
        };

        // Act
        await _bitstampService.FetchOrderBook();

        // Assert
        Assert.True(eventTriggered, "The OnOrderBookUpdated event was not triggered.");
        Assert.NotNull(receivedOrderBook);
        Assert.Equal(1, receivedOrderBook.Bids.Count);
        Assert.Equal(1, receivedOrderBook.Asks.Count);
        Assert.Equal(2, _dbContext.OrderBookItems.Count());
        Assert.Contains(_dbContext.OrderBookItems, i => i.Price == 10000 && i.Type == OrderBook.Domain.Enums.OrderBookItemType.Bid);
        Assert.Contains(_dbContext.OrderBookItems, i => i.Price == 11000 && i.Type == OrderBook.Domain.Enums.OrderBookItemType.Ask);
    }

    [Fact]
    public async Task FetchOrderBook_ExceptionOccurs_DoesNotTriggerEvent()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Network error"));

        bool eventTriggered = false;
        _bitstampService.OnOrderBookUpdated += (updatedOrderBook) =>
        {
            eventTriggered = updatedOrderBook != null;
        };

        // Act
        await _bitstampService.FetchOrderBook();

        // Assert
        Assert.False(eventTriggered);
    }
}
