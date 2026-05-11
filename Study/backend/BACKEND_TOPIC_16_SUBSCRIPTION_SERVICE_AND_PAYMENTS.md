# Topic 16: SubscriptionService and Payments

Project: Mock Interview Platform  
Focus: Understanding premium subscription flow, Stripe Checkout Session, simulated payment mode, payment records, Stripe webhooks, webhook signature validation, idempotency, subscription activation, RabbitMQ saga events, cancellation, and JWT refresh after payment.

---

## 1. What Is SubscriptionService?

### Simple Explanation

SubscriptionService manages premium access and payments.

It answers questions like:

```text
Did user start payment?
Did payment succeed?
Is subscription active?
When does premium end?
Was Stripe webhook already processed?
Should premium be activated or cancelled?
```

### In Your Project

SubscriptionService handles:

- Creating checkout session
- Simulated local payment mode
- Manual payment confirmation in demo mode
- Stripe webhook processing
- Payment records
- Subscription records
- Premium activation event
- Premium cancellation event
- Subscription/payment history
- Admin payment/subscription views

### Viva Answer

> SubscriptionService manages premium subscription and payment lifecycle. It creates payment sessions, records payments, processes Stripe webhooks, activates premium through events, handles cancellation, and stores subscription/payment history.

---

## 2. Why SubscriptionService Is Separate

### Simple Explanation

Payment and subscription logic is different from login, interviews, assessments, and emails.

### Service Ownership

```text
IdentityService -> user identity and premium claim
SubscriptionService -> payments, subscriptions, webhook logs
NotificationService -> email sending
InterviewService -> premium interview access
AssessmentService -> premium assessment access
```

### Why This Is Good

- Payment logic stays isolated
- Stripe secrets stay in one service
- Payment records have clear ownership
- Premium activation can be event-based
- Other services do not handle payment gateway details

### Viva Answer

> SubscriptionService is separate because payment processing, subscription records, webhook handling, and premium lifecycle are independent and security-sensitive responsibilities.

---

## 3. Important SubscriptionService Files

### Program and Configuration

```text
Backend/SubscriptionService/Program.cs
Backend/SubscriptionService/appsettings.Example.json
```

### Controller

```text
Backend/SubscriptionService/Controllers/SubscriptionController.cs
```

### Service

```text
Backend/SubscriptionService/Services/ISubscriptionSvc.cs
Backend/SubscriptionService/Services/SubscriptionService.cs
```

### Data and Models

```text
Backend/SubscriptionService/Data/AppDbContext.cs
Backend/SubscriptionService/Models/SubscriptionModels.cs
```

### DTOs and Messaging

```text
Backend/SubscriptionService/DTOs/SubscriptionDTOs.cs
Backend/SubscriptionService/Messaging/IdentityResultConsumer.cs
```

### Viva Answer

> Important files are SubscriptionController, ISubscriptionSvc, SubscriptionSvc, AppDbContext, SubscriptionModels, SubscriptionDTOs, and IdentityResultConsumer.

---

## 4. SubscriptionService Database Tables

### DbContext

File:

```text
Backend/SubscriptionService/Data/AppDbContext.cs
```

### DbSets

```csharp
public DbSet<Subscription> Subscriptions { get; set; }
public DbSet<PaymentRecord> PaymentRecords { get; set; }
public DbSet<WebhookEventLog> WebhookEventLogs { get; set; }
```

### Viva Answer

> SubscriptionService uses Subscriptions, PaymentRecords, and WebhookEventLogs tables.

---

## 5. Subscription Model

### Fields

```text
Id
UserId
Plan
Price
Status
SagaState
StartDate
EndDate
CreatedAt
```

### Plan Values

```text
Free
Premium
```

### Status Values

```text
Active
Cancelled
Failed
```

### Viva Answer

> Subscription model stores user's premium subscription plan, price, status, saga state, start date, end date, and creation time.

---

## 6. SagaState

### Simple Explanation

SagaState tracks multi-service premium activation/cancellation progress.

### Values in Your Project

```text
NotStarted
PendingIdentityUpdate
Completed
CompensationRequired
CancelledCompleted
```

### Why Needed

Premium update involves:

```text
SubscriptionService
IdentityService
RabbitMQ
```

So SubscriptionService needs to know whether IdentityService completed the premium update.

### Viva Answer

> SagaState tracks whether premium activation or cancellation has completed across SubscriptionService and IdentityService.

---

## 7. PaymentRecord Model

### Fields

