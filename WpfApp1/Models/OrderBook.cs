using System.Collections.Generic;

namespace WpfOrderBookApp.Models
{
    public class OrderBookLevel
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }

    public class OrderBook
    {
        public long Timestamp { get; set; }
        public List<OrderBookLevel> Bids { get; set; } = new List<OrderBookLevel>();
        public List<OrderBookLevel> Asks { get; set; } = new List<OrderBookLevel>();
    }
}