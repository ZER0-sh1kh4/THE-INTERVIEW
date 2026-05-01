using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionService.Data;
using SubscriptionService.DTOs;
using SubscriptionService.Models;
using SubscriptionService.Services;
using Xunit;

namespace SubscriptionService.Tests;

/// <summary>
/// Verifies local subscription behavior for simulated checkout, confirmation, and cancellation.
/// </summary>
public class SubscriptionServiceTests
{
    [Fact]
    public async Task SubscribeAsync_WhenStripeDisabled_ReturnsSimulatedCheckoutAndSavesPendingPayment()
    {
        await using var context = CreateContext();
        var publisher = new RecordingPublisher();
        var service = CreateService(context, CreateConfig(stripeEnabled: false), publisher);

        var response = await service.SubscribeAsync(8);

        GetProperty<string>(response, "mode").Should().Be("Simulated");
        GetProperty<string>(response, "checkoutSessionId").Should().StartWith("cs_test_");

        var payment = await context.PaymentRecords.SingleAsync();
        payment.UserId.Should().Be(8);
        payment.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_WhenStripeEnabled_ThrowsValidationError()
    {
        await using var context = CreateContext();
        var publisher = new RecordingPublisher();
        var service = CreateService(context, CreateConfig(stripeEnabled: true), publisher);

        var act = () => service.ConfirmPaymentAsync(4, new ConfirmPaymentRequest
        {
            PaymentSessionId = "cs_test_123",
            PaymentReferenceId = "pi_123",
            Signature = "sig_123"
        });

        var exception = await act.Should().ThrowAsync<ValidationAppException>();
        exception.Which.Message.Should().Contain("Manual confirm is disabled");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_WhenStripeDisabled_CreatesSubscriptionAndPublishesEvents()
    {
        await using var context = CreateContext();
        context.PaymentRecords.Add(new PaymentRecord
        {
            UserId = 21,
            Amount = 499m,
            Currency = "INR",
            StripeSessionId = "cs_test_local",
            Status = "Pending"
        });
        await context.SaveChangesAsync();

        var publisher = new RecordingPublisher();
        var service = CreateService(context, CreateConfig(stripeEnabled: false), publisher);

        var result = await service.ConfirmPaymentAsync(21, new ConfirmPaymentRequest
        {
            PaymentSessionId = "cs_test_local",
            PaymentReferenceId = "pi_local",
            Signature = "sig_local"
        });

        GetProperty<string>(result, "status").Should().Be("Success");

        var payment = await context.PaymentRecords.SingleAsync();
        payment.Status.Should().Be("Success");
        payment.StripePaymentIntentId.Should().Be("pi_local");

        var subscription = await context.Subscriptions.SingleAsync();
        subscription.UserId.Should().Be(21);
        subscription.Status.Should().Be("Active");
        subscription.SagaState.Should().Be("PendingIdentityUpdate");

        publisher.Messages.Should().HaveCount(2);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_UpdatesSagaStateForActiveSubscriptions()
    {
        await using var context = CreateContext();
        context.Subscriptions.Add(new Subscription
        {
            UserId = 31,
            Plan = "Premium",
            Price = 499m,
            Status = "Active",
            SagaState = "Completed",
            EndDate = DateTime.UtcNow.AddDays(30)
        });
        await context.SaveChangesAsync();

        var publisher = new RecordingPublisher();
        var service = CreateService(context, CreateConfig(stripeEnabled: false), publisher);

        var message = await service.CancelSubscriptionAsync(31);

        message.Should().Contain("cancellation event published");
        var subscription = await context.Subscriptions.SingleAsync();
        subscription.Status.Should().Be("Cancelled");
        subscription.SagaState.Should().Be("PendingIdentityUpdate");
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    private static IConfiguration CreateConfig(bool stripeEnabled)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Stripe:Enabled"] = stripeEnabled ? "true" : "false",
                ["Stripe:SecretKey"] = stripeEnabled ? "sk_test_123" : "",
                ["Stripe:PublishableKey"] = stripeEnabled ? "pk_test_123" : "",
                ["Stripe:WebhookSecret"] = stripeEnabled ? "whsec_123" : "",
                ["Stripe:Amount"] = "49900",
                ["Stripe:Currency"] = "INR",
                ["Stripe:SuccessUrl"] = "http://localhost/success",
                ["Stripe:CancelUrl"] = "http://localhost/cancel"
            })
            .Build();
    }

    private static SubscriptionSvc CreateService(AppDbContext context, IConfiguration config, IRabbitMqPublisher publisher)
    {
        return new SubscriptionSvc(context, config, publisher, NullLogger<SubscriptionSvc>.Instance);
    }

    private static T GetProperty<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName);
        property.Should().NotBeNull($"expected property {propertyName} to exist on result");
        return (T)property!.GetValue(instance)!;
    }

    /// <summary>
    /// Records published messages so tests can assert saga/event behavior without a real broker.
    /// </summary>
    private sealed class RecordingPublisher : IRabbitMqPublisher
    {
        public List<(string RoutingKey, object Message)> Messages { get; } = new();

        public Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
        {
            Messages.Add((routingKey, message!));
            return Task.CompletedTask;
        }
    }
}