```text
Id
SubscriptionId
UserId
Amount
Currency
StripeSessionId
StripePaymentIntentId
StripeSignature
Status
CreatedAt
```

### Status Values

```text
Pending
Success
Failed
```

### Purpose

PaymentRecord stores payment attempt and Stripe identifiers.

### Viva Answer

> PaymentRecord stores payment attempt details such as amount, currency, Stripe session id, payment intent id, signature, status, and user id.

---

## 8. WebhookEventLog Model

### Fields

```text
EventId
EventType
OrderId
PaymentId
Status
CreatedAt
ProcessedAt
```

### Purpose

Stores Stripe webhook event IDs so duplicate webhook events can be ignored.

### Database Constraint

```csharp
modelBuilder.Entity<WebhookEventLog>()
    .HasIndex(x => x.EventId)
    .IsUnique();
```

### Viva Answer

> WebhookEventLog stores processed Stripe event IDs and has a unique EventId index to help prevent duplicate webhook processing.

---

## 9. Decimal Precision for Money

### In AppDbContext

```csharp
modelBuilder.Entity<Subscription>()
    .Property(x => x.Price)
    .HasPrecision(18, 2);

modelBuilder.Entity<PaymentRecord>()
    .Property(x => x.Amount)
    .HasPrecision(18, 2);
```

### Why

Money values need fixed decimal precision.

### Viva Answer

> Decimal precision is configured for subscription price and payment amount to store money accurately.

---

## 10. SubscriptionController

### File

```text
Backend/SubscriptionService/Controllers/SubscriptionController.cs
```

### Base Route

```text
/api/subscriptions
```

### Protection

Controller has:

```csharp
[Authorize]
```

Webhook endpoint has:

```csharp
[AllowAnonymous]
```

### Viva Answer

> SubscriptionController exposes payment and subscription APIs. Normal endpoints require JWT, while Stripe webhook is anonymous but protected by Stripe signature validation.

---

## 11. SubscriptionService Endpoints

### User Endpoints

```text
POST /api/subscriptions/subscribe
POST /api/subscriptions/confirm
POST /api/subscriptions/cancel
GET /api/subscriptions/my
GET /api/subscriptions/my/payments
```

### Webhook Endpoint

```text
POST /api/subscriptions/webhook/stripe
```

### Admin Endpoints

```text
GET /api/subscriptions/all
GET /api/subscriptions/payments
```

### Viva Answer

> SubscriptionService exposes endpoints for subscribing, confirming simulated payment, handling Stripe webhook, cancelling subscription, viewing user history, and admin views.

---

## 12. ISubscriptionSvc

### File

```text
Backend/SubscriptionService/Services/ISubscriptionSvc.cs
```

### Methods

```text
SubscribeAsync
ConfirmPaymentAsync
HandleStripeWebhookAsync
GetMySubscriptionsAsync
GetMyPaymentsAsync
CancelSubscriptionAsync
GetAllSubscriptionsAsync
GetAllPaymentsAsync
```

### Viva Answer

> ISubscriptionSvc defines subscription and payment operations such as subscribe, confirm payment, handle webhook, cancel, and retrieve histories.

---

## 13. SubscriptionSvc Dependencies

### Constructor

```csharp
public SubscriptionSvc(
    AppDbContext context,
    IConfiguration config,
    IRabbitMqPublisher rabbitMqPublisher,
    ILogger<SubscriptionSvc> logger)
```

### Meaning

```text
AppDbContext -> subscription/payment database
IConfiguration -> Stripe and app settings
IRabbitMqPublisher -> premium/payment/email events
ILogger -> logs
```

### Viva Answer

> SubscriptionSvc uses AppDbContext for data, IConfiguration for Stripe settings, RabbitMQ publisher for events, and ILogger for logging.

---

## 14. Stripe Configuration

### Config Section

```json
"Stripe": {
  "Enabled": true,
  "SecretKey": "sk_test_...",
  "PublishableKey": "pk_test_...",
  "WebhookSecret": "whsec_...",
  "SuccessUrl": "http://localhost:4200/subscription/success?session_id={CHECKOUT_SESSION_ID}",
  "CancelUrl": "http://localhost:4200/premium",
  "Currency": "INR",
  "Amount": 49900
}
```

### Meaning

```text
Enabled -> whether Stripe mode is active
SecretKey -> backend Stripe API key
PublishableKey -> frontend Stripe key
WebhookSecret -> used to verify webhook signature
SuccessUrl -> Stripe redirects after success
CancelUrl -> Stripe redirects after cancellation
Amount -> amount in minor unit
Currency -> payment currency
```

