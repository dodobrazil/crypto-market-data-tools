using Binance.Net.Interfaces;



public class BookInQueue
{
    public string Symbol { get; set; }
    public OrderBookSource Source { get; set; }
    public IBinanceOrderBook OrderBook { get; set; }
}

public enum OrderBookSource
{
    Best,
    Partial,
    Updates,
}

public enum OrderBookSide
{
    Ask,
    Bid,
}
