using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WpfOrderBookApp.Models;

namespace WpfOrderBookApp.Services
{
    public class BybitService
    {
        private ClientWebSocket _ws;
        public event Action<OrderBook> OnOrderBookUpdate;

        private SortedDictionary<decimal, decimal> _currentBids = new SortedDictionary<decimal, decimal>(Comparer<decimal>.Create((x, y) => y.CompareTo(x))); // Descending prices
        private SortedDictionary<decimal, decimal> _currentAsks = new SortedDictionary<decimal, decimal>(); 

        public async Task ConnectAsync(string symbol = "BTCUSDT")
        {
            _ws = new ClientWebSocket();
            try
            {
                var url = "wss://stream.bybit.com/v5/public/linear";
                await _ws.ConnectAsync(new Uri(url), CancellationToken.None);

                Console.WriteLine("Connected to Bybit WebSocket");

                var subscribeMsg = "{\"op\": \"subscribe\", \"args\": [\"orderbook.50." + symbol + "\"]}";
                await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(subscribeMsg)), WebSocketMessageType.Text, true, CancellationToken.None);

                var buffer = new byte[8192];

                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Console.WriteLine(msg);

                    ParseMessage(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bybit WebSocket error: {ex.Message}");
            }
        }

        private void ParseMessage(string json)
        {
            try
            {
                var root = JObject.Parse(json);

                if (root["topic"] == null) return;

                var type = (string)root["type"];
                var data = root["data"] as JObject;

                if (data == null) return;

                long timestamp = (long)root["ts"];

                if (type == "snapshot")
                {
                    _currentBids.Clear();
                    _currentAsks.Clear();

                    var bidsArray = data["b"] as JArray;
                    if (bidsArray != null)
                    {
                        foreach (var bid in bidsArray)
                        {
                            if (decimal.TryParse((string)bid[0], out var price) &&
                                decimal.TryParse((string)bid[1], out var quantity))
                            {
                                _currentBids[price] = quantity;
                            }
                        }
                    }

                    var asksArray = data["a"] as JArray;
                    if (asksArray != null)
                    {
                        foreach (var ask in asksArray)
                        {
                            if (decimal.TryParse((string)ask[0], out var price) &&
                                decimal.TryParse((string)ask[1], out var quantity))
                            {
                                _currentAsks[price] = quantity;
                            }
                        }
                    }
                }
                else if (type == "delta")
                {
                    var bidsArray = data["b"] as JArray;
                    if (bidsArray != null)
                    {
                        foreach (var bid in bidsArray)
                        {
                            if (decimal.TryParse((string)bid[0], out var price) &&
                                decimal.TryParse((string)bid[1], out var quantity))
                            {
                                if (quantity == 0)
                                {
                                    _currentBids.Remove(price);
                                }
                                else
                                {
                                    _currentBids[price] = quantity;
                                }
                            }
                        }
                    }

                    var asksArray = data["a"] as JArray;
                    if (asksArray != null)
                    {
                        foreach (var ask in asksArray)
                        {
                            if (decimal.TryParse((string)ask[0], out var price) &&
                                decimal.TryParse((string)ask[1], out var quantity))
                            {
                                if (quantity == 0)
                                {
                                    _currentAsks.Remove(price);
                                }
                                else
                                {
                                    _currentAsks[price] = quantity;
                                }
                            }
                        }
                    }
                }

                var orderBook = new OrderBook
                {
                    Timestamp = timestamp,
                    Bids = _currentBids.Select(kv => new OrderBookLevel { Price = kv.Key, Quantity = kv.Value }).ToList(),
                    Asks = _currentAsks.Select(kv => new OrderBookLevel { Price = kv.Key, Quantity = kv.Value }).ToList()
                };

                orderBook.Bids = orderBook.Bids.Take(5).ToList();
                orderBook.Asks = orderBook.Asks.Take(5).ToList();

                OnOrderBookUpdate?.Invoke(orderBook);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bybit parse error: {ex.Message}");
            }
        }
    }
}