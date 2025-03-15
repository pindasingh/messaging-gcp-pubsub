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
        while (!stoppingToken.IsCancellationRequested)
        {
            await _subscriber.StartAsync((message, cancel) =>
            {
                string text = message.Data.ToStringUtf8();
                _logger.LogInformation($"Message {message.MessageId}: {text}");
                return Task.FromResult(SubscriberClient.Reply.Ack);
            });
        }
    }
}
