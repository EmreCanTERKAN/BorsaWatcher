using BorsaWatcher;
using HtmlAgilityPack;
using Quartz;
using Quartz.Impl;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Quartz scheduler başlatılıyor
        StdSchedulerFactory factory = new StdSchedulerFactory();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.Start();

        // Job tanımı
        IJobDetail job = JobBuilder.Create<BorsaTakipJob>()
            .WithIdentity("borsaJob", "group1")
            .Build();

        // Zamanlayıcı: her 1 dakikada bir çalıştır
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("borsaTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(3)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        Console.WriteLine("📈 Borsa Takip Uygulaması başlatıldı. Çıkmak için Enter'a bas...");
        Console.ReadLine(); // uygulamanın hemen kapanmaması için
    }

    public static async Task GetStockData()
    {
        string url = "https://bigpara.hurriyet.com.tr/borsa/hisse-fiyatlari/asels-aselsan-detay/";

        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var html = await httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//li[contains(@class,'type')]/span[@class='value']");

            if (priceNode != null)
            {
                var price = priceNode.InnerText.Trim();
                Console.WriteLine($"✅ ASELS Anlık Fiyatı: {price} ₺");
            }
            else
            {
                Console.WriteLine("❌ Fiyat bilgisi bulunamadı.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Hata oluştu: {ex.Message}");
        }
    }
}
