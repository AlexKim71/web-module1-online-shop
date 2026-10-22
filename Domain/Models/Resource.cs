namespace Domain.Models;
public sealed class Resource
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long PriceMinor { get; set; }
    public int Capacity { get; set; }
    public int Available { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty; 
}