### Viva Answer

> Stripe settings configure checkout mode, API keys, webhook secret, redirect URLs, currency, and amount.

---

## 15. IsStripeEnabled

### Method

```text
IsStripeEnabled
```

### Logic

Stripe mode is enabled only if:

```text
Stripe:Enabled = true
SecretKey exists
PublishableKey exists
WebhookSecret exists
```

### Viva Answer

> IsStripeEnabled checks whether Stripe mode is enabled and all required Stripe keys are configured.

---

## 16. Payment Amount

### In Config

```text
Stripe:Amount = 49900
Stripe:Currency = INR
```

### Meaning

Stripe amount is in minor unit.

```text
49900 paise = 499 INR
```

### In Database

PaymentRecord stores:

```text
Amount = amountInMinorUnit / 100
```

### Viva Answer

> Stripe amount is configured in minor units, while PaymentRecord stores decimal amount such as 499.00 INR.

---

## 17. Subscribe Endpoint

### Endpoint

```text
POST /api/subscriptions/subscribe
```

### Method

```text
SubscribeAsync
```

### Purpose

Creates a payment session/order for premium subscription.

### Viva Answer

> SubscribeAsync creates a pending payment record and either returns a Stripe Checkout Session or simulated checkout session depending on configuration.

---

## 18. Stripe Checkout Session Mode

### When Used

When `IsStripeEnabled()` returns true.

### What It Creates

Stripe Checkout Session:

```text
Mode = payment
PaymentMethodTypes = card
Line item = Premium Subscription
Amount = configured amount
Currency = configured currency
Metadata userId
```

### Returned To Frontend

```text
checkoutSessionId
checkoutUrl
publishableKey
amount
currency
mode = Stripe
message
```

### Viva Answer

> In Stripe mode, SubscribeAsync creates a Stripe Checkout Session and returns checkout URL so frontend can redirect user to Stripe.

---

## 19. Why Checkout Session Is Used

### Simple Explanation

Stripe Checkout Session provides hosted payment page.

The project does not need to build and secure custom card form.

### Benefits

- Stripe handles payment UI
- Easier PCI compliance
- Supports redirect success/cancel flow
- Works well with webhooks

### Viva Answer

> Checkout Session is used because Stripe hosts the payment page and securely handles card collection, while backend waits for webhook confirmation.

---

## 20. Simulated Payment Mode

### When Used

When Stripe is disabled or keys are missing.

### Behavior

SubscribeAsync creates local simulated session:

```text
cs_test_{guid}
```

and saves:

```text
PaymentRecord Status = Pending
```

### Returned Mode

```text
mode = Simulated
```

### Viva Answer

> Simulated payment mode is used for local demo/testing when Stripe is disabled. It creates a pending local payment session without calling Stripe.

---

## 21. Confirm Payment Endpoint

### Endpoint

```text
POST /api/subscriptions/confirm
```

### Purpose

Manually confirms payment only in simulated mode.

### Request

```csharp
public class ConfirmPaymentRequest
{
    public string PaymentSessionId { get; set; }
    public string PaymentReferenceId { get; set; }
    public string Signature { get; set; }
}
```

### Viva Answer

> ConfirmPaymentAsync is used only in simulated mode to mark a pending payment as successful and start premium activation.

---

## 22. Why Manual Confirm Is Disabled in Stripe Mode

### In Code

```csharp
if (IsStripeEnabled())
{
    throw new ValidationAppException(
        "Manual confirm is disabled when Stripe mode is enabled. Wait for Stripe webhook callback.");
}
```

### Why

Frontend or user should not be trusted to say payment succeeded.

Stripe webhook is the trusted source.

### Viva Answer

> Manual confirm is disabled in Stripe mode because payment success must be verified by Stripe webhook, not by frontend request.

---

## 23. Confirm Payment Flow in Simulated Mode

```text
1. User calls /subscribe.
2. Service creates pending PaymentRecord with simulated session id.
3. User calls /confirm with session/reference/signature.
4. Service checks Stripe mode is disabled.
5. Service finds PaymentRecord by session id and user id.
6. If already success, throws validation error.
7. PaymentRecord becomes Success.
8. PaymentIntentId/signature are saved.
9. EnsureActiveSubscriptionAsync creates/updates active subscription.
10. Payment and subscription changes are saved.
11. RabbitMQ events are published.
12. User is told to refresh claims.
```

