using Google.Api.Gax;
using Google.Cloud.PubSub.V1;

namespace Messaging.GCP.PubSub.Producers;

public class TopicPublisher : BackgroundService
{
    private readonly ILogger<TopicPublisher> _logger;
    private readonly PublisherClient _publisher;

    public TopicPublisher(ILogger<TopicPublisher> logger, PublisherClient publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var payload = "{\r\n  \"event_type\": \"payment_accepted\",\r\n  \"version\": \"1.0\",\r\n  \"payment_id\": \"payment123\",\r\n  \"order_id\": \"order789\",\r\n  \"payment_details\": {\r\n    \"amount\": 100.00,\r\n    \"currency\": \"USD\",\r\n    \"payment_method\": \"credit_card\",\r\n    \"transaction_id\": \"1234567890\"\r\n  },\r\n  \"payer\": {\r\n    \"user_id\": \"payer123\",\r\n    \"name\": \"John Doe\",\r\n    \"email\": \"john@example.com\"\r\n  },\r\n  \"payee\": {\r\n    \"user_id\": \"payee456\",\r\n    \"name\": \"Jane Smith\",\r\n    \"email\": \"jane@example.com\"\r\n  },\r\n  \"timestamp\": \"2023-10-01T12:00:00Z\"\r\n}";
            await PublishMessagesAsync([payload]);
            await Task.Delay(10000, stoppingToken);
        }
    }

    private async Task PublishMessagesAsync(IEnumerable<string> payloads)
    {
        var publishTasks = payloads.Select(async payload =>
        {
            string messageId = await _publisher.PublishAsync(payload);
            _logger.LogInformation($"Published message with messageId: {messageId}");
        });
        await Task.WhenAll(publishTasks);
    }

    private async Task<int> SubscribeWithConcurrencyControlAsync(string projectId, string subscriptionId)
    {
        SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);

        SubscriberClient subscriber = await new SubscriberClientBuilder
        {
            SubscriptionName = subscriptionName,
            // Normally the number of clients depends on the number of processors.
            // Here we explicitly request 2 concurrent clients instead.
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
        // Run for 10 seconds.
        await Task.Delay(10_000);
        await subscriber.StopAsync(CancellationToken.None);
        // Lets make sure that the start task finished successfully after the call to stop.
        await startTask;
        return count;
    }

    // https://cloud.google.com/pubsub/docs/flow-control
    private async Task<int> PullMessagesWithFlowControlAsync(string projectId, string subscriptionId, bool acknowledge)
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
        // SubscriberClient runs your message handle function on multiple
        // threads to maximize throughput.
        Task startTask = subscriber.StartAsync((message, cancel) =>
        {
            string text = message.Data.ToStringUtf8();
            Console.WriteLine($"Message {message.MessageId}: {text}");
            Interlocked.Increment(ref messageCount);
            return Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
        });
        // Run for 5 seconds.
        await Task.Delay(5000);
        await subscriber.StopAsync(CancellationToken.None);
        // Lets make sure that the start task finished successfully after the call to stop.
        await startTask;
        return messageCount;
    }

    private async Task<int> PullProtoMessagesAsync(string projectId, string subscriptionId, bool acknowledge)
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
        // SubscriberClient runs your message handle function on multiple
        // threads to maximize throughput.
        Task startTask = subscriber.StartAsync((message, cancel) =>
        {
            string encoding = message.Attributes["googclient_schemaencoding"];
            // Utilities.State state = null;
            switch (encoding)
            {
                case "BINARY":
                    // state = Utilities.State.Parser.ParseFrom(message.Data.ToByteArray());
                    break;
                case "JSON":
                    // state = Utilities.State.Parser.ParseJson(message.Data.ToStringUtf8());
                    break;
                default:
                    Console.WriteLine($"Encoding not provided in message.");
                    break;
            }
            // Console.WriteLine($"Message {message.MessageId}: {state}");
            Interlocked.Increment(ref messageCount);
            return Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
        });
        // Run for 5 seconds.
        await Task.Delay(5000);
        await subscriber.StopAsync(CancellationToken.None);
        // Lets make sure that the start task finished successfully after the call to stop.
        await startTask;
        return messageCount;
    }
}
