using System.Globalization;

public class FiyatTakip
{
    private readonly Queue<decimal> sonFiyatlar = new();
    private const int OrtalamaPencere = 5;

    public async Task FiyatKontrolEt(string hisseAdi, decimal mevcutFiyat)
    {
        sonFiyatlar.Enqueue(mevcutFiyat);

        if (sonFiyatlar.Count > OrtalamaPencere)
            sonFiyatlar.Dequeue();

        if (sonFiyatlar.Count == OrtalamaPencere)
        {
            decimal ortalama = sonFiyatlar.Average();

            if (mevcutFiyat < ortalama * 0.99m)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"🟢 {hisseAdi}: Alım Sinyali! Fiyat: {mevcutFiyat} ₺ (Ort: {ortalama})");
                Console.ResetColor();

                await Program.SendTelegramMessage("BOT_TOKEN", "CHAT_ID", $"🟢 {hisseAdi} - Al: {mevcutFiyat} ₺ (Ort: {ortalama})");
            }
            else if (mevcutFiyat > ortalama * 1.02m)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"🔴 {hisseAdi}: Satış Sinyali! Fiyat: {mevcutFiyat} ₺ (Ort: {ortalama})");
                Console.ResetColor();

                await Program.SendTelegramMessage("BOT_TOKEN", "CHAT_ID", $"🔴 {hisseAdi} - Sat: {mevcutFiyat} ₺ (Ort: {ortalama})");
            }
        }
    }
}
