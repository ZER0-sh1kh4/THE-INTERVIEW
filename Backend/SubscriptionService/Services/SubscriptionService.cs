using System.Text.Json;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using SubscriptionService.Data;
using SubscriptionService.DTOs;
using SubscriptionService.Models;
using AppSubscription = SubscriptionService.Models.Subscription;

namespace SubscriptionService.Services
{
    /// <summary>
    /// Handles Stripe checkout session creation, webhook processing and premium subscription saga events.
    /// </summary>
    public class SubscriptionSvc : ISubscriptionSvc
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly ILogger<SubscriptionSvc> _logger;

        public SubscriptionSvc(
            AppDbContext context,
            IConfiguration config,
            IRabbitMqPublisher rabbitMqPublisher,
            ILogger<SubscriptionSvc> logger)
        {
            _context = context;
            _config = config;
            _rabbitMqPublisher = rabbitMqPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Creates a Stripe Checkout session when Stripe mode is enabled, otherwise returns a local simulated order.
        /// </summary>
        public async Task<object> SubscribeAsync(int userId)
        {
            _logger.LogInformation("Creating subscription checkout session for user {UserId}", userId);

            var amountInMinorUnit = Convert.ToInt64(_config["Stripe:Amount"] ?? "49900");
            var currency = (_config["Stripe:Currency"] ?? "INR").ToLowerInvariant();

            if (!IsStripeEnabled())
            {
                var simulatedSessionId = $"cs_test_{Guid.NewGuid():N}";
                _context.PaymentRecords.Add(new PaymentRecord
                {
                    UserId = userId,
                    Amount = amountInMinorUnit / 100m,
                    Currency = currency.ToUpperInvariant(),
                    StripeSessionId = simulatedSessionId,
                    Status = "Pending"
                });

                await _context.SaveChangesAsync();

                return new
                {
                    checkoutSessionId = simulatedSessionId,
                    publishableKey = "pk_test_simulated",
                    amount = amountInMinorUnit,
                    currency = currency.ToUpperInvariant(),
                    mode = "Simulated",
                    message = "Stripe is disabled in configuration. Use the confirm endpoint for local demo mode."
                };
            }

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

            var service = new SessionService();
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"] ?? "http://localhost:3000/payment-success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _config["Stripe:CancelUrl"] ?? "http://localhost:3000/payment-cancelled",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = amountInMinorUnit,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Premium Subscription",
                                Description = "30-day premium access for the mock interview platform"
                            }
                        }
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = userId.ToString(),
                    ["source"] = "MockInterviewPlatform"
                }
            };

            var session = await service.CreateAsync(options);

            _context.PaymentRecords.Add(new PaymentRecord
            {
                UserId = userId,
                Amount = amountInMinorUnit / 100m,
                Currency = currency.ToUpperInvariant(),
                StripeSessionId = session.Id,
                Status = "Pending"
            });

            await _context.SaveChangesAsync();

            await _rabbitMqPublisher.PublishAsync(QueueNames.SubscriptionLifecycle, new SubscriptionLifecycleEvent
            {
                UserId = userId,
                Action = "SubscribeCreated",
                Status = "Pending",
                Message = $"Stripe checkout session {session.Id} created."
            });

            return new
            {
                checkoutSessionId = session.Id,
                checkoutUrl = session.Url,
                publishableKey = _config["Stripe:PublishableKey"] ?? string.Empty,
                amount = amountInMinorUnit,
                currency = currency.ToUpperInvariant(),
                mode = "Stripe",
                message = "Redirect the user to the Stripe checkout URL. Premium activates after Stripe calls the webhook."
            };
        }

        /// <summary>
        /// Confirms payment manually only when Stripe mode is disabled for local demo testing.
        /// </summary>
        public async Task<object> ConfirmPaymentAsync(int userId, ConfirmPaymentRequest request)
        {
            if (IsStripeEnabled())
            {
                throw new ValidationAppException("Manual confirm is disabled when Stripe mode is enabled. Wait for Stripe webhook callback.");
            }

            var paymentRecord = await _context.PaymentRecords.FirstOrDefaultAsync(p => p.StripeSessionId == request.PaymentSessionId && p.UserId == userId);
            if (paymentRecord == null) throw new NotFoundAppException("Payment record not found.");
            if (paymentRecord.Status == "Success") throw new ValidationAppException("This payment is already confirmed.");

            paymentRecord.Status = "Success";
            paymentRecord.StripePaymentIntentId = string.IsNullOrWhiteSpace(request.PaymentReferenceId) ? $"pi_{Guid.NewGuid():N}" : request.PaymentReferenceId;
            paymentRecord.StripeSignature = string.IsNullOrWhiteSpace(request.Signature) ? "simulated-signature" : request.Signature;

            var subscription = await EnsureActiveSubscriptionAsync(paymentRecord.UserId, paymentRecord, DateTime.UtcNow);
            await _context.SaveChangesAsync();
            await PublishSuccessfulPaymentEventsAsync(paymentRecord, subscription, null, CancellationToken.None);

            return new
            {
                transactionId = paymentRecord.StripePaymentIntentId,
                amount = paymentRecord.Amount,
                status = "Success",
                startDate = subscription.StartDate,
                endDate = subscription.EndDate,
                message = "Premium activation event published through RabbitMQ. After the saga completes, call POST /api/auth/refresh-claims to update JWT claims."
            };
        }

        /// <summary>
        /// Validates Stripe webhook signature and updates payment state from the Checkout session event.
        /// </summary>
        public async Task<object> HandleStripeWebhookAsync(string rawBody, string signature)
        {
            if (!IsStripeEnabled())
            {
                throw new ValidationAppException("Stripe mode is not enabled in configuration.");
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                throw new ForbiddenAppException("Missing Stripe-Signature header.");
            }

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var webhookSecret = _config["Stripe:WebhookSecret"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                throw new ValidationAppException("Stripe webhook secret is missing.");
            }

            Event stripeEvent;
            try
            {
                // Stripe CLI can forward events using a newer API version than the Stripe.NET package expects.
                // Keep signature validation enabled, but allow version mismatch so backend test-mode webhooks still work.
                stripeEvent = EventUtility.ConstructEvent(
                    rawBody,
                    signature,
                    webhookSecret,
                    throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook validation failed.");
                throw new ForbiddenAppException("Invalid Stripe webhook signature or webhook secret.");
            }

            var existingEvent = await _context.WebhookEventLogs.FirstOrDefaultAsync(x => x.EventId == stripeEvent.Id);
            if (existingEvent != null)
            {
                return new { processed = false, message = "Duplicate webhook event ignored." };
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null)
                {
                    throw new ValidationAppException("Stripe webhook did not contain a valid checkout session.");
                }

                var paymentRecord = await _context.PaymentRecords.FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);
                if (paymentRecord == null)
                {
                    await SaveWebhookLogAsync(stripeEvent.Id, stripeEvent.Type, session.Id, session.PaymentIntentId ?? string.Empty, "Ignored");
                    return new { processed = false, message = "No local payment record found for this Stripe session." };
                }

                if (paymentRecord.Status == "Success")
                {
                    await SaveWebhookLogAsync(stripeEvent.Id, stripeEvent.Type, session.Id, session.PaymentIntentId ?? string.Empty, "Ignored");
                    return new { processed = false, message = "Payment already processed." };
                }

                if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    await SaveWebhookLogAsync(stripeEvent.Id, stripeEvent.Type, session.Id, session.PaymentIntentId ?? string.Empty, "Ignored");
                    return new { processed = false, message = "Stripe checkout session is not paid yet." };
                }

                paymentRecord.Status = "Success";
                paymentRecord.StripePaymentIntentId = session.PaymentIntentId ?? string.Empty;
                paymentRecord.StripeSignature = signature;

                var subscription = await EnsureActiveSubscriptionAsync(paymentRecord.UserId, paymentRecord, DateTime.UtcNow);
                await SaveWebhookLogAsync(stripeEvent.Id, stripeEvent.Type, session.Id, session.PaymentIntentId ?? string.Empty, "Processed");
                await _context.SaveChangesAsync();

                await PublishSuccessfulPaymentEventsAsync(paymentRecord, subscription, session.CustomerDetails?.Email, CancellationToken.None);

                return new
                {
                    processed = true,
                    message = "Stripe webhook processed successfully.",
                    sessionId = session.Id,
                    paymentIntentId = session.PaymentIntentId,
                    subscriptionId = subscription.Id
                };
            }

            if (stripeEvent.Type == "checkout.session.expired")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null)
                {
                    var paymentRecord = await _context.PaymentRecords.FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);
                    if (paymentRecord != null)
                    {
                        paymentRecord.Status = "Failed";
                    }

                    await SaveWebhookLogAsync(stripeEvent.Id, stripeEvent.Type, session.Id, session.PaymentIntentId ?? string.Empty, "Processed");
                    await _context.SaveChangesAsync();
                }

                return new { processed = true, message = "Expired Stripe session marked as failed." };
            }

            await SaveWebhookLogAsync(stripeEvent.Id, stripeEvent.Type, string.Empty, string.Empty, "Ignored");
            return new { processed = false, message = $"Stripe webhook event '{stripeEvent.Type}' ignored." };
        }

        /// <summary>
        /// Cancels active subscriptions and starts the premium removal saga.
        /// </summary>
        public async Task<string> CancelSubscriptionAsync(int userId)
        {
            var subscriptions = await _context.Subscriptions.Where(s => s.UserId == userId && s.Status == "Active").ToListAsync();
            if (!subscriptions.Any())
            {
                throw new NotFoundAppException("No active subscription found.");
            }

            foreach (var sub in subscriptions)
            {
                sub.Status = "Cancelled";
                sub.SagaState = "PendingIdentityUpdate";
            }

            await _context.SaveChangesAsync();
            await _rabbitMqPublisher.PublishAsync(QueueNames.SubscriptionLifecycle, new SubscriptionLifecycleEvent
            {
                SagaId = Guid.NewGuid(),
                UserId = userId,
                Action = "Cancel",
                Status = "Pending",
                Message = "Premium cancellation saga started."
            });

            return "Subscription cancellation event published through RabbitMQ. After the saga completes, call POST /api/auth/refresh-claims to update your JWT.";
        }

        public async Task<IEnumerable<AppSubscription>> GetMySubscriptionsAsync(int userId) => await _context.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
        public async Task<IEnumerable<PaymentRecord>> GetMyPaymentsAsync(int userId) => await _context.PaymentRecords.Where(p => p.UserId == userId).ToListAsync();
        public async Task<IEnumerable<AppSubscription>> GetAllSubscriptionsAsync() => await _context.Subscriptions.ToListAsync();
        public async Task<IEnumerable<PaymentRecord>> GetAllPaymentsAsync() => await _context.PaymentRecords.ToListAsync();

        /// <summary>
        /// Returns true when Stripe live checkout and webhook mode is enabled.
        /// </summary>
        private bool IsStripeEnabled()
        {
            return bool.TryParse(_config["Stripe:Enabled"], out var enabled)
                && enabled
                && !string.IsNullOrWhiteSpace(_config["Stripe:SecretKey"])
                && !string.IsNullOrWhiteSpace(_config["Stripe:PublishableKey"])
                && !string.IsNullOrWhiteSpace(_config["Stripe:WebhookSecret"]);
        }

        /// <summary>
        /// Creates or reuses the user's active premium subscription record for a successful payment.
        /// </summary>
        private async Task<AppSubscription> EnsureActiveSubscriptionAsync(int userId, PaymentRecord paymentRecord, DateTime currentUtc)
        {
            AppSubscription? subscription = null;

            if (paymentRecord.SubscriptionId > 0)
            {
                subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == paymentRecord.SubscriptionId);
            }

            subscription ??= new AppSubscription
            {
                UserId = userId,
                Plan = "Premium",
                Price = paymentRecord.Amount,
                Status = "Active",
                SagaState = "PendingIdentityUpdate",
                StartDate = currentUtc,
                EndDate = currentUtc.AddDays(30)
            };

            if (subscription.Id == 0)
            {
                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();
            }
            else
            {
                subscription.Status = "Active";
                subscription.Price = paymentRecord.Amount;
                subscription.SagaState = "PendingIdentityUpdate";
                subscription.StartDate = currentUtc;
                subscription.EndDate = currentUtc.AddDays(30);
            }

            paymentRecord.SubscriptionId = subscription.Id;
            return subscription;
        }

        /// <summary>
        /// Publishes saga, payment and notification events after a successful payment.
        /// </summary>
        private async Task PublishSuccessfulPaymentEventsAsync(PaymentRecord paymentRecord, AppSubscription subscription, string? payerEmail, CancellationToken cancellationToken)
        {
            await _rabbitMqPublisher.PublishAsync(QueueNames.SubscriptionLifecycle, new SubscriptionLifecycleEvent
            {
                SagaId = Guid.NewGuid(),
                UserId = paymentRecord.UserId,
                Action = "Activate",
                Status = "Pending",
                Message = "Premium activation saga started."
            }, cancellationToken);

            await _rabbitMqPublisher.PublishAsync(QueueNames.PaymentEvents, new PaymentSucceededEvent
            {
                UserId = paymentRecord.UserId,
                Amount = paymentRecord.Amount,
                Currency = paymentRecord.Currency,
                PaymentId = paymentRecord.StripePaymentIntentId ?? paymentRecord.StripeSessionId ?? string.Empty,
                OrderId = paymentRecord.StripeSessionId ?? string.Empty
            }, cancellationToken);

            if (!string.IsNullOrWhiteSpace(payerEmail))
            {
                await _rabbitMqPublisher.PublishAsync(QueueNames.EmailNotifications, new EmailRequestedEvent
                {
                    ToEmail = payerEmail,
                    ToName = payerEmail,
                    Subject = "Payment successful",
                    TemplateKey = "payment-success",
                    Model = new Dictionary<string, string>
                    {
                        ["Amount"] = paymentRecord.Amount.ToString("0.00"),
                        ["Currency"] = paymentRecord.Currency,
                        ["PaymentId"] = paymentRecord.StripePaymentIntentId ?? paymentRecord.StripeSessionId ?? string.Empty
                    }
                }, cancellationToken);

                await _rabbitMqPublisher.PublishAsync(QueueNames.EmailNotifications, new EmailRequestedEvent
                {
                    ToEmail = payerEmail,
                    ToName = payerEmail,
                    Subject = "Premium subscription activated",
                    TemplateKey = "subscription-upgrade",
                    Model = new Dictionary<string, string>
                    {
                        ["Plan"] = subscription.Plan,
                        ["EndDate"] = subscription.EndDate.ToString("yyyy-MM-dd")
                    }
                }, cancellationToken);
            }
        }

        /// <summary>
        /// Stores webhook event ids so duplicate Stripe events can be ignored safely.
        /// </summary>
        private async Task SaveWebhookLogAsync(string eventId, string eventType, string sessionId, string paymentIntentId, string status)
        {
            _context.WebhookEventLogs.Add(new WebhookEventLog
            {
                EventId = eventId,
                EventType = eventType,
                OrderId = sessionId,
                PaymentId = paymentIntentId,
                Status = status,
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
