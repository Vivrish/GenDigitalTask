namespace OrderProcessor.Domain;

public class OrderEvent(string id, string product, decimal total, string currency)
{
    public required string Id { get; set; } = id;
    public required string Product { get; set; } = product;
    public required decimal Total { get; set; } = total;
    public required string Currency { get; set; } = currency;
}
    