### Viva Explanation

> Simulated confirm is a local testing flow that marks payment successful, creates subscription, publishes premium activation events, and asks user to refresh JWT claims.

---

## 24. Stripe Webhook Endpoint

### Endpoint

```text
POST /api/subscriptions/webhook/stripe
```

### Attribute

```csharp
[AllowAnonymous]
```

### Why Anonymous

Stripe server calls this endpoint.

Stripe does not send your app's JWT.

### Security

It is protected by:

```text
Stripe-Signature header
WebhookSecret
```

### Viva Answer

> Stripe webhook endpoint is anonymous because Stripe calls it without JWT, but it is protected by Stripe signature validation.

---

## 25. Why Webhook Is Needed

### Problem

Frontend redirect success page is not enough.

User can close browser, fake frontend state, or network can fail after payment.

### Trusted Source

Stripe webhook tells backend directly:

```text
Payment really completed
Checkout session expired
```

### Viva Answer

> Webhook is needed because backend should trust Stripe's server-to-server event, not frontend redirect, for payment success.

---

## 26. Reading Raw Webhook Body

### In Controller

```csharp
Request.EnableBuffering();
var rawBody = await reader.ReadToEndAsync();
var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
```

### Why Raw Body

Stripe signature verification needs the original raw request body.

If body is changed before verification, signature may fail.

### Viva Answer

> Stripe signature verification requires the raw webhook body and Stripe-Signature header.

---

## 27. Webhook Signature Validation

### Method

```text
HandleStripeWebhookAsync
```

### Code Concept

```csharp
stripeEvent = EventUtility.ConstructEvent(
    rawBody,
    signature,
    webhookSecret,
    throwOnApiVersionMismatch: false);
```

### If Invalid

Throws:

```text
ForbiddenAppException
```

### Viva Answer

> Webhook signature validation proves the event came from Stripe and was not tampered with.

---

## 28. Webhook Idempotency

### Simple Explanation

Stripe can send the same webhook more than once.

Backend must avoid duplicate processing.

### In Your Project

Checks:

```csharp
var existingEvent = await _context.WebhookEventLogs
    .FirstOrDefaultAsync(x => x.EventId == stripeEvent.Id);
```

If exists:

```text
Duplicate webhook event ignored.
```

### Viva Answer

> Webhook idempotency prevents duplicate Stripe events from activating payment multiple times. The project stores processed event IDs in WebhookEventLogs.

---

## 29. checkout.session.completed

### Meaning

Stripe Checkout Session completed.

### Project Flow

```text
1. Validate webhook signature.
2. Check duplicate event id.
3. Extract Checkout Session.
4. Find local PaymentRecord by StripeSessionId.
5. Ignore if no local payment record.
6. Ignore if payment already success.
7. Check session.PaymentStatus == paid.
8. Mark PaymentRecord Success.
9. Save payment intent id and signature.
10. Ensure active subscription.
11. Save webhook log as Processed.
12. Save changes.
13. Publish successful payment events.
```

### Viva Answer

> checkout.session.completed is handled by marking local payment success, creating/updating subscription, logging webhook event, and publishing premium activation events.

---

## 30. checkout.session.expired

### Meaning

User did not complete checkout session in time.

### Project Flow

```text
1. Extract session.
2. Find PaymentRecord by session id.
3. Mark payment Status = Failed.
4. Save webhook log as Processed.
5. Save changes.
```

### Viva Answer

> checkout.session.expired marks the local pending payment as failed and stores webhook log.

---

## 31. Ignored Stripe Events

### Project Behavior

If event type is not handled:

```text
Save webhook log with status Ignored
Return processed false
```

### Why

Not every Stripe event is relevant to this project.

### Viva Answer

> Unneeded Stripe events are logged as ignored so backend has traceability without changing payment state.

---

## 32. EnsureActiveSubscriptionAsync

### Purpose

Creates or updates the user's premium subscription after successful payment.

### Logic

```text
1. If PaymentRecord has SubscriptionId, find existing subscription.
2. Otherwise create new Premium subscription.
3. Set Status = Active.
4. Set SagaState = PendingIdentityUpdate.
5. Set StartDate = current UTC.
6. Set EndDate = current UTC + 30 days.
7. Link PaymentRecord.SubscriptionId.
```

### Viva Answer

> EnsureActiveSubscriptionAsync creates or updates active premium subscription and links it to the successful payment record.

---

## 33. Premium Duration

### In Your Project

Premium subscription lasts:

