using System.IO;
using WpfOrderBookApp.Models;

namespace WpfOrderBookApp.Utils
{
    public static class BinaryWriterHelper
    {
        private static readonly object _fileLock = new object();

        public static void WriteOrderBook(string filePath, OrderBook orderBook, byte exchangeId = 255)
        {
            lock (_fileLock)
            {
                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(orderBook.Timestamp);
                    writer.Write(exchangeId); 

                    writer.Write(orderBook.Bids.Count);
                    foreach (var bid in orderBook.Bids)
                    {
                        writer.Write((double)bid.Price);
                        writer.Write((double)bid.Quantity);
                    }

                    writer.Write(orderBook.Asks.Count);
                    foreach (var ask in orderBook.Asks)
                    {
                        writer.Write((double)ask.Price);
                        writer.Write((double)ask.Quantity);
                    }
                }
            }
        }
    }
}