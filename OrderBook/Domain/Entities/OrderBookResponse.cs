using Newtonsoft.Json;

namespace OrderBook.Domain.Entities
{
    public class OrderBookResponse
    {
        [JsonProperty("bids")]
        public string[][] Bids { get; set; }
        public List<OrderBookItem> BidsItems { get; set; }

        [JsonProperty("asks")]
        public string[][] Asks { get; set; }
        public List<OrderBookItem> AsksItems { get; set; }
    }
}
