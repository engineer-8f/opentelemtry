using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Osint.Source.Two;

public class MessageReceiver : IDisposable
{
    private readonly ILogger<MessageReceiver> _logger;
    private readonly ActivitySource _activitySource;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public MessageReceiver(ILogger<MessageReceiver> logger, ActivitySource activitySource)
    {
        _logger = logger;
        _activitySource = activitySource;
        _connection = RabbitMqExtensions.CreateConnection();
        _channel = RabbitMqExtensions.CreateModelAndDeclareTestQueue(_connection);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }

    public void StartConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (bc, ea) => ReceiveMessage(ea);
        _channel.BasicConsume(queue: RabbitMqExtensions.TestQueueName, autoAck: true, consumer: consumer);
    }

    private void ReceiveMessage(BasicDeliverEventArgs ea)
    {
        var parentContext = Propagator.Extract(default, ea.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        var activityName = $"{ea.RoutingKey} receive";

        using var activity = _activitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.Span.ToArray());

            _logger.LogInformation($"Message received: [{message}]");

            activity?.SetTag("message", message);

            // The OpenTelemetry messaging specification defines a number of attributes. These attributes are added here.
            RabbitMqExtensions.AddMessagingTags(activity);

            // Simulate some work
            Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            _logger.LogError(ex, "Message processing failed.");
        }
    }

    private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
    {
        try
        {
            _logger.LogInformation($"key '{key}'");
            if (props.Headers.TryGetValue(key, out var value))
            {
                var bytes = value as byte[];
                return new[] { Encoding.UTF8.GetString(bytes) };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract trace context.");
        }

        return Enumerable.Empty<string>();
    }
}