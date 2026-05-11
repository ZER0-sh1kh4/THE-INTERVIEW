# Topic 19: Stripe Integration

Project: Mock Interview Platform  
Focus: Understanding Stripe payment gateway integration, Checkout Sessions, publishable key vs secret key, webhook secret, Payment Intent, webhook event types, signature verification, test mode vs live mode, payment success/failure states, and why backend trusts Stripe webhook instead of frontend.

---

## 1. What Is Stripe?

### Simple Explanation

Stripe is a payment gateway.

It helps applications accept online payments securely.

### In Your Project

Stripe is used for:

```text
Premium subscription payment
Checkout page
Payment success verification
Webhook callback
Payment intent tracking
```

### Viva Answer

> Stripe is a payment gateway used in my project to collect premium subscription payments securely through hosted checkout and webhooks.

---

## 2. What Is Payment Gateway?

### Simple Explanation

A payment gateway processes online payments between user, bank/card network, and application.

### Without Payment Gateway

Your backend would need to handle:

```text
Card details
Bank communication
Payment security
Fraud checks
PCI compliance
```

### With Stripe

Stripe handles payment collection and tells backend when payment succeeds.

### Viva Answer

> A payment gateway handles secure online payment processing. Stripe acts as the payment gateway in my project.

---

## 3. Why Stripe Is Used

### Reasons

- Secure payment collection
- Hosted checkout page
- Card data handled by Stripe
- Webhook support
- Test mode support
- Payment intent/session IDs
- Better security than trusting frontend

### In Your Project

Frontend redirects user to Stripe Checkout.

Backend waits for Stripe webhook before activating premium.

### Viva Answer

> Stripe is used because it provides secure hosted checkout, test mode, webhook verification, and reliable payment confirmation.

---

## 4. Important Stripe Files in Your Project

### SubscriptionService

```text
Backend/SubscriptionService/Controllers/SubscriptionController.cs
Backend/SubscriptionService/Services/SubscriptionService.cs
Backend/SubscriptionService/Models/SubscriptionModels.cs
Backend/SubscriptionService/Data/AppDbContext.cs
Backend/SubscriptionService/appsettings.Example.json
```

### Important Methods

```text
SubscribeAsync
HandleStripeWebhookAsync
EnsureActiveSubscriptionAsync
PublishSuccessfulPaymentEventsAsync
SaveWebhookLogAsync
```

### Viva Answer

> Stripe integration is implemented mainly in SubscriptionService through SubscribeAsync, HandleStripeWebhookAsync, PaymentRecord, Subscription, and WebhookEventLog.

---

## 5. Stripe Configuration

### Config Section

```json
"Stripe": {
  "Enabled": true,
  "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY_HERE",
  "PublishableKey": "pk_test_YOUR_STRIPE_PUBLISHABLE_KEY_HERE",
  "WebhookSecret": "whsec_YOUR_STRIPE_WEBHOOK_SECRET_HERE",
  "SuccessUrl": "http://localhost:4200/subscription/success?session_id={CHECKOUT_SESSION_ID}",
  "CancelUrl": "http://localhost:4200/premium",
  "Currency": "INR",
  "Amount": 49900
}
```

### Meaning

```text
Enabled -> turn Stripe mode on/off
SecretKey -> backend Stripe API key
PublishableKey -> frontend Stripe key
WebhookSecret -> verifies webhook signature
SuccessUrl -> redirect after successful checkout
CancelUrl -> redirect after checkout cancellation
Currency -> INR
Amount -> amount in minor unit
```

### Viva Answer

> Stripe configuration contains enabled flag, secret key, publishable key, webhook secret, redirect URLs, currency, and amount.

---

## 6. Publishable Key vs Secret Key

### Publishable Key

Starts with:

```text
pk_test_
```

Can be used by frontend.

It identifies your Stripe account but cannot perform secret backend operations.

### Secret Key

Starts with:

```text
sk_test_
```

Must stay on backend.

Used to create Checkout Sessions and call Stripe APIs.

### Viva Answer

> Publishable key can be exposed to frontend, but secret key must stay on backend because it can perform sensitive Stripe API operations.

---

## 7. Webhook Secret

### Starts With

```text
whsec_
```

### Purpose

Used to verify that webhook request really came from Stripe.

### In Your Project

```csharp
var webhookSecret = _config["Stripe:WebhookSecret"];
```

### Viva Answer

> Webhook secret is used to verify Stripe webhook signature and protect the webhook endpoint from fake requests.

---

