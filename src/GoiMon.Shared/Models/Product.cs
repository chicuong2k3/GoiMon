namespace GoiMon.Shared.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PriceCents { get; set; }
    public string Category { get; set; } = string.Empty;
}
