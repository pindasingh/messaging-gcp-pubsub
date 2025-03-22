using System.Reflection;
using System.Text.Json;
using Google.Cloud.PubSub.V1;

namespace Messaging.GCP.PubSub.Subscribers;

public class PaymentSubscriber : BackgroundService
{
    private readonly ILogger<PaymentSubscriber> _logger;
    private readonly SubscriberClient _subscriber;

    public PaymentSubscriber(ILogger<PaymentSubscriber> logger, SubscriberClient subscriber)
    {
        _logger = logger;
        _subscriber = subscriber;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _subscriber.StartAsync((message, cancel) =>
        {
            var payload = DeserializePayload(message);
            _logger.LogInformation($"Received message {message.MessageId}: {payload}");
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });
    }

    protected object DeserializePayload(PubsubMessage message)
    {
        var contentType = message.Attributes["ContentType"];
        var messageType = Type.GetType(message.Attributes["MessageType"], true);

        switch (contentType)
        {
            case "application/json":
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize(message.Data.ToStringUtf8(), messageType, options);
            case "application/x-protobuf":
                var parser = messageType.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (parser == null)
                    throw new InvalidOperationException($"Could not find Parser for type {messageType.FullName}");

                var parseFromMethod = parser.GetType().GetMethod("ParseFrom", [typeof(byte[])]);
                return parseFromMethod.Invoke(parser, [message.Data.ToArray()]);
            default:
                throw new ArgumentException($"Unsupported content type: {contentType}");
        }
    }
}