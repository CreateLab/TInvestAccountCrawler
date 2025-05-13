namespace TInvestAccountCrawler.Models;

public record PositionRecord
{
    public string InstrumentType { get; set; }
    public string Figi { get; set; }
    public string Name { get; set; }
    public string Ticker { get; set; }
    public decimal Quantity { get; set; }
    public decimal StartPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal StartValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal ProfitRub { get; set; }
    public double ProfitPercent { get; set; }
}