```text
30 days
```

### Code Concept

```csharp
EndDate = currentUtc.AddDays(30)
```

### Viva Answer

> Premium subscription is activated for 30 days from successful payment time.

---

## 34. PublishSuccessfulPaymentEventsAsync

### Purpose

Publishes events after successful payment.

### Events

```text
SubscriptionLifecycleEvent with Action = Activate
PaymentSucceededEvent
EmailRequestedEvent for payment success
EmailRequestedEvent for subscription upgrade
```

### Important Note

Email events are published only if payer email is available in webhook flow.

Manual confirm controller also publishes payment/subscription email events using current user's email.

### Viva Answer

> After successful payment, SubscriptionService publishes premium activation event, payment success event, and notification email events.

---

## 35. SubscriptionLifecycleEvent

### Purpose

Coordinates premium update with IdentityService.

### For Activation

```text
Action = Activate
Status = Pending
```

### For Cancellation

```text
Action = Cancel
Status = Pending
```

### Viva Answer

> SubscriptionLifecycleEvent tells IdentityService to activate or cancel user's premium flag.

---

## 36. PaymentSucceededEvent

### Purpose

Represents successful payment.

### Fields

```text
UserId
Amount
Currency
PaymentId
OrderId
```

### Viva Answer

> PaymentSucceededEvent is published after payment success for notification, tracking, or future integrations.

---

## 37. Premium Activation Saga

### Complete Flow

```text
1. Payment succeeds in SubscriptionService.
2. SubscriptionService creates/updates active subscription.
3. Subscription SagaState = PendingIdentityUpdate.
4. SubscriptionService publishes SubscriptionLifecycleEvent Action = Activate.
5. IdentityService SubscriptionEventConsumer consumes event.
6. IdentityService updates User.IsPremium = true.
7. IdentityService publishes result event to subscription.results.v2.
8. SubscriptionService IdentityResultConsumer consumes result.
9. SubscriptionService updates SagaState = Completed.
10. User calls IdentityService refresh-claims.
11. New JWT contains isPremium = true.
```

### Viva Answer

> Premium activation uses a saga-style event flow between SubscriptionService and IdentityService because payment and user premium flag are owned by different services.

---

## 38. IdentityResultConsumer

### File

```text
Backend/SubscriptionService/Messaging/IdentityResultConsumer.cs
```

### Consumes

```text
subscription.results.v2
```

### What It Does

Updates latest subscription:

```text
Completed -> SagaState = Completed or CancelledCompleted
Failed -> SagaState = CompensationRequired
```

If activation failed:

```text
Subscription Status = Failed
```

### Viva Answer

> IdentityResultConsumer listens for IdentityService saga result events and updates local subscription saga state.

---

## 39. Cancel Subscription

### Endpoint

```text
POST /api/subscriptions/cancel
```

### Method

```text
CancelSubscriptionAsync
```

### Flow

```text
1. Find active subscriptions for user.
2. If none, throw NotFoundAppException.
3. Set Status = Cancelled.
4. Set SagaState = PendingIdentityUpdate.
5. Save changes.
6. Publish SubscriptionLifecycleEvent Action = Cancel.
7. User must refresh JWT after saga completes.
```

### Viva Answer

> CancelSubscriptionAsync cancels active subscriptions locally and publishes a cancellation event so IdentityService removes premium status.

---

## 40. Why JWT Refresh Is Needed After Payment

### Simple Explanation

JWT is stateless.

Old token does not automatically update.

### Scenario

```text
1. User logs in with isPremium = false.
2. User pays successfully.
3. IdentityService updates IsPremium = true.
4. Old JWT still says isPremium = false.
5. User calls POST /api/auth/refresh-claims.
6. New JWT says isPremium = true.
```

### Viva Answer

> JWT refresh is needed after payment because old token claims do not automatically update when premium status changes in database.

---

## 41. Get My Subscriptions

### Endpoint

```text
GET /api/subscriptions/my
```

### Returns

Current user's subscription records.

### Viva Answer

> GetMySubscriptionsAsync returns subscriptions belonging to the authenticated user.

---

## 42. Get My Payments

### Endpoint

```text
GET /api/subscriptions/my/payments
```

### Returns

Current user's payment history.

### Viva Answer

> GetMyPaymentsAsync returns payment records for the authenticated user.

---

## 43. Admin Subscription and Payment Views

### Endpoints

```text
GET /api/subscriptions/all
GET /api/subscriptions/payments
```

### Protection

