using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        string url = "https://www.okx.com/api/v5/market/ticker?instId=USDT-TRY";

        while (true) // Sürekli döngü
        {
            try
            {
                // API'den veri çek
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // JSON'u işle
                JsonDocument json = JsonDocument.Parse(responseBody);
                JsonElement dataArray = json.RootElement.GetProperty("data");

                if (dataArray.GetArrayLength() > 0)
                {
                    JsonElement result = dataArray[0];

                    // Gerekli bilgileri al
                    string lastPrice = result.GetProperty("last").GetString();
                    string volume24h = result.GetProperty("vol24h").GetString();
                    string open24h = result.GetProperty("open24h").GetString();
                    string timestamp = result.GetProperty("ts").GetString();

                    // 24 saatlik değişim oranını hesapla
                    decimal last = decimal.Parse(lastPrice);
                    decimal open = decimal.Parse(open24h);
                    decimal change24h = ((last - open) / open) * 100;

                    // Zaman damgasını insan okunabilir bir forma çevir
                    long unixTime = long.Parse(timestamp);
                    DateTime lastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(unixTime).LocalDateTime;

                    // Bilgileri konsolda göster
                    Console.Clear(); // Önceki verileri temizle
                    Console.WriteLine($"--USD/TRY--");
                    Console.WriteLine($"Anlık Fiyat: {lastPrice} TRY");
                    Console.WriteLine($"24 Saatlik Hacim: {volume24h}");
                    Console.WriteLine($"24 Saatlik Değişim: {change24h:F2}%");
                    Console.WriteLine($"Son Güncelleme: {lastUpdated}");
                }
                else
                {
                    Console.WriteLine("Data dizisi boş! API'den beklenen veri gelmedi.");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"İstek Hatası: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Genel Hata: {e.Message}");
            }

            // Yeniden istek için bekleme süresi
            await Task.Delay(350); // 350 ms bekle
        }
    }
}
