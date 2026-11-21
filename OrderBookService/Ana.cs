using Binance.Net.Clients;
using Binance.Net.Objects.Models;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1;





public class OrderBookService
{


    public string Symbol { get; set; }
    public int Levels { get; set; }
    public int Depth { get; set; }


    public event EventHandler<OrderBook> OnOrderBookChanged;

    // Private Fields
    private readonly BlockingCollection<BookInQueue> _queue = [];
    private readonly List<UpdateSubscription> _subscriptions = [];
    private readonly IConfiguration _configuration;
    private readonly OrderBook _book;
    private readonly Bootloader _loader;
    private CancellationToken _ct;





    protected BinanceSocketClient SocketClient { get => _loader.SocketClient; }


    public OrderBookService( Bootloader loader, string symbol)
    {

        _loader = loader;

        _book = new OrderBook(symbol);

        Symbol = symbol;

    }

    public async Task StartAsync(CancellationToken token)
    {

        _ = Task.Factory.StartNew(SubscribeToPartialUpdatesAsync, TaskCreationOptions.LongRunning);

        _ = Task.Factory.StartNew(OrderBookBuilderAsync, TaskCreationOptions.LongRunning);

        // Dummy
        await Task.CompletedTask;
    }


    private async Task SubscribeToPartialUpdatesAsync()
    {
        // Subscription
        var subs = await SocketClient.SpotApi.ExchangeData.SubscribeToPartialOrderBookUpdatesAsync(Symbol, 10, 100, data =>
        {
            Enqueue(new BookInQueue
            {
                Symbol = Symbol,
                Source = OrderBookSource.Partial,
                OrderBook = data.Data,
            });
        }, _ct);

        // Check Point
        if (subs.Success)
        {
            // Add
            _subscriptions.Add(subs.Data);

        }
        else
        {

        }
    }


    private void Enqueue(BookInQueue book)
    {
        _queue.TryAdd(book);
    }



    private async Task OrderBookBuilderAsync()
    {
        // Dummy
        await Task.CompletedTask;

        foreach (var book in _queue.GetConsumingEnumerable(_ct))
        {

            if (book.Source == OrderBookSource.Best)
            {
                // Best Rows
                var bask = book.OrderBook.Asks.FirstOrDefault();
                var bbid = book.OrderBook.Bids.FirstOrDefault();

                // Merge Rows
                AddOrUpdate(OrderBookSide.Ask, bask);
                AddOrUpdate(OrderBookSide.Bid, bbid);

                // Remove Crossed
                foreach (var row in _book.Asks.Where(x => x.Price < bask.Price)) row.Quantity = 0;
                foreach (var row in _book.Bids.Where(x => x.Price > bbid.Price)) row.Quantity = 0;
            }
            else if (book.Source == OrderBookSource.Partial)
            {
                // Best Rows
                var bask = book.OrderBook.Asks.FirstOrDefault();
                var bbid = book.OrderBook.Bids.FirstOrDefault();

                // Merge Rows
                foreach (var ask in book.OrderBook.Asks) AddOrUpdate(OrderBookSide.Ask, ask);
                foreach (var bid in book.OrderBook.Bids) AddOrUpdate(OrderBookSide.Bid, bid);

                // Remove Crossed
                if (bask != null) foreach (var row in _book.Asks.Where(x => x.Price < bask.Price)) row.Quantity = 0;
                if (bbid != null) foreach (var row in _book.Bids.Where(x => x.Price > bbid.Price)) row.Quantity = 0;
            }
            else if (book.Source == OrderBookSource.Updates)
            {
                // Merge Rows
                foreach (var ask in book.OrderBook.Asks) AddOrUpdate(OrderBookSide.Ask, ask);
                foreach (var bid in book.OrderBook.Bids) AddOrUpdate(OrderBookSide.Bid, bid);
            }

            // Remove Zeroes
            _book.Asks.RemoveAll(e => e.Quantity == 0);
            _book.Bids.RemoveAll(e => e.Quantity == 0);

            // Sort & Take
            _book.Asks = _book.Asks.OrderBy(e => e.Price).Take(10).ToList();
            _book.Bids = _book.Bids.OrderByDescending(e => e.Price).Take(10).ToList();

            // OnOrderBookChanged
            OnOrderBookChanged?.Invoke(this, _book);
        }
    }


    private void AddOrUpdate(OrderBookSide side, BinanceOrderBookEntry entry)
    {
        var rows = side == OrderBookSide.Ask ? _book.Asks : _book.Bids;
        var row = rows.FirstOrDefault(e => e.Price == entry.Price);
        if (row == null) rows.Add(entry);

    }
}



internal static class ConfigurationExtensions
{
    public static string GetConfiguration(this IConfiguration @this, string key)
    {
        if (@this == null) return string.Empty;
        if (string.IsNullOrEmpty(key)) return string.Empty;

        return @this.GetSection(key).Value;
    }

    public static T GetConfiguration<T>(this IConfiguration @this, string key)
    {
        if (@this == null) return default;
        if (string.IsNullOrEmpty(key)) return default;

        return @this.GetSection(key).Get<T>();
    }
}

