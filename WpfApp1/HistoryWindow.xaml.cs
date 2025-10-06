using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfOrderBookApp.Models;
using WpfOrderBookApp.Utils;

namespace WpfOrderBookApp
{
    public class HistoryOrderBookViewModel
    {
        public OrderBook OrderBook { get; set; }
        public string Exchange { get; set; } // Now correctly displays based on ExchangeId
        public DateTimeOffset Timestamp => DateTimeOffset.FromUnixTimeMilliseconds(OrderBook.Timestamp);
    }

    public partial class HistoryWindow : Window
    {
        private readonly ObservableCollection<HistoryOrderBookViewModel> _orderBooks;
        private readonly List<HistoryOrderBookViewModel> _allOrderBooks; // For filtering

        public HistoryWindow(List<(OrderBook OrderBook, byte ExchangeId)> orderBooks)
        {
            InitializeComponent();
            _allOrderBooks = orderBooks.Select(ob => new HistoryOrderBookViewModel
            {
                OrderBook = ob.OrderBook,
                Exchange = ob.ExchangeId == 0 ? "Bybit" : ob.ExchangeId == 1 ? "Binance" : "Aggregated"
            }).ToList();
            _orderBooks = new ObservableCollection<HistoryOrderBookViewModel>(_allOrderBooks);
            OrderBookGrid.ItemsSource = _orderBooks;
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is HistoryOrderBookViewModel viewModel)
            {
                var detailsWindow = new OrderBookDetailsWindow(viewModel.OrderBook);
                detailsWindow.ShowDialog();
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            _orderBooks.Clear();
            long? startTime = long.TryParse(StartTimeFilter.Text, out var start) ? start : (long?)null;
            long? endTime = long.TryParse(EndTimeFilter.Text, out var end) ? end : (long?)null;

            var filtered = _allOrderBooks.Where(vm =>
            {
                bool pass = true;
                if (startTime.HasValue) pass &= vm.OrderBook.Timestamp >= startTime.Value;
                if (endTime.HasValue) pass &= vm.OrderBook.Timestamp <= endTime.Value;
                return pass;
            });

            foreach (var vm in filtered)
                _orderBooks.Add(vm);
        }
    }
}