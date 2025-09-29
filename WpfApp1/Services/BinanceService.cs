using Newtonsoft.Json.Linq;
using System;
using System.Globalization; 
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfOrderBookApp.Models;

namespace WpfOrderBookApp.Services
{
    public class BinanceService
    {
        private ClientWebSocket _ws;
        public event Action<OrderBook> OnOrderBookUpdate;

        public async Task ConnectAsync(string symbol = "btcusdt")
        {
            _ws = new ClientWebSocket();
            try
            {
                var url = $"wss://fstream.binance.com/ws/{symbol}@depth5@100ms";
                await _ws.ConnectAsync(new Uri(url), CancellationToken.None);

                Console.WriteLine("Connected to Binance WebSocket");

                var buffer = new byte[8192];

                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Console.WriteLine($"Binance message: {msg}");

                    ParseMessage(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Binance WebSocket error: {ex.Message}");
            }
        }

        private void ParseMessage(string json)
        {
            try
            {
                var root = JObject.Parse(json);

                var orderBook = new OrderBook
                {
                    Timestamp = (long)root["E"]
                };

                var bidsArray = root["b"] as JArray;
                if (bidsArray != null)
                {
                    foreach (var bid in bidsArray)
                    {
                        if (decimal.TryParse((string)bid[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
                            decimal.TryParse((string)bid[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
                        {
                            orderBook.Bids.Add(new OrderBookLevel { Price = price, Quantity = quantity });
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse Binance bid: {bid.ToString()}");
                        }
                    }
                }

                var asksArray = root["a"] as JArray;
                if (asksArray != null)
                {
                    foreach (var ask in asksArray)
                    {
                        if (decimal.TryParse((string)ask[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
                            decimal.TryParse((string)ask[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
                        {
                            orderBook.Asks.Add(new OrderBookLevel { Price = price, Quantity = quantity });
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse Binance ask: {ask.ToString()}");
                        }
                    }
                }

                if (orderBook.Bids.Any() || orderBook.Asks.Any())
                {
                    orderBook.Bids = orderBook.Bids.OrderByDescending(b => b.Price).ToList();
                    orderBook.Asks = orderBook.Asks.OrderBy(a => a.Price).ToList();

                    Console.WriteLine($"Binance parsed: {orderBook.Bids.Count} bids, {orderBook.Asks.Count} asks");
                    OnOrderBookUpdate?.Invoke(orderBook);
                }
                else
                {
                    Console.WriteLine("No bids or asks parsed from Binance.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Binance parse error: {ex.Message}");
            }
        }
    }
}