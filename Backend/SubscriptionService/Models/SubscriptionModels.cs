using System;

namespace SubscriptionService.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Plan { get; set; } = "Free"; // Free/Premium
        public decimal Price { get; set; }
        public string Status { get; set; } = "Active"; // Active/Cancelled
        public string SagaState { get; set; } = "NotStarted"; // NotStarted/PendingIdentityUpdate/Completed/CompensationRequired/CancelledCompleted
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PaymentRecord
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string StripeSessionId { get; set; } = string.Empty;
        public string StripePaymentIntentId { get; set; } = string.Empty;
        public string StripeSignature { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending/Success/Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WebhookEventLog
    {
        public int Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
