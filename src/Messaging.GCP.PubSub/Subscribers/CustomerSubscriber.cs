using System.Reflection;
using System.Text.Json;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;

namespace Messaging.GCP.PubSub.Subscribers;

public class CustomerSubscriber : BackgroundService
{
    private readonly ILogger<CustomerSubscriber> _logger;
    private readonly SubscriberClient _subscriber;

    public CustomerSubscriber(ILogger<CustomerSubscriber> logger, SubscriberClient subscriber)
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

    private async Task SubscribeWithConcurrencyControlAsync(string projectId, string subscriptionId)
    {
        SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);

        SubscriberClient subscriber = await new SubscriberClientBuilder
        {
            SubscriptionName = subscriptionName,
            ClientCount = 2
        }.BuildAsync();

        int count = 0;
        Task startTask = subscriber.StartAsync((message, cancellationToken) =>
        {
            string text = message.Data.ToStringUtf8();
            Console.WriteLine($"Received message: {text}");
            Interlocked.Increment(ref count);
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });
    }

    private async Task PullMessagesWithFlowControlAsync(string projectId, string subscriptionId, bool acknowledge)
    {
        SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
        int messageCount = 0;
        SubscriberClient subscriber = await new SubscriberClientBuilder
        {
            SubscriptionName = subscriptionName,
            Settings = new SubscriberClient.Settings
            {
                AckExtensionWindow = TimeSpan.FromSeconds(4),
                AckDeadline = TimeSpan.FromSeconds(10),
                FlowControlSettings = new FlowControlSettings(maxOutstandingElementCount: 100, maxOutstandingByteCount: 10240)
            }
        }.BuildAsync();
    }

    private async Task PullProtoMessagesAsync(string projectId, string subscriptionId, bool acknowledge)
    {
        SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
        int messageCount = 0;
        SubscriberClient subscriber = await new SubscriberClientBuilder
        {
            SubscriptionName = subscriptionName,
            Settings = new SubscriberClient.Settings
            {
                AckExtensionWindow = TimeSpan.FromSeconds(4),
                AckDeadline = TimeSpan.FromSeconds(10),
                FlowControlSettings = new FlowControlSettings(maxOutstandingElementCount: 100, maxOutstandingByteCount: 10240)
            }
        }.BuildAsync();
    }
}
