using System.IO;
using WpfOrderBookApp.Models;

namespace WpfOrderBookApp.Utils
{
    public static class BinaryWriterHelper
    {
        public static void WriteOrderBook(string filePath, OrderBook orderBook)
        {
            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(orderBook.Timestamp);

                writer.Write(orderBook.Bids.Count);
                foreach (var bid in orderBook.Bids)
                {
                    writer.Write(bid.Price);
                    writer.Write(bid.Quantity);
                }

                writer.Write(orderBook.Asks.Count);
                foreach (var ask in orderBook.Asks)
                {
                    writer.Write(ask.Price);
                    writer.Write(ask.Quantity);
                }
            }
        }
    }
}