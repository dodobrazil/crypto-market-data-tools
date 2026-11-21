using System;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

class Program
{
    static Dictionary<string, TickerData> latestData = new Dictionary<string, TickerData>();
    static System.Timers.Timer displayTimer;

    static void Main(string[] args)
    {
        // Binance WebSocket endpoint for ticker updates (top 20 symbols in a multi-stream)
        string url = "wss://stream.binance.com:9443/stream?streams=";

        // List of top 20 cryptocurrency symbols
        List<string> symbols = new List<string>
        {
            "btcusdt", "ethusdt", "bnbusdt", "xrpusdt", "solusdt",
            "adausdt", "dogeusdt", "maticusdt", "dotusdt", "ltcusdt",
            "shibusdt", "trxusdt", "avaxusdt", "uniusdt", "linkusdt",
            "xlmusdt", "atomusdt", "qntusdt", "filusdt", "etcusdt"
        };

        // Create the subscription message to request updates for multiple symbols
        string streams = string.Join("/", symbols.ConvertAll(s => s + "@ticker"));
        string fullUrl = url + streams;

        Console.WriteLine("Connecting to URL: " + fullUrl);

        using (var ws = new WebSocket(fullUrl))
        {
            ws.OnOpen += (sender, e) =>
            {
                Console.WriteLine("WebSocket bağlantısı açıldı.");
            };

            ws.OnMessage += (sender, e) =>
            {
                try
                {
                    // Parse the incoming JSON message dynamically
                    var response = JObject.Parse(e.Data);
                    var data = response["data"];

                    if (data != null)
                    {
                        // Extract relevant fields
                        string symbol = data["s"]?.ToString();
                        string currentPrice = data["c"]?.ToString();
                        string highPrice = data["h"]?.ToString();
                        string lowPrice = data["l"]?.ToString();
                        string volume = data["v"]?.ToString();

                        // Update or add the data to the dictionary
                        latestData[symbol] = new TickerData
                        {
                            Symbol = symbol,
                            CurrentPrice = currentPrice,
                            HighPrice = highPrice,
                            LowPrice = lowPrice,
                            Volume = volume
                        };
                    }
                    else
                    {
                        Console.WriteLine("Invalid data received.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Veriyi ayrıştırırken hata: " + ex.Message);
                }
            };

            ws.OnClose += (sender, e) =>
            {
                Console.WriteLine("WebSocket bağlantısı kapandı.");
            };

            ws.Connect();

            // Set up a timer to refresh the display every 2 seconds
            displayTimer = new System.Timers.Timer(2000);
            displayTimer.Elapsed += DisplayData;
            displayTimer.AutoReset = true;
            displayTimer.Start();

            Console.WriteLine("Veri alınıyor, çıkmak için bir tuşa basın...");
            Console.ReadKey();

            // Stop the timer when done
            displayTimer.Stop();
            displayTimer.Dispose();
        }
    }

    static void DisplayData(object sender, ElapsedEventArgs e)
    {
        Console.Clear();
        Console.WriteLine("Real-Time Cryptocurrency Dashboard");
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine("| Symbol   | Current Price (USD) | High Price (24h) | Low Price (24h) | 24h Volume |");
        Console.WriteLine("-------------------------------------------------------------");

        // Print data side by side in a tabular format
        foreach (var ticker in latestData.Values.OrderBy(t => t.Symbol))
        {
            Console.WriteLine($"| {ticker.Symbol,-8} | {ticker.CurrentPrice,18} | {ticker.HighPrice,15} | {ticker.LowPrice,15} | {ticker.Volume,10} |");
        }

        Console.WriteLine("-------------------------------------------------------------");
    }
}

// Class to store ticker data for each symbol
public class TickerData
{
    public string Symbol { get; set; }
    public string CurrentPrice { get; set; }
    public string HighPrice { get; set; }
    public string LowPrice { get; set; }
    public string Volume { get; set; }
}