```csharp
[Authorize(Roles = "Admin")]
```

### Viva Answer

> Admin endpoints allow admins to view all subscriptions and all payment records, protected by Admin role.

---

## 44. Why Payment Is Not Trusted from Frontend

### Problem

Frontend can be manipulated.

A user could fake:

```text
Payment success screen
Session id
Payment reference
```

### Project Rule

In Stripe mode:

```text
Only webhook activates premium.
```

### Viva Answer

> Frontend payment success is not trusted because clients can be manipulated. Backend trusts Stripe webhook instead.

---

## 45. Webhook Endpoint Is Anonymous but Secure

### Why Anonymous

Stripe does not have JWT token.

### Why Secure

Stripe sends:

```text
Stripe-Signature
```

Backend validates it with:

```text
WebhookSecret
```

### Viva Answer

> Webhook endpoint is anonymous for Stripe access but secure because it verifies Stripe-Signature using webhook secret.

---

## 46. Duplicate Webhook Events

### Why Duplicates Happen

Stripe may retry webhook delivery if it does not receive success response.

Network issues can cause same event to arrive multiple times.

### Project Solution

```text
Store stripeEvent.Id in WebhookEventLogs
Check before processing
Ignore duplicate event ids
```

### Viva Answer

> Duplicate webhook events are ignored using WebhookEventLog, preventing repeated payment processing.

---

## 47. Payment Status Values

### PaymentRecord.Status

```text
Pending
Success
Failed
```

### Meaning

```text
Pending -> checkout created, payment not completed
Success -> payment confirmed
Failed -> checkout expired or failed
```

### Viva Answer

> PaymentRecord status tracks payment lifecycle from Pending to Success or Failed.

---

## 48. Subscription Status Values

### Subscription.Status

```text
Active
Cancelled
Failed
```

### Meaning

```text
Active -> premium subscription active
Cancelled -> user cancelled premium
Failed -> premium activation saga failed
```

### Viva Answer

> Subscription status tracks whether premium plan is active, cancelled, or failed.

---

## 49. Checkout Session ID vs Payment Intent ID

### Checkout Session ID

Represents Stripe hosted checkout session.

Example:

```text
cs_test_...
```

Stored in:

```text
StripeSessionId
```

### Payment Intent ID

Represents actual payment object created by Stripe.

Example:

```text
pi_...
```

Stored in:

```text
StripePaymentIntentId
```

### Viva Answer

> Checkout Session ID identifies the checkout flow, while Payment Intent ID identifies the actual payment transaction.

---

## 50. Error Handling

### Custom Exceptions

```text
ValidationAppException
ForbiddenAppException
NotFoundAppException
```

### Examples

```text
Manual confirm disabled in Stripe mode -> 400
Payment record not found -> 404
Missing Stripe-Signature -> 403
Invalid webhook signature -> 403
No active subscription found -> 404
Stripe mode not enabled -> 400
```

### Viva Answer

> SubscriptionService uses custom exceptions to return clear errors for invalid payment state, missing records, invalid webhook signature, and missing active subscriptions.

---

## 51. Security Features

### Security Features

- Stripe secret key stays on backend
- Stripe publishable key can go to frontend
- Stripe webhook signature validation
- Webhook event idempotency
- Admin endpoints role-protected
- User endpoints JWT-protected
- Frontend payment success not trusted
- Payment status stored server-side
- Webhook secret stored in configuration

### Viva Answer

> SubscriptionService security includes backend-only Stripe secret key, webhook signature verification, idempotency logging, JWT protection, admin role protection, and server-side payment state.

---

## 52. Complete Flow: Stripe Subscribe

```text
1. User logs in and sends POST /api/subscriptions/subscribe.
2. Controller reads userId from JWT.
3. SubscribeAsync checks Stripe configuration.
4. Stripe Checkout Session is created.
5. Pending PaymentRecord is saved with StripeSessionId.
6. SubscriptionLifecycleEvent SubscribeCreated is published.
7. Backend returns checkoutUrl and publishableKey.
8. Frontend redirects user to Stripe Checkout.
9. User pays on Stripe page.
10. Stripe redirects frontend to success URL.
11. Stripe sends webhook to backend.
```

### Viva Explanation

> Subscribe creates a Stripe Checkout Session, stores pending payment, and lets frontend redirect user to Stripe.

---

## 53. Complete Flow: Stripe Webhook Success

