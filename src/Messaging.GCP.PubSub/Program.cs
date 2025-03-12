using Messaging.GCP.PubSub.Producers;
using Messaging.GCP.PubSub.Subscribers;

namespace Messaging.GCP.PubSub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<CustomerSubscriber>();
            builder.Services.AddHostedService<PaymentSubscriber>();
            builder.Services.AddHostedService<Publisher>();

            var host = builder.Build();
            host.Run();
        }
    }
}