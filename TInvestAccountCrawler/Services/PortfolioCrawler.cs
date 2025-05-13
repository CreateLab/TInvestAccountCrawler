using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TInvestAccountCrawler.Helpers;
using TInvestAccountCrawler.Models;

namespace TInvestAccountCrawler.Services;

public class PortfolioCrawler
{
    public static async Task ProcessAccountsAsync(InvestApiClient client, string outputDir)
    {
        var accounts = await client.Users.GetAccountsAsync(new GetAccountsRequest());
        foreach (var account in accounts.Accounts)
        {
            Console.WriteLine($"Счет: {account.Id} ({account.Name})");

            var portfolio = await client.Operations.GetPortfolioAsync(new PortfolioRequest
            {
                AccountId = account.Id,
                Currency = PortfolioRequest.Types.CurrencyRequest.Rub
            });

            Console.WriteLine(
                $"  Стоимость позиций: {portfolio.TotalAmountPortfolio.ToMoney():N2} RUB");

            try
            {
                var limits = await client.Operations.GetWithdrawLimitsAsync(
                    new WithdrawLimitsRequest { AccountId = account.Id });
                var cashTotal = limits.Money.Sum(DecimalExtensions.ToMoney);
                Console.WriteLine($"  Доступно наличных: {cashTotal:N2} RUB");
            }
            catch
            {
                Console.WriteLine("  Доступно наличных: неизвестно");
            }

            var records = await CreatePositionRecordsAsync(client, portfolio);
            WriteCsv(account, records, outputDir);
            Console.WriteLine(new string('-', 60));
        }
    }

    private static async Task<PositionRecord[]> CreatePositionRecordsAsync(InvestApiClient client,
        PortfolioResponse portfolio)
    {
        var tasks = portfolio.Positions.Select(async p =>
        {
            var quantity = p.Quantity.ToDecimal();
            var avgPrice = p.AveragePositionPrice.ToMoney();
            var currentPrice = p.CurrentPrice.ToMoney();

            var details = await client.Instruments.FindInstrumentAsync(new FindInstrumentRequest
            {
                Query = p.InstrumentUid
            });
            var instr = details.Instruments.FirstOrDefault();

            return new PositionRecord
            {
                InstrumentType = p.InstrumentType,
                Figi = p.Figi,
                Name = instr?.Name ?? p.InstrumentUid,
                Ticker = instr?.Ticker ?? string.Empty,
                Quantity = quantity,
                StartPrice = avgPrice,
                CurrentPrice = currentPrice,
                StartValue = quantity * avgPrice,
                CurrentValue = quantity * currentPrice,
                ProfitRub = quantity * (currentPrice - avgPrice),
                ProfitPercent = (double)((currentPrice - avgPrice) ==  0 ? 0 : (currentPrice - avgPrice) / avgPrice * 100),
            };
        });

        return await Task.WhenAll(tasks);
    }

    private static void WriteCsv(Account account, PositionRecord[] records, string outputDir)
    {
        var safeName = string.Concat(account.Name
            .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var fileName = $"{account.Id}_{safeName}_{DateTime.Now:dd.MM.yyyy}.csv";
        var filePath = Path.Combine(outputDir, fileName);

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        csv.WriteRecords(records.OrderBy(r => r.InstrumentType).ThenByDescending(r => r.CurrentValue));
        Console.WriteLine($"  → Файл сохранён: {filePath}");
    }
}