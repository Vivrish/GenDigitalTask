using OrderProcessor.Domain;

namespace OrderProcessor.Converter;

public static class OrderConverter
{
    public static Order PaymentEventToOrder(PaymentEvent paymentEvent)
    {
        return new Order
        {
            PaidAmount = paymentEvent.Amount,
            Status = OrderStatus.WaitingForProduct
        };
    }

    public static Order OrderEventToNewOrder(OrderEvent orderEvent)
    {
        return new Order
        {
            OrderCode = orderEvent.Id,
            Currency = orderEvent.Currency,
            ProductCode = orderEvent.Product,
            TotalPrice = orderEvent.Total,
            Status = OrderStatus.WaitingForPayment
        };
    }
}