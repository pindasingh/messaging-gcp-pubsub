using Messaging.GCP.PubSub.Contracts.Payment.Authorized;

namespace Messaging.GCP.PubSub.Builders;

public class PaymentAuthorizedBuilder
{
    private readonly PaymentAuthorized _payment = new();

    public PaymentAuthorizedBuilder WithPaymentReference(string reference)
    {
        _payment.PaymentReference = reference;
        return this;
    }

    public PaymentAuthorizedBuilder WithCustomerReference(string reference)
    {
        _payment.CustomerReference = reference;
        return this;
    }

    public PaymentAuthorizedBuilder WithOrderReference(string reference)
    {
        _payment.OrderReference = reference;
        return this;
    }

    public PaymentAuthorizedBuilder WithGateway(string gateway)
    {
        _payment.Gateway = gateway;
        return this;
    }

    public PaymentAuthorizedBuilder WithAuthorizationCode(string code)
    {
        _payment.AuthorizationCode = code;
        return this;
    }

    public PaymentAuthorizedBuilder WithStatus(string status)
    {
        _payment.Status = status;
        return this;
    }

    public PaymentAuthorized Build() => _payment;
} 