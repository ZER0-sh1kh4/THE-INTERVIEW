using SubscriptionService.DTOs;
using SubscriptionService.Models;

namespace SubscriptionService.Services
{
    /// <summary>
    /// Defines subscription and payment operations.
    /// </summary>
    public interface ISubscriptionSvc
    {
        Task<object> SubscribeAsync(int userId);
        Task<object> ConfirmPaymentAsync(int userId, ConfirmPaymentRequest request);
        Task<object> HandleStripeWebhookAsync(string rawBody, string signature);
        Task<IEnumerable<Subscription>> GetMySubscriptionsAsync(int userId);
        Task<IEnumerable<PaymentRecord>> GetMyPaymentsAsync(int userId);
        Task<string> CancelSubscriptionAsync(int userId);
        Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync();
        Task<IEnumerable<PaymentRecord>> GetAllPaymentsAsync();
    }
}
