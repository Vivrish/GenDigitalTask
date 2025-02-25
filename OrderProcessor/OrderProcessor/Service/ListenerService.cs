using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderProcessor.Domain;
using OrderProcessor.Exception;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderProcessor.Service;

public class ListenerService(IServiceScopeFactory scopeFactory, IConfiguration configuration): BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private IChannel _channel;
    private IConnection _connection;
    private string _queueName;
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory {HostName = _configuration.GetValue<string>("RabbitMQ:Host") ?? throw new BadConfigurationException()};
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        _queueName = _configuration.GetValue<string>("RabbitMQ:QueueName") ?? throw new BadConfigurationException();
        await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += ConsumeMessageAsync;
        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer, cancellationToken: stoppingToken);
    }

    private async Task ConsumeMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var bytes = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(bytes);
        try
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                await ProcessMessageAsync(message, ea.BasicProperties.Headers, orderService);
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Warning: message cannot be consumed due to the error: {e.Message}");
            await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }

    private async Task ProcessMessageAsync(string message, IDictionary<string, object?>? headers, OrderService orderService)
    {
        if (headers == null || !headers.TryGetValue("X-MsgType", out var header))
        {
            throw new InvalidOperationException("Message is missing X-MsgType header");
        }
        var messageType = Encoding.UTF8.GetString((byte[])(header ?? throw new InvalidOperationException("Header misses a value")));
        switch (messageType)
        {
            case "OrderEvent":
            {
                var orderEvent = JsonSerializer.Deserialize<OrderEvent>(message);
                await orderService.ProcessOrderEvent(orderEvent ?? throw new InvalidOperationException("Failed to deserialize an event"));
                break;
            }
            case "PaymentEvent":
            {
                var paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(message);
                await orderService.ProcessPaymentEvent(paymentEvent ?? throw new InvalidOperationException("Failed to deserialize an event"));
                break;
            }
            default:
                throw new InvalidOperationException("Unknown event");
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _channel.CloseAsync(cancellationToken: cancellationToken);
        await _connection.CloseAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}