using Messaging.GCP.PubSub.Contracts.Payment.Initiated;

namespace Messaging.GCP.PubSub.Builders;

public class PaymentInitiatedBuilder
{
    private readonly PaymentInitiated _payment = new();
    private readonly List<LineItem> _lineItems = new();

    public PaymentInitiatedBuilder WithPaymentReference(string reference)
    {
        _payment.PaymentReference = reference;
        return this;
    }

    public PaymentInitiatedBuilder WithCustomerReference(string reference)
    {
        _payment.CustomerReference = reference;
        return this;
    }

    public PaymentInitiatedBuilder WithOrderReference(string reference)
    {
        _payment.OrderReference = reference;
        return this;
    }

    public PaymentInitiatedBuilder WithGateway(string gateway)
    {
        _payment.Gateway = gateway;
        return this;
    }

    public PaymentInitiatedBuilder WithMethod(string method)
    {
        _payment.Method = method;
        return this;
    }

    public PaymentInitiatedBuilder WithLineItem(string name, double unitPrice, int quantity)
    {
        var subtotal = unitPrice * quantity;
        _lineItems.Add(new LineItem
        {
            Name = name,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Subtotal = subtotal
        });
        return this;
    }

    public PaymentInitiatedBuilder WithSubtotal(double subtotal)
    {
        _payment.Subtotal = subtotal;
        return this;
    }

    public PaymentInitiatedBuilder WithTax(double tax)
    {
        _payment.Tax = tax;
        return this;
    }

    public PaymentInitiatedBuilder WithShipping(double shipping)
    {
        _payment.Shipping = shipping;
        return this;
    }

    public PaymentInitiatedBuilder WithDiscount(double discount)
    {
        _payment.Discount = discount;
        return this;
    }

    public PaymentInitiatedBuilder WithTotal(double total)
    {
        _payment.Total = total;
        return this;
    }

    public PaymentInitiatedBuilder WithCurrency(string currency)
    {
        _payment.Currency = currency;
        return this;
    }

    public PaymentInitiated Build()
    {
        _payment.LineItems.AddRange(_lineItems);
        return _payment;
    }
} 