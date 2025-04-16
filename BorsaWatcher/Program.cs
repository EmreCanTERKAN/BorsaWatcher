using HtmlAgilityPack;
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine(" 📈 Borsa Takip Uygulaması Başladı!");

string url = "https://bigpara.hurriyet.com.tr/borsa/hisse-fiyatlari/asels-aselsan-detay/";

try
{
    var httpClient = new HttpClient();

    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml");
    httpClient.DefaultRequestHeaders.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");

    var html = await httpClient.GetStringAsync(url);

    var htmlDoc = new HtmlDocument();
    htmlDoc.LoadHtml(html);

    var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//li[contains(@class,'type')]/span[@class='value']");

    if (priceNode != null)
    {
        Console.WriteLine($"ASELS Anlık Fiyatı: {priceNode.InnerText} ₺");
    }
    else
    {
        Console.WriteLine("❌ Fiyat bilgisi bulunamadı.");
    }
}
catch (Exception ex)
{

    Console.WriteLine($"❗ Hata oluştu: {ex.Message}");
}