## 8. Test Mode vs Live Mode

### Test Mode

Uses:

```text
sk_test_
pk_test_
Test cards
Stripe CLI/testing webhooks
```

No real money is charged.

### Live Mode

Uses:

```text
sk_live_
pk_live_
Real cards
Real payments
```

### In Your Project

Example config uses test keys:

```text
sk_test_
pk_test_
```

### Viva Answer

> Test mode is used for development without real money, while live mode is used in production with real payments.

---

## 9. Stripe Checkout Session

### Simple Explanation

Checkout Session represents a hosted Stripe checkout flow.

It creates a payment page where user can pay securely.

### In Your Project

Created in:

```text
SubscribeAsync
```

using:

```csharp
var service = new SessionService();
var session = await service.CreateAsync(options);
```

### Viva Answer

> Stripe Checkout Session represents the hosted payment checkout flow. Backend creates it and frontend redirects user to session URL.

---

## 10. Why Checkout Session Is Better Here

### Reason

The app needs a simple one-time premium payment.

Checkout Session is suitable because:

- Stripe hosts payment UI
- Backend does not handle card details
- Easier integration
- Webhooks work naturally
- Lower PCI burden

### Viva Answer

> Checkout Session is used because it provides hosted Stripe payment page and avoids handling card details directly in the application.

---

## 11. Creating Checkout Session

### In SubscribeAsync

```csharp
var options = new SessionCreateOptions
{
    Mode = "payment",
    SuccessUrl = ...,
    CancelUrl = ...,
    PaymentMethodTypes = new List<string> { "card" },
    LineItems = ...,
    Metadata = ...
};
```

### Important Values

```text
Mode = payment
Product = Premium Subscription
Description = 30-day premium access
Amount = Stripe:Amount
Currency = Stripe:Currency
Metadata userId
```

### Viva Answer

> SubscribeAsync creates a Stripe Checkout Session with payment mode, success/cancel URLs, card payment method, line item, and user metadata.

---

## 12. Checkout URL

### Returned To Frontend

```text
checkoutUrl = session.Url
```

### Frontend Flow

Frontend redirects user to Stripe-hosted checkout page.

### Viva Answer

> Checkout URL is returned to frontend so user can be redirected to Stripe's hosted payment page.

---

## 13. Success URL and Cancel URL

### Success URL

Used when checkout completes.

```text
http://localhost:4200/subscription/success?session_id={CHECKOUT_SESSION_ID}
```

### Cancel URL

Used when user cancels payment.

```text
http://localhost:4200/premium
```

### Important Note

Success URL does not prove payment succeeded.

Backend still waits for webhook.

### Viva Answer

> Success and cancel URLs control frontend redirects after Stripe checkout, but backend still trusts webhook for payment confirmation.

---

## 14. Checkout Session ID

### Example

```text
cs_test_...
```

### Stored In

```text
PaymentRecord.StripeSessionId
```

### Purpose

Links local pending payment to Stripe checkout session.

### Viva Answer

> Checkout Session ID identifies the Stripe checkout flow and is stored in PaymentRecord as StripeSessionId.

---

## 15. Payment Intent ID

### Example

```text
pi_...
```

### Stored In

```text
PaymentRecord.StripePaymentIntentId
```

### Purpose

Identifies the actual payment transaction inside Stripe.

### Viva Answer

> Payment Intent ID identifies the actual Stripe payment transaction created from the checkout session.

---

## 16. Checkout Session ID vs Payment Intent ID

### Checkout Session ID

```text
Represents checkout page/session
Starts with cs_
Used to find local PaymentRecord
```

### Payment Intent ID

```text
Represents actual payment
Starts with pi_
Stored after payment succeeds
```

### Viva Answer

> Checkout Session ID tracks the checkout flow, while Payment Intent ID tracks the actual payment transaction.

---

## 17. PaymentRecord in Stripe Flow

### When Created

After Checkout Session is created.

### Initial Status

```text
Pending
```

### Stored Fields

```text
UserId
Amount
Currency
StripeSessionId
Status
```

### Viva Answer

> PaymentRecord is created as Pending when checkout session is created, and later updated after webhook confirms payment.

---

## 18. Why Payment Is Not Trusted From Frontend

### Problem

Frontend can be manipulated.

User can fake:

```text
Success page
Session id
Payment status
```

### Project Rule

In Stripe mode:

```text
Manual confirm is disabled
Webhook is required
```

### Viva Answer

> Payment success is not trusted from frontend because client-side data can be manipulated. Backend waits for Stripe webhook.

