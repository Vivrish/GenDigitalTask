namespace OrderProcessor.Domain;

public class PaymentEvent(string orderId, decimal amount)
{
    public required string OrderId { get; set; } = orderId;
    public required decimal Amount { get; set; } = amount;
}