using ConsoleApp1;
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using Binance.Net.Clients;
using Microsoft.Extensions.Configuration;

public class Bootloader : BackgroundService
{
    public ConcurrentDictionary<string, OrderBookService> OrderBookServices { get; } = [];

    public BinanceSocketClient SocketClient { get; }

    private readonly IConfiguration _configuration;

    public Bootloader(IConfiguration configuration)
    {
        SocketClient = new BinanceSocketClient();

        _configuration = configuration;

        //string[] symbols = { "INJUSDT","LUNCUSDT","AGIXUSDT","AMPUSDT","API3USDT","ARBUSDT","TRXUSDT","FILUSDT","OCEANUSDT","RNDRUSDT","SUSHIUSDT","SOLUSDT","BTCUSDT","BONKUSDT","ASRUSDT","GLMUSDT","USDTTRY" };
        string[] symbols = { "LUNAUSDT","SUPERUSDT","STRKUSDT","SHIBUSDT","FLOKIUSDT","PEPEUSDT","SPELLUSDT", "JASMYUSDT", "LPTUSDT", "GRTUSDT", "GALAUSDT", "ETHUSDT", "XRPUSDT", "CVCUSDT", "CHZUSDT", "AVAXUSDT", "UMAUSDT", "DOGEUSDT", "ANKRUSDT", "ZRXUSDT", "USDTTRY" };


        foreach (var symbol in symbols)
        {
            var orderBookService = new OrderBookService(this, symbol);
            orderBookService.OnOrderBookChanged += Integrator.OnOrderBookUpdate;
            OrderBookServices[symbol] = orderBookService;
        }
     }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {


        // Order Book Services
        foreach (var service in OrderBookServices.Values)
            _ = service.StartAsync(stoppingToken);

        // Dummy
        await Task.CompletedTask;
    }
}