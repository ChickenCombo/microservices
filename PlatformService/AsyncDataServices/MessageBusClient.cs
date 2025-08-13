using System.Text;
using System.Text.Json;
using PlatformService.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace PlatformService.AsyncDataServices;

public class MessageBusClient : IMessageBusClient
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private MessageBusClient(IConfiguration configuration, IConnection connection, IChannel channel)
    {
        _configuration = configuration;
        _connection = connection;
        _channel = channel;

        _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
    }

    public static async Task<MessageBusClient> CreateAsync(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQHost"]
                ?? throw new ArgumentNullException("RabbitMQHost config is missing"),
            Port = int.Parse(configuration["RabbitMQPort"] ?? "5672")
        };

        var connection = await factory.CreateConnectionAsync()
            ?? throw new NullReferenceException("Failed to create RabbitMQ connection.");

        var channel = await connection.CreateChannelAsync()
            ?? throw new NullReferenceException("Failed to create RabbitMQ channel.");


        await channel.ExchangeDeclareAsync(
            exchange: "trigger",
            type: ExchangeType.Fanout
        );

        Console.WriteLine("--> Successfully connected to RabbitMQ");

        return new MessageBusClient(configuration, connection, channel);
    }

    private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
        return Task.CompletedTask;
    }

    public async Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
    {
        var message = JsonSerializer.Serialize(platformPublishedDto);

        if (_connection.IsOpen)
        {
            Console.WriteLine("--> RabbitMQ Connection is open, sending message...");
            await SendMessage(message);
        }
        else
        {
            Console.WriteLine("--> RabbitMQ Connection is closed, failed to send message...");

        }
    }

    private async Task SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        var props = new BasicProperties();

        await _channel.BasicPublishAsync(
            exchange: "trigger",
            routingKey: "",
            false,
            props,
            body: body
        );

        Console.WriteLine($"--> Sending message: {message}");
    }

    public void Dispose()
    {
        Console.WriteLine("--> Message bus disposed");

        if (_channel.IsOpen)
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}