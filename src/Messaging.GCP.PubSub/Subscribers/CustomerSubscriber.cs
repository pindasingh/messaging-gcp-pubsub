using Google.Cloud.PubSub.V1;

namespace Messaging.GCP.PubSub.Subscribers;

public class CustomerSubscriber : BackgroundService
{
    private readonly ILogger<CustomerSubscriber> _logger;

    public CustomerSubscriber(ILogger<CustomerSubscriber> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SubscriberClient subscriber = await SubscriberClient.CreateAsync(SubscriptionName.FromProjectSubscription(Constants.PROJECT_ID, "customer"));
        await subscriber.StartAsync((message, cancel) =>
        {
            string text = message.Data.ToStringUtf8();
            _logger.LogInformation($"Message {message.MessageId}: {text}");
            return Task.FromResult(SubscriberClient.Reply.Ack);
        });
    }
}
