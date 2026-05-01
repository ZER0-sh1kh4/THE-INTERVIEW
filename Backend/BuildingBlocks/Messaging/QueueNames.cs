namespace BuildingBlocks.Messaging;

/// <summary>
/// Central place for queue and exchange names.
/// </summary>
public static class QueueNames
{
    public const string Exchange = "mockinterview.events.v2";
    public const string DeadLetterExchange = "mockinterview.events.v2.dlx";
    public const string EmailNotifications = "notifications.email.v2";
    public const string UserRegistration = "identity.user.registration.v2";
    public const string SubscriptionLifecycle = "subscription.lifecycle.v2";
    public const string SubscriptionResults = "subscription.results.v2";
    public const string PaymentEvents = "payment.events.v2";
    public const string AssessmentEvents = "assessment.events.v2";
    public const string InterviewEvents = "interview.events.v2";

    /// <summary>
    /// Returns the dead-letter queue name for a primary queue.
    /// </summary>
    public static string DeadLetterQueue(string queueName) => $"{queueName}.dead";
}
