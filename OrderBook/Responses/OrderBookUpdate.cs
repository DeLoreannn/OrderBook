using OrderBook.Domain.Entities;

namespace OrderBook.Responses
{
    public class OrderBookUpdate
    {
        public List<OrderBookItem> Bids { get; set; }
        public List<OrderBookItem> Asks { get; set; }
    }
}
