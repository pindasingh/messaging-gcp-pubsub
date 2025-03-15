using Google.Cloud.PubSub.V1;

namespace Messaging.GCP.PubSub.Subscribers;

public class PaymentSubscriber : BackgroundService
{
    private readonly ILogger<PaymentSubscriber> _logger;
    private readonly string _projectId;

    public PaymentSubscriber(ILogger<PaymentSubscriber> logger, string projectId)
    {
        _logger = logger;
        _projectId = projectId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SubscriberClient subscriber = await SubscriberClient.CreateAsync(SubscriptionName.FromProjectSubscription(_projectId, "payment"));
        await subscriber.StartAsync((message, cancel) =>
        {
            string text = message.Data.ToStringUtf8();
            _logger.LogInformation($"Message {message.MessageId}: {text}");
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });
    }
}
