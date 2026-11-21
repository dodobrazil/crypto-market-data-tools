using Binance.Net.Objects.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class OrderBook(string symbol)
{
    public string Symbol { get; set; } = symbol;
    public List<BinanceOrderBookEntry> Asks { get; set; } = [];
    public List<BinanceOrderBookEntry> Bids { get; set; } = [];
}

public static class Integrator
{
    public async static void SendAsync(string message, int port)
    {
        UdpClient udp = new UdpClient();
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse("255.255.255.255"), port);
        byte[] bytes = Encoding.ASCII.GetBytes(message);
        udp.SendAsync(bytes, bytes.Length, ip);
        udp.Close();
    }

    public async static void OnOrderBookUpdate(object sender, OrderBook book)
    {
        // Do something with the order book
        var bbid = book.Bids.FirstOrDefault();
        var bask = book.Asks.FirstOrDefault();


        if (bbid != null && bask != null)
        {
            string text = string.Empty;
            DateTime dt = DateTime.Now;
            text += book.Symbol + "@" + ((bbid.Price + bask.Price) / 2.0m).ToString("F8") + "@" + dt.Ticks;

            
            SendAsync(text,5000);
            Console.WriteLine(text);
        }
    }
}