---

## 19. Manual Confirm in Simulated Mode

### Endpoint

```text
POST /api/subscriptions/confirm
```

### Used Only When

```text
Stripe is disabled
```

### Why Exists

Local demo/testing without real Stripe.

### In Stripe Mode

Throws:

```text
Manual confirm is disabled when Stripe mode is enabled.
```

### Viva Answer

> Manual confirm is only for simulated local mode. In Stripe mode, backend waits for webhook.

---

## 20. Stripe Webhook

### Simple Explanation

Webhook is a server-to-server callback from Stripe to your backend.

### Endpoint

```text
POST /api/subscriptions/webhook/stripe
```

### Purpose

Stripe tells backend:

```text
Checkout completed
Checkout expired
Other Stripe event happened
```

### Viva Answer

> Stripe webhook is a server-to-server notification that tells backend about payment events.

---

## 21. Why Webhook Endpoint Is AllowAnonymous

### In Controller

```csharp
[AllowAnonymous]
[HttpPost("webhook/stripe")]
```

### Why

Stripe does not send JWT token.

It calls backend directly.

### Security Comes From

```text
Stripe-Signature header
WebhookSecret
```

### Viva Answer

> Webhook endpoint is anonymous because Stripe cannot send JWT, but it is protected by Stripe signature verification.

---

## 22. Raw Body Requirement

### In Controller

```csharp
Request.EnableBuffering();
var rawBody = await reader.ReadToEndAsync();
Request.Body.Position = 0;
```

### Why

Stripe signature verification requires original raw request body.

### Viva Answer

> Raw webhook body is required because Stripe signature verification checks the exact original payload.

---

## 23. Stripe-Signature Header

### Header

```text
Stripe-Signature
```

### Read In Controller

```csharp
var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
```

### If Missing

Service throws:

```text
Missing Stripe-Signature header.
```

### Viva Answer

> Stripe-Signature header is used with webhook secret to verify webhook authenticity.

---

## 24. Signature Verification

### Code Concept

```csharp
stripeEvent = EventUtility.ConstructEvent(
    rawBody,
    signature,
    webhookSecret,
    throwOnApiVersionMismatch: false);
```

### If Invalid

```text
ForbiddenAppException
Invalid Stripe webhook signature or webhook secret.
```

### Viva Answer

> Signature verification ensures the webhook really came from Stripe and was not tampered with.

---

## 25. Webhook Event Types

### Handled Events

```text
checkout.session.completed
checkout.session.expired
```

### Ignored Events

Other Stripe events are logged as:

```text
Ignored
```

### Viva Answer

> The project handles checkout.session.completed and checkout.session.expired, and logs other Stripe events as ignored.

---

## 26. checkout.session.completed

### Meaning

Checkout session completed.

### Backend Still Checks

```text
session.PaymentStatus == paid
```

### Why

Completion event should still be validated before marking payment success.

### Viva Answer

> checkout.session.completed means checkout completed, but backend still checks PaymentStatus is paid before activating premium.

---

## 27. checkout.session.completed Flow

```text
1. Stripe sends webhook.
2. Backend verifies signature.
3. Backend checks duplicate EventId.
4. Extract Checkout Session.
5. Find local PaymentRecord by session.Id.
6. Ignore if no local payment exists.
7. Ignore if payment already Success.
8. Check session.PaymentStatus == paid.
9. Mark PaymentRecord Success.
10. Save StripePaymentIntentId.
11. Save StripeSignature.
12. Create/update active subscription.
13. Save WebhookEventLog as Processed.
14. Publish payment and premium activation events.
```

### Viva Explanation

> checkout.session.completed webhook is the trusted payment success flow that updates payment, subscription, webhook log, and publishes events.

---

## 28. checkout.session.expired

### Meaning

User did not complete checkout session in time.

### Project Behavior

```text
Find PaymentRecord by session id
Set Status = Failed
Save webhook log as Processed
```

### Viva Answer

> checkout.session.expired marks the local pending payment as failed.

---

## 29. Webhook Idempotency

### Simple Explanation

Stripe may send same event multiple times.

Backend must process it only once.

### Project Solution

```text
WebhookEventLogs table
Unique EventId index
Duplicate EventId check
```

### Code Concept

```csharp
var existingEvent = await _context.WebhookEventLogs
    .FirstOrDefaultAsync(x => x.EventId == stripeEvent.Id);
```

### Viva Answer

> Webhook idempotency prevents duplicate Stripe events from processing payment multiple times.

