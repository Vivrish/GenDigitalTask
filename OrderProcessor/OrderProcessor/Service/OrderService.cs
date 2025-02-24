using Microsoft.EntityFrameworkCore;
using OrderProcessor.Converter;
using OrderProcessor.Data;
using OrderProcessor.Domain;

namespace OrderProcessor.Service;

public class OrderService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task ProcessOrderEvent(OrderEvent orderEvent)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderEvent.Id);
        if (order == null)
        {
            await _context.Orders.AddAsync(OrderConverter.OrderEventToNewOrder(orderEvent));
            return;
        }
        if (order.Status != OrderStatus.WaitingForProduct) {
            Console.WriteLine($"Error! Order with code {orderEvent.Id} already exists.");
            return;
        }

        order.Currency = orderEvent.Currency;
        order.ProductCode = orderEvent.Product;
        order.TotalPrice = orderEvent.Total;

        if (order.PaidAmount >= order.TotalPrice)
        {
            order.Status = OrderStatus.Finished;
            Console.WriteLine(order);
            await _context.SaveChangesAsync();
            return;
        }

        order.Status = OrderStatus.WaitingForPayment;
        await _context.SaveChangesAsync();
    }

    public async Task ProcessPaymentEvent(PaymentEvent paymentEvent)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == paymentEvent.OrderId);
        if (order == null)
        {
            await _context.Orders.AddAsync(OrderConverter.PaymentEventToOrder(paymentEvent));
            return;
        }

        order.PaidAmount += paymentEvent.Amount;
        if (order.PaidAmount >= order.TotalPrice)
        {
            order.Status = OrderStatus.Finished;
            Console.WriteLine(order);
            await _context.SaveChangesAsync();
            return;
        }

        order.Status = OrderStatus.WaitingForPayment;
        await _context.SaveChangesAsync();
    }
    
}