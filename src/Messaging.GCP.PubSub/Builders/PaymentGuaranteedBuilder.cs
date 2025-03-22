using Messaging.GCP.PubSub.Contracts.Payment.Guaranteed;

namespace Messaging.GCP.PubSub.Builders;

public class PaymentGuaranteedBuilder
{
    private readonly PaymentGuaranteed _payment = new();

    public PaymentGuaranteedBuilder WithPaymentReference(string reference)
    {
        _payment.PaymentReference = reference;
        return this;
    }

    public PaymentGuaranteedBuilder WithCustomerReference(string reference)
    {
        _payment.CustomerReference = reference;
        return this;
    }

    public PaymentGuaranteedBuilder WithOrderReference(string reference)
    {
        _payment.OrderReference = reference;
        return this;
    }

    public PaymentGuaranteedBuilder WithGateway(string gateway)
    {
        _payment.Gateway = gateway;
        return this;
    }

    public PaymentGuaranteedBuilder WithGuaranteeCode(string code)
    {
        _payment.GuaranteeCode = code;
        return this;
    }

    public PaymentGuaranteedBuilder WithStatus(string status)
    {
        _payment.Status = status;
        return this;
    }

    public PaymentGuaranteed Build() => _payment;
} 