---

## 30. WebhookEventLog

### Stores

```text
EventId
EventType
OrderId
PaymentId
Status
ProcessedAt
```

### Status Examples

```text
Processed
Ignored
```

### Viva Answer

> WebhookEventLog stores Stripe webhook processing history and helps ignore duplicate events.

---

## 31. Payment Status

### PaymentRecord Status Values

```text
Pending
Success
Failed
```

### Meaning

```text
Pending -> checkout created
Success -> Stripe confirmed paid
Failed -> checkout expired or failed
```

### Viva Answer

> PaymentRecord status tracks whether payment is pending, successful, or failed.

---

## 32. Premium Activation After Stripe Payment

### After Payment Success

Service calls:

```text
EnsureActiveSubscriptionAsync
PublishSuccessfulPaymentEventsAsync
```

### Subscription

```text
Plan = Premium
Status = Active
SagaState = PendingIdentityUpdate
EndDate = now + 30 days
```

### Viva Answer

> After Stripe payment success, SubscriptionService creates or updates active premium subscription and starts premium activation saga.

---

## 33. PublishSuccessfulPaymentEventsAsync

### Events Published

```text
SubscriptionLifecycleEvent
PaymentSucceededEvent
EmailRequestedEvent for payment success
EmailRequestedEvent for subscription upgrade
```

### Why

Other services need to react asynchronously.

### Viva Answer

> Successful Stripe payment publishes events for premium activation, payment tracking, and email notifications.

---

## 34. Stripe and RabbitMQ Together

### Flow

```text
Stripe confirms payment
SubscriptionService updates local DB
SubscriptionService publishes RabbitMQ events
IdentityService updates premium flag
NotificationService sends emails
```

### Viva Answer

> Stripe confirms payment, and RabbitMQ coordinates premium activation and notification across services.

---

## 35. Why JWT Refresh Is Needed

### Problem

Payment updates database, but old JWT still has old claim.

### Required Action

After premium activation:

```text
POST /api/auth/refresh-claims
```

### Result

New token contains:

```text
isPremium = true
```

### Viva Answer

> JWT refresh is needed because old token does not automatically know that Stripe payment activated premium.

---

## 36. Stripe Test Cards

### Concept

In Stripe test mode, developers use Stripe test card numbers to simulate payment success/failure.

### Why Useful

Test payment flow without real money.

### Viva Answer

> Stripe test mode allows payment flow testing with test cards without charging real money.

---

## 37. Stripe CLI

### Concept

Stripe CLI can forward webhook events to local backend.

### Why Useful

Localhost is not publicly accessible to Stripe by default.

Stripe CLI forwards events to:

```text
http://localhost:<port>/api/subscriptions/webhook/stripe
```

### Viva Answer

> Stripe CLI helps test webhooks locally by forwarding Stripe events to localhost.

---

## 38. API Version Mismatch Handling

### In Your Code

```csharp
throwOnApiVersionMismatch: false
```

### Why

Stripe CLI may forward events using a newer API version than the installed Stripe.NET package expects.

### Viva Answer

> The project allows Stripe API version mismatch during webhook construction so local test webhooks still work while keeping signature validation enabled.

---

## 39. Stripe Secret Storage

### Secrets

```text
SecretKey
WebhookSecret
```

### Should Not Be

```text
Committed to Git
Exposed to frontend
Logged
Hardcoded
```

### Better Storage

```text
Environment variables
User secrets
Cloud secret manager
Key vault
```

### Viva Answer

> Stripe secret key and webhook secret must be stored securely on backend and never exposed to frontend.

---

## 40. Payment Security Features

### In Your Project

- Hosted Stripe Checkout
- Backend-only secret key
- Webhook signature validation
- Server-side payment record
- Duplicate webhook detection
- Frontend success not trusted
- Admin endpoints role-protected
- Webhook secret in configuration

### Viva Answer

> Stripe integration security includes hosted checkout, webhook signature validation, server-side payment records, idempotency, and backend-only secret key.

---

## 41. What Happens If Webhook Signature Is Invalid?

### Flow

```text
1. Stripe webhook request arrives.
2. Backend tries ConstructEvent.
3. Signature validation fails.
4. StripeException is caught.
5. Backend throws ForbiddenAppException.
6. Payment is not processed.
```

### Viva Answer

> Invalid webhook signature causes the request to be rejected and payment is not processed.

---

## 42. What Happens If Local PaymentRecord Is Missing?

### Scenario

Stripe sends webhook for session not found in database.

