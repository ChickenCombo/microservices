using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AsyncDataServices.AsyncDataService;

public class MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor) : BackgroundService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IEventProcessor _eventProcessor = eventProcessor;
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _queueName;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeRabbitMQ();
        await base.StartAsync(cancellationToken);
    }

    private async Task InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQHost"]
               ?? throw new ArgumentNullException("RabbitMQHost config is missing"),
            Port = int.Parse(_configuration["RabbitMQPort"] ?? "5672")
        };

        try
        {
            _connection = await factory.CreateConnectionAsync()
                ?? throw new NullReferenceException("Failed to create RabbitMQ connection.");

            _channel = await _connection.CreateChannelAsync()
                ?? throw new NullReferenceException("Failed to create RabbitMQ channel.");

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;

            await _channel.ExchangeDeclareAsync(
                exchange: "trigger",
                type: ExchangeType.Fanout
            );

            var queueDeclareResult = await _channel.QueueDeclareAsync(
                    queue: "",
                    durable: false,
                    exclusive: false,
                    autoDelete: false
                );

            _queueName = queueDeclareResult.QueueName;

            await _channel.QueueBindAsync(
                queue: _queueName,
                exchange: "trigger",
                routingKey: ""
            );

            Console.WriteLine("--> Listening on the Message Bus...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            Console.WriteLine("--> Event received!");

            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            _eventProcessor.ProcessEvent(notificationMessage);
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
    }

    private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        Console.WriteLine("--> Message bus disposed");

        if (_channel?.IsOpen == true)
        {
            _channel.Dispose();
        }

        if (_connection?.IsOpen == true)
        {
            _connection.Dispose();
        }

        base.Dispose();
    }
}