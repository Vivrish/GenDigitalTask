using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessor.Data;
using OrderProcessor.Domain;
using OrderProcessor.Service;
using Xunit;

namespace OrderProcessor.Test;

public class OrderServiceTest
{
    private readonly OrderService _orderService;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ILogger<OrderService>> _loggerMock;

        public OrderServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<OrderService>>();
            _orderService = new OrderService(_dbContext, _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessOrderEventShouldCreateNewOrder()
        {
            var orderEvent = new OrderEvent("O-123", "PR-ABC", 12.34m, "USD");
            
            await _orderService.ProcessOrderEvent(orderEvent);
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == "O-123");
            
            Assert.NotNull(order);
            Assert.Equal("PR-ABC", order.ProductCode);
            Assert.Equal(12.34m, order.TotalPrice);
            Assert.Equal("USD", order.Currency);
            Assert.Equal(OrderStatus.WaitingForPayment, order.Status);
        }

        [Fact]
        public async Task ProcessPaymentEventShouldCreatePlaceholderOrder()
        {
            var paymentEvent = new PaymentEvent("O-999", 5.00m);
            
            await _orderService.ProcessPaymentEvent(paymentEvent);
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == "O-999");
            
            Assert.NotNull(order);
            Assert.Equal(5.00m, order.PaidAmount);
            Assert.Equal(OrderStatus.WaitingForProduct, order.Status);
        }

        [Fact]
        public async Task ProcessPaymentEventShouldUpdateExistingOrder()
        {
            var order = new Order
            {
                OrderCode = "O-456",
                ProductCode = "PR-XYZ",
                TotalPrice = 20.00m,
                Currency = "USD",
                PaidAmount = 10.00m,
                Status = OrderStatus.WaitingForPayment
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            var paymentEvent = new PaymentEvent("O-456", 10.00m);
            
            await _orderService.ProcessPaymentEvent(paymentEvent);
            var updatedOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == "O-456");
            
            Assert.NotNull(updatedOrder);
            Assert.Equal(20.00m, updatedOrder.PaidAmount);
            Assert.Equal(OrderStatus.Finished, updatedOrder.Status);
        }
}