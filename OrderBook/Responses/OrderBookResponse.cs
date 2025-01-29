using Newtonsoft.Json;
using OrderBook.Domain.Entities;

namespace OrderBook.Responses
{
    public class OrderBookResponse
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("bids")]
        public string[][] Bids { get; set; }

        [JsonProperty("asks")]
        public string[][] Asks { get; set; }
    }
}