```text
1. Stripe sends checkout.session.completed webhook.
2. Controller reads raw body and Stripe-Signature.
3. Service verifies signature using webhook secret.
4. Service checks WebhookEventLogs for duplicate EventId.
5. Service extracts Stripe Checkout Session.
6. Service finds local PaymentRecord by session id.
7. Service checks PaymentStatus is paid.
8. PaymentRecord becomes Success.
9. StripePaymentIntentId and signature are saved.
10. Active premium Subscription is created or updated.
11. WebhookEventLog is saved as Processed.
12. SubscriptionLifecycleEvent Activate is published.
13. PaymentSucceededEvent is published.
14. Email events may be published.
15. IdentityService updates IsPremium.
16. User refreshes JWT claims.
```

### Viva Explanation

> Stripe webhook success validates payment server-to-server, updates payment/subscription data, and starts premium activation saga.

---

## 54. Complete Flow: Simulated Payment

```text
1. Stripe is disabled in configuration.
2. User calls /subscribe.
3. Service creates simulated session id.
4. Pending PaymentRecord is saved.
5. Frontend calls /confirm.
6. Service marks PaymentRecord Success.
7. Active subscription is created.
8. Premium activation events are published.
9. Controller publishes payment/subscription email events.
10. User refreshes JWT after saga completion.
```

### Viva Explanation

> Simulated payment mode supports local demo payment flow without calling Stripe.

---

## 55. Complete Flow: Cancel Subscription

```text
1. User calls POST /api/subscriptions/cancel.
2. Service finds active subscriptions.
3. Subscription status becomes Cancelled.
4. SagaState becomes PendingIdentityUpdate.
5. Service publishes SubscriptionLifecycleEvent Action = Cancel.
6. IdentityService consumes event.
7. IdentityService sets User.IsPremium = false.
8. IdentityService publishes result event.
9. IdentityResultConsumer updates SagaState to CancelledCompleted.
10. User refreshes JWT claims.
```

### Viva Explanation

> Cancellation is also event-based so IdentityService can remove premium status from the user's identity.

---

## 56. What Happens If SubscriptionService Is Down?

### Effects

Users cannot:

```text
Start payment
Confirm simulated payment
Cancel subscription
View subscription history
View payment history
```

Stripe webhooks may fail temporarily.

Stripe can retry webhook delivery.

### Viva Answer

> If SubscriptionService is down, subscription APIs fail and Stripe webhooks may be retried by Stripe later.

---

## 57. What Happens If IdentityService Is Down During Premium Activation?

### Scenario

Payment succeeds but IdentityService cannot update premium flag.

### Event-Based Behavior

RabbitMQ can keep activation event.

If IdentityService later fails the update, result event marks:

```text
SagaState = CompensationRequired
```

### Viva Answer

> If IdentityService is unavailable, premium update event can wait in RabbitMQ. If update ultimately fails, SubscriptionService marks saga as compensation required.

---

## 58. Limitations and Improvements

### Current Limitations

- Simulated confirm is only for local/demo mode
- No automatic subscription renewal
- No Stripe customer ID stored
- No refund flow
- No invoice/billing history from Stripe
- No outbox pattern for DB save plus event publish
- Webhook event save and business updates are not fully wrapped in explicit transaction
- Email publishing happens in both controller and service paths depending on mode
- No dedicated idempotency key for checkout creation
- No automatic premium expiry job

### Possible Improvements

- Add Stripe Customer ID
- Add subscription renewal/billing APIs if recurring plans are needed
- Add outbox pattern for reliable event publishing
- Add explicit database transaction for webhook processing
- Add automatic premium expiry worker
- Add refund/cancellation synchronization with Stripe
- Add idempotency key for checkout session creation
- Add payment failure email
- Add admin refund/payment detail endpoints
- Add stronger saga compensation handling

### Balanced Viva Answer

> Current SubscriptionService supports premium checkout, webhook processing, simulated payment, saga-based premium activation, and cancellation. Future improvements could include outbox pattern, automatic expiry, Stripe customer tracking, recurring billing, refunds, and stronger transaction/idempotency handling.

---

## 59. Best Full Viva Answer for Topic 16

> SubscriptionService manages premium subscriptions and payments. It supports Stripe Checkout Session mode and simulated local payment mode. When a user subscribes, the service creates a pending PaymentRecord and either returns a Stripe checkout URL or simulated session id. In Stripe mode, payment is trusted only after Stripe sends a signed webhook. The webhook endpoint is anonymous because Stripe calls it, but it validates Stripe-Signature using the webhook secret. For checkout.session.completed, the service checks idempotency using WebhookEventLog, verifies the session is paid, marks PaymentRecord as Success, creates or updates a 30-day active premium subscription, logs the webhook, and publishes RabbitMQ events. Premium activation is saga-based: SubscriptionService publishes SubscriptionLifecycleEvent, IdentityService updates User.IsPremium, and IdentityResultConsumer updates SagaState. After payment, the user must refresh JWT claims to get isPremium = true.

