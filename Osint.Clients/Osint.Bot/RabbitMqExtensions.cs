using System.Diagnostics;
using RabbitMQ.Client;

namespace Osint.Bot;

public static class RabbitMqExtensions
{
    public const string DefaultExchangeName = "";
    public const string TestQueueName = "TestQueue";

    private static readonly ConnectionFactory ConnectionFactory;

    static RabbitMqExtensions()
    {
        ConnectionFactory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "rabbitmq",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "app",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "app",
            Port = 5672,
            RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
        };
    }

    public static IConnection CreateConnection() => ConnectionFactory.CreateConnection();

    public static IModel CreateModelAndDeclareTestQueue(IConnection connection)
    {
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: TestQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        return channel;
    }

    public static void AddMessagingTags(Activity activity) =>
        activity?
            .SetTag("messaging.system", "rabbitmq")
            .SetTag("messaging.destination_kind", "queue")
            .SetTag("messaging.destination", DefaultExchangeName)
            .SetTag("messaging.rabbitmq.routing_key", TestQueueName);
    
    public static ActivityContext GetContext(this Activity activity)
    {
        ActivityContext context = default;
        if (activity != null)
        {
            context = activity.Context;
        }
        else if (Activity.Current != null)
        {
            context = Activity.Current.Context;
        }

        return context;
    }
}