using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using WpfApp1;
using WpfOrderBookApp.Models;
using WpfOrderBookApp.Services;
using WpfOrderBookApp.Utils;

namespace WpfOrderBookApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<OrderBookLevel> BidsCollection { get; set; } = new ObservableCollection<OrderBookLevel>();
        public ObservableCollection<OrderBookLevel> AsksCollection { get; set; } = new ObservableCollection<OrderBookLevel>();

        private List<OrderBookLevel> _latestBids = new List<OrderBookLevel>();
        private List<OrderBookLevel> _latestAsks = new List<OrderBookLevel>();

        private readonly Timer _uiUpdateTimer;
        private BinanceService _binanceService;
        private BybitService _bybitService;

        private OrderBook _latestBinanceOrderBook;
        private OrderBook _latestBybitOrderBook;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _binanceService = new BinanceService();
            _binanceService.OnOrderBookUpdate += OnBinanceOrderBookReceived;

            _bybitService = new BybitService();
            _bybitService.OnOrderBookUpdate += OnBybitOrderBookReceived;

            _uiUpdateTimer = new Timer(100);
            _uiUpdateTimer.Elapsed += UpdateUi;
            _uiUpdateTimer.Start();
        }

        public async System.Threading.Tasks.Task StartAsync()
        {
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await _binanceService.ConnectAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Binance WebSocket error: {ex.Message}");
                }
            });

            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await _bybitService.ConnectAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bybit WebSocket error: {ex.Message}");
                }
            });
        }

        private void OnBinanceOrderBookReceived(OrderBook orderBook)
        {
            _latestBinanceOrderBook = orderBook;
            Console.WriteLine($"Received Binance order book: {orderBook.Bids.Count} bids, {orderBook.Asks.Count} asks");
            BinaryWriterHelper.WriteOrderBook("orderbook.bin", orderBook, 1); // Binance (ExchangeId=1)
            AggregateAndSave();
        }

        private void OnBybitOrderBookReceived(OrderBook orderBook)
        {
            _latestBybitOrderBook = orderBook;
            Console.WriteLine($"Received Bybit order book: {orderBook.Bids.Count} bids, {orderBook.Asks.Count} asks");
            BinaryWriterHelper.WriteOrderBook("orderbook.bin", orderBook, 0); // Bybit (ExchangeId=0)
            AggregateAndSave();
        }

        private void AggregateAndSave()
        {
            var aggregatedBids = new Dictionary<decimal, decimal>();
            var aggregatedAsks = new Dictionary<decimal, decimal>();

            if (_latestBinanceOrderBook != null)
            {
                foreach (var bid in _latestBinanceOrderBook.Bids)
                {
                    if (aggregatedBids.ContainsKey(bid.Price))
                        aggregatedBids[bid.Price] += bid.Quantity;
                    else
                        aggregatedBids[bid.Price] = bid.Quantity;
                }
                foreach (var ask in _latestBinanceOrderBook.Asks)
                {
                    if (aggregatedAsks.ContainsKey(ask.Price))
                        aggregatedAsks[ask.Price] += ask.Quantity;
                    else
                        aggregatedAsks[ask.Price] = ask.Quantity;
                }
            }

            if (_latestBybitOrderBook != null)
            {
                foreach (var bid in _latestBybitOrderBook.Bids)
                {
                    if (aggregatedBids.ContainsKey(bid.Price))
                        aggregatedBids[bid.Price] += bid.Quantity;
                    else
                        aggregatedBids[bid.Price] = bid.Quantity;
                }
                foreach (var ask in _latestBybitOrderBook.Asks)
                {
                    if (aggregatedAsks.ContainsKey(ask.Price))
                        aggregatedAsks[ask.Price] += ask.Quantity;
                    else
                        aggregatedAsks[ask.Price] = ask.Quantity;
                }
            }

            _latestBids = aggregatedBids.Select(kv => new OrderBookLevel { Price = kv.Key, Quantity = kv.Value })
                .OrderByDescending(b => b.Price).Take(10).ToList();
            _latestAsks = aggregatedAsks.Select(kv => new OrderBookLevel { Price = kv.Key, Quantity = kv.Value })
                .OrderBy(a => a.Price).Take(10).ToList();

            Console.WriteLine($"Aggregated: {_latestBids.Count} bids, {_latestAsks.Count} asks");

            var aggregatedOrderBook = new OrderBook
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Bids = _latestBids,
                Asks = _latestAsks
            };

            BinaryWriterHelper.WriteOrderBook("orderbook.bin", aggregatedOrderBook, 255); 

            App.Current.Dispatcher.Invoke(() =>
            {
                BidsCollection.Clear();
                foreach (var bid in _latestBids)
                    BidsCollection.Add(bid);

                AsksCollection.Clear();
                foreach (var ask in _latestAsks)
                    AsksCollection.Add(ask);
            });
        }

        private void UpdateUi(object sender, ElapsedEventArgs e)
        {
            
        }
    }
}