---

## 60. Common Viva Questions and Answers

### Q1. What is SubscriptionService?

SubscriptionService manages payment, subscription records, Stripe webhooks, premium activation, cancellation, and payment history.

### Q2. Why is SubscriptionService separate?

Because payment and subscription lifecycle are independent and security-sensitive responsibilities.

### Q3. What tables does SubscriptionService use?

Subscriptions, PaymentRecords, and WebhookEventLogs.

### Q4. What is PaymentRecord?

It stores payment attempt details such as amount, currency, Stripe session id, payment intent id, and status.

### Q5. What is WebhookEventLog?

It stores processed Stripe event ids to detect duplicate webhook events.

### Q6. Why use Stripe Checkout Session?

It provides a hosted Stripe payment page and avoids building custom card handling.

### Q7. What does /subscribe do?

It creates a pending payment and returns Stripe checkout URL or simulated checkout session.

### Q8. What is simulated payment mode?

A local demo mode used when Stripe is disabled, where payment can be manually confirmed.

### Q9. Why is manual confirm disabled in Stripe mode?

Because Stripe webhook should be the trusted source of payment success.

### Q10. Why is webhook needed?

Webhook lets Stripe notify backend directly that payment succeeded or session expired.

### Q11. Why is webhook endpoint anonymous?

Because Stripe calls it without JWT.

### Q12. How is webhook endpoint secured?

By validating Stripe-Signature header using Stripe webhook secret.

### Q13. What is checkout.session.completed?

It is a Stripe event indicating checkout completed; backend processes it if payment status is paid.

### Q14. What is checkout.session.expired?

It means checkout session expired; backend marks payment as failed.

### Q15. Why are duplicate webhook events ignored?

Stripe may retry events, so duplicate processing must be avoided.

### Q16. What is SagaState?

SagaState tracks whether premium update with IdentityService is pending, completed, failed, or cancelled.

### Q17. How is premium activated?

After successful payment, SubscriptionService publishes Activate event, IdentityService sets IsPremium true, and result event updates saga state.

### Q18. Why refresh JWT after payment?

Old JWT still has old isPremium claim, so user needs a new token.

### Q19. What is PaymentSucceededEvent?

An event published after successful payment with user id, amount, currency, payment id, and order id.

### Q20. How is subscription cancelled?

Active subscriptions are marked Cancelled and a Cancel event is published so IdentityService removes premium.

### Q21. What is Checkout Session ID?

It identifies the Stripe checkout flow.

### Q22. What is Payment Intent ID?

It identifies the actual Stripe payment transaction.

### Q23. Which endpoints are admin-only?

GET /api/subscriptions/all and GET /api/subscriptions/payments.

### Q24. What happens if payment webhook has invalid signature?

The service rejects it with forbidden error.

### Q25. What improvements can be made?

Outbox pattern, automatic expiry, Stripe customer tracking, recurring billing, refund flow, idempotency keys, and transaction wrapping.

---

## 61. Quick Revision Summary

- SubscriptionService manages payments and premium subscriptions.
- It uses Subscriptions, PaymentRecords, WebhookEventLogs.
- Stripe Checkout Session is used for hosted payment.
- Simulated mode is used when Stripe is disabled.
- Manual confirm works only in simulated mode.
- Stripe mode trusts webhook, not frontend.
- Webhook endpoint is anonymous but signature-protected.
- Stripe-Signature is validated using WebhookSecret.
- checkout.session.completed activates payment if paid.
- checkout.session.expired marks payment failed.
- WebhookEventLog prevents duplicate webhook processing.
- PaymentRecord tracks Pending, Success, Failed.
- Subscription tracks Active, Cancelled, Failed.
- Premium subscription lasts 30 days.
- Successful payment publishes Activate saga event.
- IdentityService updates User.IsPremium.
- IdentityResultConsumer updates SagaState.
- User must refresh JWT claims after premium changes.
- Cancellation publishes Cancel saga event.
- Admin can view all subscriptions and payments.
- Future improvements include outbox, expiry worker, recurring billing, refunds, and idempotency keys.

