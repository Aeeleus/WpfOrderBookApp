using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using WpfOrderBookApp.Models;

namespace WpfOrderBookApp.Utils
{
    public static class BinaryReaderHelper
    {
        public static List<(OrderBook OrderBook, byte ExchangeId)> ReadOrderBook(string filePath)
        {
            var orderBooks = new List<(OrderBook, byte)>();
            Console.WriteLine($"Attempting to read file: {filePath}");

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new BinaryReader(stream))
                {
                    Console.WriteLine($"File size: {stream.Length} bytes");
                    while (stream.Position < stream.Length)
                    {
                        try
                        {
                            var orderBook = new OrderBook();
                            orderBook.Timestamp = reader.ReadInt64();
                            byte exchangeId = reader.ReadByte();

                            Console.WriteLine($"Reading snapshot at timestamp: {orderBook.Timestamp}, ExchangeId: {exchangeId}");

                            int bidsCount = reader.ReadInt32();
                            Console.WriteLine($"Bids count: {bidsCount}");
                            orderBook.Bids = new List<OrderBookLevel>(bidsCount);
                            for (int i = 0; i < bidsCount; i++)
                            {
                                decimal price = (decimal)reader.ReadDouble();
                                decimal quantity = (decimal)reader.ReadDouble();
                                orderBook.Bids.Add(new OrderBookLevel { Price = price, Quantity = quantity });
                            }

                            int asksCount = reader.ReadInt32();
                            Console.WriteLine($"Asks count: {asksCount}");
                            orderBook.Asks = new List<OrderBookLevel>(asksCount);
                            for (int i = 0; i < asksCount; i++)
                            {
                                decimal price = (decimal)reader.ReadDouble();
                                decimal quantity = (decimal)reader.ReadDouble();
                                orderBook.Asks.Add(new OrderBookLevel { Price = price, Quantity = quantity });
                            }

                            orderBooks.Add((orderBook, exchangeId));
                            Console.WriteLine($"Successfully read snapshot with {bidsCount} bids and {asksCount} asks");
                        }
                        catch (EndOfStreamException)
                        {
                            Console.WriteLine("End of file reached");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading snapshot at position {stream.Position}: {ex.Message}");
                            break;
                        }
                    }
                    Console.WriteLine($"Total snapshots read: {orderBooks.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening file {filePath}: {ex.Message}");
                MessageBox.Show($"Failed to read file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return orderBooks;
        }
    }
}