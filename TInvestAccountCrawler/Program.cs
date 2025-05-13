using System.Text;
using Tinkoff.InvestApi;
using TInvestAccountCrawler.Services;

Console.OutputEncoding = Encoding.Unicode;
if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Ошибка: не передан токен API. Использование:");
    Console.Error.WriteLine("  dotnet run <API_TOKEN> [<путь_к_папке>]");
    return 1;
}

var token = args[0];
var outputDir = args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1])
    ? args[1]
    : Directory.GetCurrentDirectory();

Console.OutputEncoding = Encoding.Unicode;
var client = InvestApiClientFactory.Create(token);

await PortfolioCrawler.ProcessAccountsAsync(client, outputDir);
return 0;


