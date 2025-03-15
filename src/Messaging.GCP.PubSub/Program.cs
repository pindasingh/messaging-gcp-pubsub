using Messaging.GCP.PubSub.Producers;
using Messaging.GCP.PubSub.Subscribers;

namespace Messaging.GCP.PubSub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            
            builder.Configuration.AddUserSecrets("6f8e18e6-8ca0-46d6-bf17-47e0e7374337");

            builder.Services.AddHostedService<CustomerSubscriber>((sp) =>
            {
                return new CustomerSubscriber(sp.GetRequiredService<ILogger<CustomerSubscriber>>(), builder.Configuration["Messaging:ProjectId"]);
            });

            builder.Services.AddHostedService<PaymentSubscriber>((sp) =>
            {
                return new PaymentSubscriber(sp.GetRequiredService<ILogger<PaymentSubscriber>>(), builder.Configuration["Messaging:ProjectId"]);
            });

            builder.Services.AddHostedService<Publisher>((sp) =>
            {
                return new Publisher(sp.GetRequiredService<ILogger<Publisher>>(), builder.Configuration["Messaging:ProjectId"]);
            });

            var host = builder.Build();
            host.Run();
        }
    }
}