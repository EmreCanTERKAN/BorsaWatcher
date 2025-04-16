using Quartz;

public class BorsaTakipJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var hisseler = new List<HisseTakip>
        {
            new HisseTakip
            {
                Ad = "ASELS",
                Url = "https://bigpara.hurriyet.com.tr/borsa/hisse-fiyatlari/asels-aselsan-detay/",
                AlimSinyalSeviyesi = 40.0m
            },
            new HisseTakip
            {
                Ad = "THYAO",
                Url = "https://bigpara.hurriyet.com.tr/borsa/hisse-fiyatlari/thyao-turk-hava-yollari-detay/",
                AlimSinyalSeviyesi = 400.0m
            }
            // istediğin kadar hisse ekleyebilirsin
        };

        foreach (var hisse in hisseler)
        {
            await Program.GetStockData(hisse.Url, hisse.Ad, hisse.AlimSinyalSeviyesi);
        }
    }
}

public class HisseTakip
{
    public string Ad { get; set; }=default!;
    public string Url { get; set; } = default!;
    public decimal AlimSinyalSeviyesi { get; set; }
}
