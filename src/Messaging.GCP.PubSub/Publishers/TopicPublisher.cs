using System.Text.Json;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Messaging.GCP.PubSub.Builders;

namespace Messaging.GCP.PubSub.Publishers;

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
            var paymentReference = Guid.NewGuid().ToString();
            var customerReference = Guid.NewGuid().ToString();
            var orderReference = Guid.NewGuid().ToString();

            await PublishPaymentInitiated(paymentReference, customerReference, orderReference);
            await PublishPaymentAuthorised(paymentReference, customerReference, orderReference);
            await PublishPaymentGuaranteed(paymentReference, customerReference, orderReference);
        }
    }

    private async Task PublishPaymentInitiated(string paymentReference, string customerReference, string orderReference)
    {
        var initiated = new PaymentInitiatedBuilder()
            .WithPaymentReference(paymentReference)
            .WithCustomerReference(customerReference)
            .WithOrderReference(orderReference)
            .WithGateway("Stripe")
            .WithMethod("Credit Card")
            .WithLineItem("Premium Wireless Headphones", 199.99, 1)
            .WithLineItem("Smart Watch", 299.99, 1)
            .WithSubtotal(499.98)
            .WithTax(40.00)
            .WithShipping(15.00)
            .WithDiscount(20.00)
            .WithTotal(534.98)
            .WithCurrency("USD")
            .Build();

        await PublishEvent(initiated, "application/json");
    }

    private async Task PublishPaymentAuthorised(string paymentReference, string customerReference, string orderReference)
    {
        var authorized = new PaymentAuthorizedBuilder()
            .WithPaymentReference(paymentReference)
            .WithCustomerReference(customerReference)
            .WithOrderReference(orderReference)
            .WithGateway("Stripe")
            .WithAuthorizationCode(Guid.NewGuid().ToString())
            .WithStatus("Authorized")
            .Build();

        await PublishEvent(authorized, "application/x-protobuf");
    }

    private async Task PublishPaymentGuaranteed(string paymentReference, string customerReference, string orderReference)
    {
        var guaranteed = new PaymentGuaranteedBuilder()
            .WithPaymentReference(paymentReference)
            .WithCustomerReference(customerReference)
            .WithOrderReference(orderReference)
            .WithGateway("Stripe")
            .WithGuaranteeCode(Guid.NewGuid().ToString())
            .WithStatus("Guaranteed")
            .Build();

        await PublishEvent(guaranteed, "application/x-protobuf");
    }

    private async Task PublishEvent<T>(T evt, string contentType) where T : class
    {
        var message = new PubsubMessage
        {
            Attributes =
            {
                { "ContentType", contentType },
                { "MessageType", typeof(T).AssemblyQualifiedName },
                { "OriginatingEndpoint", GetType().FullName },
                { "TimeSent", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() },
            },
            Data = SerializeEvent(evt, contentType)
        };

        await _publisher.PublishAsync(message);
        _logger.LogInformation("Published {MessageType} as {ContentType}", typeof(T).Name, contentType);
        Thread.Sleep(10000);
    }

    private ByteString SerializeEvent<T>(T evt, string contentType) => contentType switch
    {
        "application/json" => ByteString.CopyFromUtf8(JsonSerializer.Serialize(evt)),
        "application/x-protobuf" => ((IMessage)evt).ToByteString(),
        _ => throw new ArgumentException($"Unsupported content type: {contentType}")
    };
}