### Project Behavior

```text
Save webhook log as Ignored
Return processed false
Do not activate premium
```

### Viva Answer

> If local PaymentRecord is missing, webhook is ignored and premium is not activated.

---

## 43. What Happens If Payment Already Processed?

### Scenario

Duplicate event or already successful PaymentRecord.

### Project Behavior

```text
Save webhook log as Ignored
Return payment already processed
Do not activate again
```

### Viva Answer

> If payment is already successful, webhook is ignored to prevent duplicate activation.

---

## 44. What Happens If Stripe Session Is Not Paid?

### Project Check

```csharp
if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
```

### Behavior

```text
Save webhook log as Ignored
Do not mark payment Success
Do not activate premium
```

### Viva Answer

> If Stripe session is not paid, backend does not activate premium.

---

## 45. Complete Flow: Stripe Payment

```text
1. User clicks premium subscribe.
2. Frontend calls POST /api/subscriptions/subscribe.
3. Backend creates Stripe Checkout Session.
4. Backend saves PaymentRecord with Status = Pending.
5. Backend returns checkoutUrl.
6. Frontend redirects user to Stripe Checkout.
7. User pays on Stripe hosted page.
8. Stripe redirects user to success URL.
9. Stripe sends checkout.session.completed webhook.
10. Backend verifies Stripe-Signature.
11. Backend checks duplicate EventId.
12. Backend finds PaymentRecord by StripeSessionId.
13. Backend checks PaymentStatus is paid.
14. Backend marks payment Success.
15. Backend stores PaymentIntentId.
16. Backend creates active Premium subscription.
17. Backend logs webhook as Processed.
18. Backend publishes premium activation event.
19. IdentityService updates IsPremium.
20. User refreshes JWT claims.
```

### Viva Explanation

> Stripe payment flow starts with Checkout Session and completes only after webhook confirms paid status.

---

## 46. Complete Flow: Stripe Expired Session

```text
1. User starts checkout but does not pay.
2. Stripe checkout session expires.
3. Stripe sends checkout.session.expired webhook.
4. Backend verifies signature.
5. Backend finds PaymentRecord by session id.
6. Backend marks PaymentRecord Failed.
7. Backend saves WebhookEventLog as Processed.
8. Premium is not activated.
```

### Viva Explanation

> Expired checkout session marks payment failed and does not activate premium.

---

## 47. Stripe Integration vs Simulated Mode

### Stripe Mode

```text
Creates real Stripe Checkout Session
Frontend redirects to Stripe
Webhook confirms payment
Manual confirm disabled
```

### Simulated Mode

```text
Creates fake cs_test session id
No Stripe call
Manual confirm allowed
Used for local demo
```

### Viva Answer

> Stripe mode uses real Checkout and webhook verification, while simulated mode is only for local testing without Stripe.

---

## 48. Why Not Use Charges API?

### Stripe Best Practice

Do not use legacy Charges API.

Use:

```text
Checkout Sessions
PaymentIntents
Billing APIs
```

### In Your Project

Checkout Sessions are appropriate for this web app payment flow.

### Viva Answer

> Charges API is legacy. My project uses Checkout Sessions, which is the recommended Stripe integration for hosted web checkout.

---

## 49. Checkout Sessions vs PaymentIntents

### Checkout Sessions

Best for hosted checkout flow.

Stripe handles payment page.

### PaymentIntents

Best when app needs more custom payment UI/control.

### In Your Project

Checkout Sessions are used because hosted checkout is simpler and safer.

### Viva Answer

> Checkout Sessions are used because the project needs hosted checkout, while PaymentIntents are more useful for custom payment flows.

---

## 50. Limitations and Improvements

### Current Limitations

- Only card payment method is specified
- No Stripe customer ID stored
- No recurring Stripe Billing subscription
- No refund flow
- No invoice history
- No idempotency key for checkout creation
- No automatic premium expiry job
- Webhook processing could use explicit transaction
- Payment success email differs between simulated and webhook paths

### Possible Improvements

- Use dynamic payment methods
- Store Stripe Customer ID
- Add Stripe Billing if recurring plans are needed
- Add refund handling
- Add invoice/payment receipt sync
- Add idempotency key for Checkout Session creation
- Add explicit transaction around webhook processing
- Add outbox pattern for payment events
- Add premium expiry background worker
- Add Stripe webhook alerting/monitoring

### Balanced Viva Answer

