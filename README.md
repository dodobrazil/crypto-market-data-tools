# Crypto Market Data Tools

A collection of .NET console applications for consuming real-time cryptocurrency market data.

## Projects

| Project | Description |
|---|---|
| `OrderBookService` | Maintains a local Binance order book from WebSocket depth updates using a producer/consumer queue (`BlockingCollection`) and raises change events. Built on `Binance.Net` with a hosted-service bootloader. |
| `BinanceWebSocketDemo` | Subscribes to a combined Binance ticker stream for the top 20 symbols and renders a periodically refreshed console table. |
| `OkxUsdTryTicker` | Polls the OKX REST API for the USDT/TRY ticker and prints live price updates. |

## Requirements

- .NET 8 SDK

## Running

```bash
cd OrderBookService
dotnet run
```

Replace the folder name to run any of the other projects.
