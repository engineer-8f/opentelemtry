using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using RabbitMQ.Client;

namespace Osint.Bot;

public class MessageSender : IDisposable
{
    private readonly ILogger<MessageSender> _logger;
    private readonly ActivitySource _activitySource;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public MessageSender(ILogger<MessageSender> logger, ActivitySource activitySource)
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

    public string Send()
    {
        const string activityName = $"{RabbitMqExtensions.TestQueueName} send";
        using var activity = _activitySource.StartActivity(activityName, ActivityKind.Producer);

        try
        {
            var props = _channel.CreateBasicProperties();
            
            Propagator.Inject(new PropagationContext(activity.GetContext(), Baggage.Current), props, InjectTraceContextIntoBasicProperties);
            
            RabbitMqExtensions.AddMessagingTags(activity);
            
            var body = $"Published message: DateTime.Now = {DateTime.Now}.";

            _channel.BasicPublish(
                exchange: RabbitMqExtensions.DefaultExchangeName,
                routingKey: RabbitMqExtensions.TestQueueName,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(body));

            _logger.LogInformation($"Message sent: [{body}]");

            return body;
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            _logger.LogError(ex, "Message publishing failed.");
            
            throw;
        }
    }

    private void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
    {
        try
        {
            _logger.LogDebug($"key '{key}' - value '{value}'");
            
            props.Headers ??= new Dictionary<string, object>();
            props.Headers[key] = value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject trace context.");
        }
    }
}