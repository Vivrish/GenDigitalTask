using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderProcessor.Converter;
using OrderProcessor.Data;
using OrderProcessor.Domain;

namespace OrderProcessor.Service;

public class OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
{
    private readonly ApplicationDbContext _context = context;
    private ILogger<OrderService> _logger = logger;

    public async Task ProcessOrderEvent(OrderEvent orderEvent)
    {
        _logger.LogInformation("Processing event: {orderEvent}", orderEvent);
        var order = await _context.Orders
            .Where(o => o.OrderCode == orderEvent.id)
            .FirstOrDefaultAsync();
        if (order == null)
        {
            _logger.LogInformation("Adding new order: {orderEvent}", orderEvent);
            await _context.Orders.AddAsync(OrderConverter.OrderEventToNewOrder(orderEvent));
            await _context.SaveChangesAsync();
            return;
        }
        await _context.Entry(order).ReloadAsync();
        if (order.Status != OrderStatus.WaitingForProduct) {
            Console.WriteLine($"Error! Order with code {orderEvent.id} already exists.");
            return;
        }

        order.Currency = orderEvent.currency;
        order.ProductCode = orderEvent.product;
        order.TotalPrice = orderEvent.total;
        _logger.LogInformation("Adding missing information to order: {order}", order);

        if (order.PaidAmount >= order.TotalPrice)
        {
            order.Status = OrderStatus.Finished;
            Console.WriteLine(order);
            _logger.LogInformation("Order is finished {order}", order);
            await _context.SaveChangesAsync();
            return;
        }
        
        _logger.LogInformation("Order is waiting for payment: {order}", order);
        order.Status = OrderStatus.WaitingForPayment;
        await _context.SaveChangesAsync();
    }

    public async Task ProcessPaymentEvent(PaymentEvent paymentEvent)
    {
        _logger.LogInformation("Processing payment event: {paymentEvent}", paymentEvent);
        var order = await _context.Orders
            .Where(o => o.OrderCode == paymentEvent.orderId)
            .FirstOrDefaultAsync();
        if (order == null)
        {
            _logger.LogInformation("Adding new order: {paymentEvent}", paymentEvent);
            await _context.Orders.AddAsync(OrderConverter.PaymentEventToOrder(paymentEvent));
            await _context.SaveChangesAsync();
            return;
        }
        await _context.Entry(order).ReloadAsync();

        order.PaidAmount += paymentEvent.amount;
        _logger.LogInformation("Adding payment to order: {order}", order);
        if (order.PaidAmount >= order.TotalPrice)
        {
            order.Status = OrderStatus.Finished;
            Console.WriteLine(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order is finished {order}", order);
            return;
        }
        _logger.LogInformation("Order is waiting for payment: {order}", order);
        order.Status = OrderStatus.WaitingForPayment;
        await _context.SaveChangesAsync();
    }
    
}