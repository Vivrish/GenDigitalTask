namespace OrderProcessor.Domain;

public class OrderEvent(string id, string productId, decimal total, string currency)
{
    public required string Id { get; set; } = id;
    public required string ProductId { get; set; } = productId;
    public required decimal Total { get; set; } = total;
    public required string Currency { get; set; } = currency;
}
    