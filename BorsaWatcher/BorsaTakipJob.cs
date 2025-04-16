using Quartz;

namespace BorsaWatcher;
public class BorsaTakipJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Fiyat kontrol ediliyor...");
        await Program.GetStockData(); // Bu metodu birazdan tanımlayacağız
    }
}