using Tinkoff.InvestApi.V1;

namespace TInvestAccountCrawler.Helpers;

public static class DecimalExtensions
{
    public static decimal ToDecimal(this Quotation q) =>
        q.Units + q.Nano / 1_000_000_000m;

    public static decimal ToMoney(this MoneyValue m) =>
        m.Units + m.Nano / 1_000_000_000m;
}