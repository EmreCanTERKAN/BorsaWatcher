using HtmlAgilityPack;
using Quartz;
using Quartz.Impl;
using System.Globalization;
using Microsoft.Extensions.Configuration;

class Program
{

    private static readonly HttpClient httpClient = new HttpClient();
    private static Dictionary<string, Queue<decimal>> fiyatGeçmişi = new();
    private static string? botToken;
    private static string? chatId;

    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<Program>()
            .Build();

        botToken = config["Telegram:BotToken"]!;
        chatId = config["Telegram:ChatId"]!;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        StdSchedulerFactory factory = new StdSchedulerFactory();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.Start();

        IJobDetail job = JobBuilder.Create<BorsaTakipJob>()
            .WithIdentity("borsaJob", "group1")
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("borsaTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(3)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        Console.WriteLine("📈 Çoklu Hisse Takip Başladı. Çıkmak için Enter'a bas...");
        Console.ReadLine();
    }

    public static async Task GetStockData(string url, string hisseAdi, decimal sinyalSeviyesi)
    {
        try
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            var html = await httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//li[contains(@class,'type')]/span[@class='value']");

            if (priceNode != null)
            {
                var rawPrice = priceNode.InnerText.Trim();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {hisseAdi} Fiyatı: {rawPrice} ₺");

                if (decimal.TryParse(rawPrice.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal fiyat))
                {
                    // Hareketli ortalama için
                    if (!fiyatGeçmişi.ContainsKey(hisseAdi))
                        fiyatGeçmişi[hisseAdi] = new Queue<decimal>();

                    fiyatGeçmişi[hisseAdi].Enqueue(fiyat);
                    if (fiyatGeçmişi[hisseAdi].Count > 5)
                        fiyatGeçmişi[hisseAdi].Dequeue();

                    if (fiyatGeçmişi[hisseAdi].Count == 5)
                    {
                        decimal ortalama = fiyatGeçmişi[hisseAdi].Average();
                        Console.WriteLine($"🟢 {hisseAdi} alım sinyali: {fiyat} ₺ (Ort: {ortalama} ₺)");
                        if (fiyat < ortalama * 0.99m)
                        {
                            string mesaj = $"🟢 {hisseAdi} alım sinyali: {fiyat} ₺ (Ort: {ortalama} ₺)";
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(mesaj);
                            Console.ResetColor();
                            await SendTelegramMessage(botToken!, chatId!, mesaj);
                        }
                        else if (fiyat > ortalama * 1.02m)
                        {
                            Console.WriteLine($"🔴 {hisseAdi} satış sinyali: {fiyat} ₺ (Ort: {ortalama} ₺)");
                            string mesaj = $"🔴 {hisseAdi} satış sinyali: {fiyat} ₺ (Ort: {ortalama} ₺)";
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(mesaj);
                            Console.ResetColor();
                            await SendTelegramMessage(botToken!, chatId!, mesaj);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ {hisseAdi} için fiyat bilgisi alınamadı.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ {hisseAdi} için hata: {ex.Message}");
        }

    }


    public static async Task SendTelegramMessage(string botToken, string chatId, string message)
    {
        try
        {
            string url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(message)}";
            using var client = new HttpClient();
            await client.GetAsync(url);
            Console.WriteLine("📩 Telegram mesajı gönderildi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Telegram mesajı gönderilemedi: " + ex.Message);
        }
    }
}
