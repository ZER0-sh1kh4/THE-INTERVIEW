using System.ComponentModel.DataAnnotations;

namespace SubscriptionService.DTOs
{
    public class ConfirmPaymentRequest
    {
        [Required]
        public string PaymentSessionId { get; set; } = string.Empty;
        [Required]
        public string PaymentReferenceId { get; set; } = string.Empty;
        [Required]
        public string Signature { get; set; } = string.Empty;
    }
}
