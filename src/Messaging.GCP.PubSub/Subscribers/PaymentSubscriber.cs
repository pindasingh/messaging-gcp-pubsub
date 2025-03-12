using Google.Cloud.PubSub.V1;

namespace Messaging.GCP.PubSub.Subscribers;

public class PaymentSubscriber : BackgroundService
{
    private readonly ILogger<PaymentSubscriber> _logger;

    public PaymentSubscriber(ILogger<PaymentSubscriber> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SubscriberClient subscriber = await SubscriberClient.CreateAsync(SubscriptionName.FromProjectSubscription(Constants.PROJECT_ID, "payment"));
        await subscriber.StartAsync((message, cancel) =>
        {
            string text = message.Data.ToStringUtf8();
            _logger.LogInformation($"Message {message.MessageId}: {text}");
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });
    }
}
