using Google.Cloud.PubSub.V1;
using Messaging.GCP.PubSub.Producers;
using Messaging.GCP.PubSub.Subscribers;

namespace Messaging.GCP.PubSub;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Configuration.AddUserSecrets("6f8e18e6-8ca0-46d6-bf17-47e0e7374337");

        builder.Services.AddHostedService<CustomerSubscriber>((provider) =>
        {
            var subscriberClientBuilder = new SubscriberClientBuilder()
            {
                SubscriptionName = SubscriptionName.FromProjectSubscription(builder.Configuration["Messaging:ProjectId"], "customer")
            };

            return new CustomerSubscriber(
                provider.GetRequiredService<ILogger<CustomerSubscriber>>(),
                subscriberClientBuilder.Build());
        });

        builder.Services.AddHostedService<PaymentSubscriber>((provider) =>
        {
            var subscriberClientBuilder = new SubscriberClientBuilder()
            {
                SubscriptionName = SubscriptionName.FromProjectSubscription(builder.Configuration["Messaging:ProjectId"], "payment")
            };

            return new PaymentSubscriber(
                provider.GetRequiredService<ILogger<PaymentSubscriber>>(),
                subscriberClientBuilder.Build());
        });

        builder.Services.AddHostedService<TopicPublisher>((provider) =>
        {
            var publisherClientBuilder = new PublisherClientBuilder()
            {
                TopicName = TopicName.FromProjectTopic(builder.Configuration["Messaging:ProjectId"], "apex")
            };

            return new TopicPublisher(
                provider.GetRequiredService<ILogger<TopicPublisher>>(),
                publisherClientBuilder.Build());
        });

        var host = builder.Build();
        host.Run();
    }
}
