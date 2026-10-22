namespace Domain.Models;
public enum RequestStatus { Active, Cancelled }
public sealed class Request
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string CustomerName { get; set; } = string.Empty; 
    public int Quantity { get; set; }                        
    public long UnitPriceMinor { get; set; }                 
    public DateTime CreatedAtUtc { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Active;
    public string ResourceName { get; set; } = string.Empty; 
}