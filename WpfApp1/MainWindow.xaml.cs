using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using WpfOrderBookApp.Utils;
using WpfOrderBookApp.ViewModels;

namespace WpfOrderBookApp
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private static readonly object _fileLock = new object(); // Общий объект для блокировки

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;

            Loaded += async (s, e) => await _vm.StartAsync();
        }

        private async void ViewHistoricalOrderBook_Click(object sender, RoutedEventArgs e)
        {
            string sourceFile = "orderbook.bin";
            string tempFile = Path.GetTempFileName();

            try
            {
                if (!File.Exists(sourceFile))
                {
                    MessageBox.Show("File 'orderbook.bin' not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Копируем под lock, чтобы избежать записи во время копирования
                lock (_fileLock) // Используем общий объект блокировки
                {
                    Console.WriteLine($"Copying {sourceFile} to {tempFile}");
                    File.Copy(sourceFile, tempFile, true);
                    Console.WriteLine("File copied successfully");
                }

                var orderBooks = await Task.Run(() => BinaryReaderHelper.ReadOrderBook(tempFile));
                Console.WriteLine($"Read {orderBooks.Count} snapshots from {tempFile}");

                var orderBooksWithExchange = orderBooks.Select(ob => (OrderBook: ob.OrderBook, ExchangeId: ob.ExchangeId)).ToList();

                if (orderBooksWithExchange.Any())
                {
                    var historyWindow = new HistoryWindow(orderBooksWithExchange);
                    historyWindow.Show();
                }
                else
                {
                    MessageBox.Show("No data found in the file.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ViewHistoricalOrderBook: {ex.Message}");
                MessageBox.Show($"Failed to load historical data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    Console.WriteLine($"Deleting temporary file {tempFile}");
                    File.Delete(tempFile);
                }
            }
        }
    }
}