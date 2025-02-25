namespace OrderProcessor.Domain;

public class PaymentEvent(string orderId, decimal amount)
{
    public string orderId { get; set; } = orderId;
    public decimal amount { get; set; } = amount;

    public override string ToString()
    {
        return $"{nameof(orderId)}: {orderId}, {nameof(amount)}: {amount}";
    }
}