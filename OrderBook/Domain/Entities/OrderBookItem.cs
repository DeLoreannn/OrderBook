using OrderBook.Domain.Enums;

namespace OrderBook.Domain.Entities
{
    public class OrderBookItem
    {
        public int Id { get; set; }
        public decimal? Price { get; set; }
        public decimal? Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public OrderBookItemType Type { get; set; }
    }
}