> Current Stripe integration correctly uses Checkout Sessions and webhook verification for premium payment. Future improvements could include dynamic payment methods, customer tracking, Billing subscriptions, refund handling, idempotency keys, and stronger transaction/outbox handling.

---

## 51. Best Full Viva Answer for Topic 19

> My project integrates Stripe as the payment gateway for premium subscription. When the user calls subscribe, SubscriptionService creates a Stripe Checkout Session using the backend secret key, configured amount, currency, success URL, cancel URL, and user metadata. A local PaymentRecord is saved as Pending with the StripeSessionId, and frontend redirects the user to Stripe's hosted checkout URL. The backend does not trust frontend success. Premium is activated only when Stripe sends a signed webhook. The webhook endpoint is AllowAnonymous because Stripe does not send JWT, but it verifies Stripe-Signature using the webhook secret and raw body. For checkout.session.completed, backend checks duplicate EventId, finds local payment record, ensures PaymentStatus is paid, marks payment Success, stores PaymentIntentId, creates active premium subscription, logs webhook, and publishes RabbitMQ events to activate premium in IdentityService. User must refresh JWT claims after activation.

---

## 52. Common Viva Questions and Answers

### Q1. What is Stripe?

Stripe is a payment gateway used to accept online payments securely.

### Q2. What is payment gateway?

A payment gateway processes online payment between user, bank/card network, and application.

### Q3. Why use Stripe Checkout Session?

Because Stripe hosts the payment page and handles card details securely.

### Q4. What does /subscribe do?

It creates a Stripe Checkout Session and saves a pending PaymentRecord.

### Q5. What is publishable key?

Frontend-safe Stripe key starting with pk_test or pk_live.

### Q6. What is secret key?

Backend-only Stripe key starting with sk_test or sk_live.

### Q7. What is webhook secret?

Secret used to verify webhook signature.

### Q8. What is Checkout Session ID?

ID of Stripe hosted checkout flow, stored as StripeSessionId.

### Q9. What is Payment Intent ID?

ID of actual Stripe payment transaction, stored as StripePaymentIntentId.

### Q10. Why not trust frontend payment success?

Frontend can be manipulated, so backend trusts Stripe webhook.

### Q11. What is Stripe webhook?

Server-to-server callback from Stripe to backend about payment events.

### Q12. Why is webhook endpoint anonymous?

Stripe calls it without JWT.

### Q13. How is webhook endpoint secured?

By verifying Stripe-Signature header using webhook secret.

### Q14. Why read raw request body?

Stripe signature verification needs original raw body.

### Q15. What event means checkout completed?

checkout.session.completed.

### Q16. What does backend check before marking payment success?

It checks PaymentStatus is paid and local PaymentRecord exists.

### Q17. What event means checkout expired?

checkout.session.expired.

### Q18. What happens on expired checkout?

PaymentRecord is marked Failed.

### Q19. Why store WebhookEventLog?

To detect duplicate webhook events and keep processing history.

### Q20. Why can Stripe send duplicate webhook events?

Stripe retries if delivery fails or response is not received.

### Q21. What happens if webhook signature is invalid?

Request is rejected and payment is not processed.

### Q22. What is test mode?

Stripe mode using test keys and test cards without real money.

### Q23. What is live mode?

Stripe mode using live keys and real payments.

### Q24. Why refresh JWT after payment?

Old JWT still has old isPremium claim.

### Q25. What improvements can be made?

Dynamic payment methods, customer IDs, recurring billing, refunds, idempotency keys, transactions, and outbox pattern.

---

## 53. Quick Revision Summary

- Stripe is the payment gateway.
- Project uses Stripe for premium payment.
- Checkout Session creates hosted payment page.
- Secret key stays on backend.
- Publishable key can be used by frontend.
- Webhook secret verifies webhook.
- Amount is stored in minor unit.
- Checkout Session ID starts with cs_.
- Payment Intent ID starts with pi_.
- PaymentRecord starts as Pending.
- Frontend success is not trusted.
- Webhook confirms payment.
- Webhook endpoint is AllowAnonymous.
- Stripe-Signature protects webhook.
- Raw body is needed for signature verification.
- checkout.session.completed handles successful checkout.
- PaymentStatus must be paid.
- checkout.session.expired marks payment Failed.
- WebhookEventLog prevents duplicate processing.
- Premium activation happens after webhook.
- RabbitMQ event updates IdentityService premium flag.
- JWT refresh is needed after payment.
- Simulated mode is only for local testing.
- Future improvements include dynamic payment methods, recurring billing, refunds, idempotency keys, and outbox pattern.

