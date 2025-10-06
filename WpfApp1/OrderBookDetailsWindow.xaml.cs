using System.Windows;
using WpfOrderBookApp.Models;

namespace WpfOrderBookApp
{
    public partial class OrderBookDetailsWindow : Window
    {
        public OrderBook OrderBook { get; }

        public OrderBookDetailsWindow(OrderBook orderBook)
        {
            InitializeComponent();
            OrderBook = orderBook;
            DataContext = orderBook;
        }
    }
}