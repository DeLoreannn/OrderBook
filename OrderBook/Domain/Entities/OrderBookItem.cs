using OrderBook.Domain.Enums;

namespace OrderBook.Domain.Entities
{
    public class OrderBookItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Value { get; set; }
        public DateTime Timestamp { get; set; }
        public OrderBookItemType Type { get; set; }
    }
}
