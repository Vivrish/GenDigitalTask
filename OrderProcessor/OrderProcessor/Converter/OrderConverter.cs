using OrderProcessor.Domain;

namespace OrderProcessor.Converter;

public static class OrderConverter
{
    public static Order PaymentEventToOrder(PaymentEvent paymentEvent)
    {
        return new Order
        {
            OrderCode = paymentEvent.orderId,
            PaidAmount = paymentEvent.amount,
            Status = OrderStatus.WaitingForProduct
        };
    }

    public static Order OrderEventToNewOrder(OrderEvent orderEvent)
    {
        return new Order
        {
            OrderCode = orderEvent.id,
            Currency = orderEvent.currency,
            ProductCode = orderEvent.product,
            TotalPrice = orderEvent.total,
            Status = OrderStatus.WaitingForPayment
        };
    }
}