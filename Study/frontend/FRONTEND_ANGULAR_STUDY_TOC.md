# Frontend Angular Deep Study Table Of Contents

This file is the master roadmap for studying this project's Angular frontend in depth.

Goal of the notes we will build from this table of contents:

- First understand the Angular topic in simple scenario-based language.
- Then understand what the same topic is doing inside this project.
- Then understand why the project uses it.
- Then understand what would happen if we did not use it.
- Then compare alternative ways to build the same behavior.
- Then read the actual project code line by line: TypeScript, HTML, CSS, components, services, guards, interceptors, routing, models, and API flow.

Project frontend path:

```text
Frontend/
```

Main app idea:

```text
An Angular single page app for interview practice, assessments, premium subscription, user profile, notifications, and admin management.
```

---

## 1. Big Picture: What The Frontend Is

### 1.1 What Is A Frontend?
### 1.2 What Is Angular?
### 1.3 What Is A Single Page Application?
### 1.4 How Browser, Angular, API Gateway, And Backend Services Work Together
### 1.5 Project Scenario: Candidate Uses The Website
### 1.6 Project Scenario: Admin Uses The Website
### 1.7 Full Frontend Request Flow From Button Click To Backend Response
### 1.8 What Happens When The App Loads In The Browser
### 1.9 What Happens When The User Refreshes The Page
### 1.10 What Happens When The User Logs Out

---

## 2. Project Setup And Tooling

### 2.1 `package.json`
- What npm is
- What dependencies are
- What devDependencies are
- What Angular packages do
- What RxJS does
- What TypeScript does
- What Vitest/jsdom are for
- What happens if dependencies are missing
- Alternatives: npm, pnpm, yarn

### 2.2 `angular.json`
- What Angular workspace configuration means
- Build target
- Serve target
- Test target
- Development vs production build
- Asset configuration
- Global style configuration
- Build budgets
- What happens if `angular.json` is wrong
- Alternatives: Vite config, custom builders, Nx workspace

### 2.3 TypeScript Config Files
- Why Angular uses TypeScript
- Compile-time safety
- Strictness and type checking
- How TypeScript becomes JavaScript

### 2.4 Environment Files
- File: `src/environments/environment.ts`
- What `apiUrl` means
- Why API base URLs should not be hardcoded everywhere
- What happens if the backend URL changes
- Alternatives: runtime config, `.env`, deployment variables

### 2.5 Commands
- `npm install`
- `npm start`
- `npm run build`
- `npm test`
- When to use each command
- What each command produces

---

## 3. Angular App Startup

### 3.1 `src/index.html`
- Root HTML shell
- Browser entry document
- Where Angular attaches the app

### 3.2 `src/main.ts`
- `bootstrapApplication`
- Starting a standalone Angular app
- Connecting root component with app configuration
- Error handling during startup
- What happens if bootstrap fails
- Alternative: old NgModule bootstrap style

### 3.3 `src/app/app.config.ts`
- `ApplicationConfig`
- Global providers
- `provideRouter`
- `provideHttpClient`
- `withInterceptors`
- `provideBrowserGlobalErrorListeners`
- Why global app setup is centralized here
- What happens if providers are not registered

### 3.4 `src/app/app.component.ts`
- Root component
- `@Component`
- `selector`
- `imports`
- `templateUrl`
- `styleUrl`
- Angular `signal`
- What root component owns
- What root component does not own

### 3.5 `src/app/app.component.html`
- `router-outlet`
- Why it is the page placeholder
- What happens when route changes
- What happens if `router-outlet` is removed

### 3.6 `src/app/app.component.css`
- Root-level component style
- Difference between global CSS and component CSS

---

## 4. Angular Components

### 4.1 What Is A Component?
### 4.2 Component Class, Template, And CSS
### 4.3 Component Selector
### 4.4 Component Imports In Standalone Angular
### 4.5 Component State
### 4.6 Component Methods
### 4.7 Component Lifecycle
- `constructor`
- `ngOnInit`
- `ngOnDestroy`
- Why cleanup matters
- What happens if subscriptions/timers are not cleaned up

### 4.8 Component Communication
- Parent to child
- Child to parent
- Shared service
- Router state
- Session/local storage

### 4.9 Project Components Overview
- Public components
- Auth components
- User dashboard components
- Interview components
- Assessment components
- Subscription components
- Profile components
- Shared navbar components
- Admin components

---

## 5. Angular Templates And HTML Binding

### 5.1 What A Template Is
### 5.2 Interpolation: `{{ value }}`
### 5.3 Property Binding: `[property]="value"`
### 5.4 Event Binding: `(click)="method()"`
### 5.5 Two-Way Binding: `[(ngModel)]`
### 5.6 Conditional Rendering: `*ngIf`
### 5.7 List Rendering: `*ngFor`
### 5.8 CSS Class Binding
### 5.9 Style Binding
### 5.10 Router Links In Templates
### 5.11 Forms In Templates
### 5.12 Loading, Empty, Success, And Error States
### 5.13 What Happens If Template State Does Not Match Component State
### 5.14 Alternatives: Reactive forms, Angular control flow syntax, custom directives

---

## 6. Styling And Layout

### 6.1 `src/styles.css`
- Global styles
- Browser-wide CSS
- Reset/common design rules

### 6.2 Component CSS Files
- Scoped styling
- How Angular applies component styles
- Why each component has its own CSS

### 6.3 Layout Patterns In This Project
- Auth pages
- Dashboards
- Cards and tables
- Session screens
- Admin screens
- Navbar layouts

### 6.4 Responsive Design
- Mobile vs desktop behavior
- Grids
- Flexbox
- Width constraints
- Overflow handling

### 6.5 CSS States
- Hover
- Active
- Disabled
- Loading
- Error
- Success

### 6.6 What Happens If CSS Is Global Only
### 6.7 Alternatives: SCSS, Tailwind, Angular Material, Bootstrap, design systems

---

## 7. Routing And Navigation

### 7.1 What Is Routing?
### 7.2 `src/app/app.routes.ts`
- Route array
- `path`
- `component`
- `canActivate`
- Route parameters
- Wildcard route
- Redirects

### 7.3 Public Routes
- `/`
- `/login`
- `/register`
- `/reset-password`

### 7.4 Protected User Routes
- `/user-dashboard`
- `/interviews/start`
- `/interviews/history`
- `/interviews/:id/preflight`
- `/interviews/:id/session`
- `/interviews/:id/result`
- `/assessments/domain`
- `/assessments/start`
- `/assessments/:id/session`
- `/assessments/:id/result`
- `/assessments/history`
- `/premium`
- `/subscription/success`
- `/profile`

### 7.5 Admin Routes
- `/admin`
- `/admin/users`
- `/admin/interviews`
- `/admin/assessments`
- `/admin/questions`

### 7.6 Route Parameters
- What `:id` means
- How components read route params
- Why route params are useful for interview and assessment sessions

### 7.7 Query Parameters
- What query params are
- How premium and assessment start pages use them

### 7.8 Programmatic Navigation
- `router.navigate`
- `router.navigateByUrl`
- When buttons navigate users

### 7.9 What Happens If Routing Is Removed
### 7.10 Alternatives: lazy routes, nested routes, route resolvers, module-based routing

---

## 8. Guards And Access Control

### 8.1 What Is A Route Guard?
### 8.2 Why Frontend Guards Exist
### 8.3 Why Frontend Guards Are Not Enough For Security
### 8.4 `src/app/guards/auth.guard.ts`
- `CanActivateFn`
- `inject`
- Token check
- User loading check
- Redirect to login
- Observable return
- `catchError`
- `of(false)`

### 8.5 `src/app/guards/admin.guard.ts`
- Admin role check
- Admin redirect behavior
- Why admin routes are separate

### 8.6 Scenario: User Tries To Open `/user-dashboard` Without Login
### 8.7 Scenario: Candidate Tries To Open `/admin/users`
### 8.8 Scenario: Page Refresh With Existing Token
### 8.9 What Happens If Guards Are Removed
### 8.10 Alternatives: backend-only enforcement, route resolvers, permission directives

---

## 9. HTTP, API Calls, And Backend Communication

### 9.1 What Is HTTP?
### 9.2 What Is `HttpClient`?
### 9.3 GET, POST, PUT, DELETE
### 9.4 Request Body
### 9.5 Response Body
### 9.6 Headers
### 9.7 JWT Authorization Header
### 9.8 `ApiResponse<T>` Wrapper Pattern
### 9.9 Mapping Backend Responses To Frontend Models
### 9.10 Error Handling
### 9.11 API Gateway Base URL
### 9.12 What Happens If Components Call HTTP Directly
### 9.13 Alternatives: fetch API, axios, generated API clients, GraphQL, TanStack Query-style libraries

---

## 10. Interceptors

### 10.1 What Is An Interceptor?
### 10.2 `src/app/interceptors/auth.interceptor.ts`
- `HttpInterceptorFn`
- `inject(AuthService)`
- Reading token
- `req.clone`
- Setting `Authorization: Bearer <token>`
- `next(req)`
- `catchError`
- Handling 401
- Avoiding forced logout on `/auth/me`

### 10.3 Scenario: Any Service Calls A Protected API
### 10.4 Scenario: Token Expired
### 10.5 What Happens If There Is No Interceptor
### 10.6 Alternatives: manually set headers in each service, multiple interceptors, refresh-token interceptor

---

## 11. Services And Dependency Injection

### 11.1 What Is A Service?
### 11.2 What Is Dependency Injection?
### 11.3 `@Injectable({ providedIn: 'root' })`
### 11.4 Why Services Hold Shared Logic
### 11.5 Why Components Should Not Own API Details
### 11.6 Service Method Return Types
### 11.7 Service Caching
### 11.8 What Happens If Everything Is Written Inside Components
### 11.9 Alternatives: state stores, facade services, signal stores, NgRx

### 11.10 Project Services
- `src/app/services/auth.service.ts`
- `src/app/services/interview.service.ts`
- `src/app/services/assessment.service.ts`
- `src/app/services/subscription.service.ts`
- `src/app/services/notification.service.ts`
- `src/app/admin/services/admin.service.ts`

---

## 12. RxJS And Async Data

### 12.1 What Is Async Data?
### 12.2 What Is An Observable?
### 12.3 Observable vs Promise
### 12.4 `subscribe`
### 12.5 `pipe`
### 12.6 `map`
### 12.7 `tap`
### 12.8 `catchError`
### 12.9 `of`
### 12.10 `throwError`
### 12.11 `BehaviorSubject`
### 12.12 `forkJoin`
### 12.13 When To Unsubscribe
### 12.14 Project Scenario: Auth State Stream
### 12.15 Project Scenario: Admin Dashboard Loads Multiple APIs
### 12.16 Project Scenario: History Page Loads Results For Completed Items
### 12.17 What Happens If Async Code Is Handled Poorly
### 12.18 Alternatives: async/await, Angular signals, NgRx, ComponentStore

---

## 13. Authentication And User Session

### 13.1 Login Flow
### 13.2 Register Flow
### 13.3 JWT Token Storage
### 13.4 `localStorage`
### 13.5 `sessionStorage`
### 13.6 Current User State
### 13.7 `currentUserSubject`
### 13.8 `currentUser$`
### 13.9 `currentUserValue`
### 13.10 `getMe`
### 13.11 Logout Flow
### 13.12 Role Check
### 13.13 Premium User Check
### 13.14 Password Reset OTP Flow
### 13.15 Profile Update Flow
### 13.16 What Happens If Token Is Not Stored
### 13.17 What Happens If User State Is Not Centralized
### 13.18 Alternatives: cookies, refresh tokens, OAuth/OIDC, server sessions

---

## 14. TypeScript Models And Interfaces

### 14.1 What Is A Model?
### 14.2 What Is An Interface?
### 14.3 Why Frontend Models Matter
### 14.4 Generic Interface: `ApiResponse<T>`
### 14.5 Optional Properties
### 14.6 Union Types And Flexible Backend Values
### 14.7 What Happens If Everything Is `any`
### 14.8 Alternatives: classes, zod schemas, generated OpenAPI clients

### 14.9 Project Model Files
- `src/app/models/user.model.ts`
- `src/app/models/interview.model.ts`
- `src/app/models/assessment.model.ts`
- `src/app/models/notification.model.ts`
- `src/app/admin/models/admin.models.ts`

---

## 15. Forms And User Input

### 15.1 What Is A Form?
### 15.2 Template-Driven Forms
### 15.3 `FormsModule`
### 15.4 `ngModel`
### 15.5 Input Validation
### 15.6 Form Submit Handling
### 15.7 Disabled Buttons
### 15.8 Showing Error Messages
### 15.9 Project Forms
- Login form
- Register form
- Reset password form
- Interview start form
- Assessment start form
- Assessment session answers
- Interview session answers
- Profile update form
- Admin question form

### 15.10 What Happens If Inputs Are Not Validated
### 15.11 Alternatives: Reactive Forms, custom validators, form libraries

---

## 16. State Management In This Project

### 16.1 What Is State?
### 16.2 Local Component State
### 16.3 Shared Service State
### 16.4 Browser Storage State
### 16.5 URL State
### 16.6 Server State
### 16.7 Loading/Error State
### 16.8 Derived State With Getters
### 16.9 Where State Lives In This Project
### 16.10 What Happens If State Is Duplicated Everywhere
### 16.11 Alternatives: NgRx, Akita, signal store, router state-first design

---

## 17. Candidate/User Feature Walkthrough

### 17.1 Landing Dashboard
- Files:
  - `src/app/components/dashboard/dashboard.component.ts`
  - `src/app/components/dashboard/dashboard.component.html`
  - `src/app/components/dashboard/dashboard.component.css`

### 17.2 Login
- Files:
  - `src/app/components/login/login.component.ts`
  - `src/app/components/login/login.component.html`
  - `src/app/components/login/login.component.css`

### 17.3 Register
- Files:
  - `src/app/components/register/register.component.ts`
  - `src/app/components/register/register.component.html`
  - `src/app/components/register/register.component.css`

### 17.4 Reset Password
- Files:
  - `src/app/components/reset-password/reset-password.component.ts`
  - `src/app/components/reset-password/reset-password.component.html`
  - `src/app/components/reset-password/reset-password.component.css`

### 17.5 User Dashboard
- Files:
  - `src/app/components/user-dashboard/user-dashboard.component.ts`
  - `src/app/components/user-dashboard/user-dashboard.component.html`
  - `src/app/components/user-dashboard/user-dashboard.component.css`

### 17.6 Shared User Navbar
- Files:
  - `src/app/components/shared/user-navbar/user-navbar.component.ts`
  - `src/app/components/shared/user-navbar/user-navbar.component.html`
  - `src/app/components/shared/user-navbar/user-navbar.component.css`

### 17.7 Profile
- Files:
  - `src/app/components/profile/profile.component.ts`
  - `src/app/components/profile/profile.component.html`
  - `src/app/components/profile/profile.component.css`

---

## 18. Interview Feature Walkthrough

### 18.1 Interview Feature Big Picture
### 18.2 Interview Start Page
- Files:
  - `src/app/components/interviews/interview-start/interview-start.component.ts`
  - `src/app/components/interviews/interview-start/interview-start.component.html`
  - `src/app/components/interviews/interview-start/interview-start.component.css`

### 18.3 Interview Preflight Page
- Files:
  - `src/app/components/interviews/interview-preflight/interview-preflight.component.ts`
  - `src/app/components/interviews/interview-preflight/interview-preflight.component.html`
  - `src/app/components/interviews/interview-preflight/interview-preflight.component.css`

### 18.4 Interview Session Page
- Files:
  - `src/app/components/interviews/interview-session/interview-session.component.ts`
  - `src/app/components/interviews/interview-session/interview-session.component.html`
  - `src/app/components/interviews/interview-session/interview-session.component.css`

### 18.5 Interview Result Page
- Files:
  - `src/app/components/interviews/interview-result/interview-result.component.ts`
  - `src/app/components/interviews/interview-result/interview-result.component.html`
  - `src/app/components/interviews/interview-result/interview-result.component.css`

### 18.6 Interview History Page
- Files:
  - `src/app/components/interviews/interview-history/interview-history.component.ts`
  - `src/app/components/interviews/interview-history/interview-history.component.html`
  - `src/app/components/interviews/interview-history/interview-history.component.css`

### 18.7 Interview Service
- File: `src/app/services/interview.service.ts`

### 18.8 Interview Models
- File: `src/app/models/interview.model.ts`

### 18.9 Interview Flow Scenario
- User chooses interview settings
- Frontend starts interview
- Backend returns questions/session
- User answers questions
- Frontend submits answers
- Result page loads score/feedback
- History page shows previous interviews

### 18.10 What Happens If Session Data Is Lost
### 18.11 Alternative Interview Flow Designs

---

## 19. Assessment Feature Walkthrough

### 19.1 Assessment Feature Big Picture
### 19.2 Assessment Domain Page
- Files:
  - `src/app/components/assessments/assessment-domain/assessment-domain.component.ts`
  - `src/app/components/assessments/assessment-domain/assessment-domain.component.html`
  - `src/app/components/assessments/assessment-domain/assessment-domain.component.css`

### 19.3 Assessment Start Page
- Files:
  - `src/app/components/assessments/assessment-start/assessment-start.component.ts`
  - `src/app/components/assessments/assessment-start/assessment-start.component.html`
  - `src/app/components/assessments/assessment-start/assessment-start.component.css`

### 19.4 Assessment Session Page
- Files:
  - `src/app/components/assessments/assessment-session/assessment-session.component.ts`
  - `src/app/components/assessments/assessment-session/assessment-session.component.html`
  - `src/app/components/assessments/assessment-session/assessment-session.component.css`

### 19.5 Assessment Result Page
- Files:
  - `src/app/components/assessments/assessment-result/assessment-result.component.ts`
  - `src/app/components/assessments/assessment-result/assessment-result.component.html`
  - `src/app/components/assessments/assessment-result/assessment-result.component.css`

### 19.6 Assessment History Page
- Files:
  - `src/app/components/assessments/assessment-history/assessment-history.component.ts`
  - `src/app/components/assessments/assessment-history/assessment-history.component.html`
  - `src/app/components/assessments/assessment-history/assessment-history.component.css`

### 19.7 Assessment Service
- File: `src/app/services/assessment.service.ts`

### 19.8 Assessment Models
- File: `src/app/models/assessment.model.ts`

### 19.9 Assessment Flow Scenario
- User chooses domain
- User configures assessment
- Frontend optionally warms up backend cache
- User answers MCQs
- Frontend submits answers
- Result page displays score
- History page summarizes past attempts

### 19.10 What Happens If Answer State Is Wrong
### 19.11 Alternative Assessment Flow Designs

---

## 20. Premium Subscription Feature Walkthrough

### 20.1 Premium Feature Big Picture
### 20.2 Premium Page
- Files:
  - `src/app/components/premium/premium.component.ts`
  - `src/app/components/premium/premium.component.html`
  - `src/app/components/premium/premium.component.css`

### 20.3 Subscription Success Page
- Files:
  - `src/app/components/subscription-success/subscription-success.component.ts`
  - `src/app/components/subscription-success/subscription-success.component.html`
  - `src/app/components/subscription-success/subscription-success.component.css`

### 20.4 Subscription Service
- File: `src/app/services/subscription.service.ts`

### 20.5 Payment Records
### 20.6 Premium Claims Refresh
### 20.7 Cancel Subscription Flow
### 20.8 What Happens If Claims Are Not Refreshed
### 20.9 Alternatives: Stripe Checkout only, embedded checkout, backend-rendered billing portal

---

## 21. Notifications Feature Walkthrough

### 21.1 Notification Feature Big Picture
### 21.2 Notification Model
- File: `src/app/models/notification.model.ts`

### 21.3 Notification Service
- File: `src/app/services/notification.service.ts`

### 21.4 Navbar Notification UI
- Files:
  - `src/app/components/shared/user-navbar/user-navbar.component.ts`
  - `src/app/components/shared/user-navbar/user-navbar.component.html`
  - `src/app/components/shared/user-navbar/user-navbar.component.css`

### 21.5 Fetch Notifications
### 21.6 Mark All As Read
### 21.7 Navigate From Notification Action URL
### 21.8 What Happens If Notifications Fail To Load
### 21.9 Alternatives: polling, SignalR/WebSockets, server-sent events, push notifications

---

## 22. Admin Feature Walkthrough

### 22.1 Admin Feature Big Picture
### 22.2 Admin Navbar
- Files:
  - `src/app/admin/components/admin-navbar/admin-navbar.component.ts`
  - `src/app/admin/components/admin-navbar/admin-navbar.component.html`
  - `src/app/admin/components/admin-navbar/admin-navbar.component.css`

### 22.3 Admin Dashboard
- Files:
  - `src/app/admin/components/admin-dashboard/admin-dashboard.component.ts`
  - `src/app/admin/components/admin-dashboard/admin-dashboard.component.html`
  - `src/app/admin/components/admin-dashboard/admin-dashboard.component.css`

### 22.4 Admin Users
- Files:
  - `src/app/admin/components/admin-users/admin-users.component.ts`
  - `src/app/admin/components/admin-users/admin-users.component.html`
  - `src/app/admin/components/admin-users/admin-users.component.css`

### 22.5 Admin Interviews
- Files:
  - `src/app/admin/components/admin-interviews/admin-interviews.component.ts`
  - `src/app/admin/components/admin-interviews/admin-interviews.component.html`
  - `src/app/admin/components/admin-interviews/admin-interviews.component.css`

### 22.6 Admin Assessments
- Files:
  - `src/app/admin/components/admin-assessments/admin-assessments.component.ts`
  - `src/app/admin/components/admin-assessments/admin-assessments.component.html`
  - `src/app/admin/components/admin-assessments/admin-assessments.component.css`

### 22.7 Admin Questions
- Files:
  - `src/app/admin/components/admin-questions/admin-questions.component.ts`
  - `src/app/admin/components/admin-questions/admin-questions.component.html`
  - `src/app/admin/components/admin-questions/admin-questions.component.css`

### 22.8 Admin Service
- File: `src/app/admin/services/admin.service.ts`

### 22.9 Admin Models
- File: `src/app/admin/models/admin.models.ts`

### 22.10 Admin Flow Scenario
- Admin logs in
- Admin guard checks role
- Admin dashboard loads summaries
- Admin views users/interviews/assessments/questions
- Admin edits role/premium/questions

### 22.11 What Happens If Admin UI Is Not Guarded
### 22.12 Alternatives: separate admin app, lazy admin area, role-based components

---

## 23. Line-By-Line Code Reading Method

For every `.ts`, `.html`, and `.css` file, study in this order:

### 23.1 File Purpose
### 23.2 Imports
### 23.3 Decorator Metadata
### 23.4 Class Properties
### 23.5 Constructor Dependencies
### 23.6 Lifecycle Methods
### 23.7 Event Handler Methods
### 23.8 API Call Methods
### 23.9 Helper Methods
### 23.10 Template Bindings
### 23.11 CSS Classes
### 23.12 User Scenario
### 23.13 Why This Code Exists
### 23.14 What Breaks If This Code Is Removed
### 23.15 Possible Alternatives
### 23.16 Improvement Ideas

---

## 24. File-By-File Study Checklist

### 24.1 App Shell
- [ ] `src/index.html`
- [ ] `src/main.ts`
- [ ] `src/app/app.config.ts`
- [ ] `src/app/app.component.ts`
- [ ] `src/app/app.component.html`
- [ ] `src/app/app.component.css`
- [ ] `src/styles.css`

### 24.2 Routing And Security
- [ ] `src/app/app.routes.ts`
- [ ] `src/app/guards/auth.guard.ts`
- [ ] `src/app/guards/admin.guard.ts`
- [ ] `src/app/interceptors/auth.interceptor.ts`

### 24.3 Models
- [ ] `src/app/models/user.model.ts`
- [ ] `src/app/models/interview.model.ts`
- [ ] `src/app/models/assessment.model.ts`
- [ ] `src/app/models/notification.model.ts`
- [ ] `src/app/admin/models/admin.models.ts`

### 24.4 Services
- [ ] `src/app/services/auth.service.ts`
- [ ] `src/app/services/interview.service.ts`
- [ ] `src/app/services/assessment.service.ts`
- [ ] `src/app/services/subscription.service.ts`
- [ ] `src/app/services/notification.service.ts`
- [ ] `src/app/admin/services/admin.service.ts`

### 24.5 Public/Auth Components
- [ ] `src/app/components/dashboard/dashboard.component.ts`
- [ ] `src/app/components/dashboard/dashboard.component.html`
- [ ] `src/app/components/dashboard/dashboard.component.css`
- [ ] `src/app/components/login/login.component.ts`
- [ ] `src/app/components/login/login.component.html`
- [ ] `src/app/components/login/login.component.css`
- [ ] `src/app/components/register/register.component.ts`
- [ ] `src/app/components/register/register.component.html`
- [ ] `src/app/components/register/register.component.css`
- [ ] `src/app/components/reset-password/reset-password.component.ts`
- [ ] `src/app/components/reset-password/reset-password.component.html`
- [ ] `src/app/components/reset-password/reset-password.component.css`

### 24.6 User Components
- [ ] `src/app/components/user-dashboard/user-dashboard.component.ts`
- [ ] `src/app/components/user-dashboard/user-dashboard.component.html`
- [ ] `src/app/components/user-dashboard/user-dashboard.component.css`
- [ ] `src/app/components/shared/user-navbar/user-navbar.component.ts`
- [ ] `src/app/components/shared/user-navbar/user-navbar.component.html`
- [ ] `src/app/components/shared/user-navbar/user-navbar.component.css`
- [ ] `src/app/components/profile/profile.component.ts`
- [ ] `src/app/components/profile/profile.component.html`
- [ ] `src/app/components/profile/profile.component.css`

### 24.7 Interview Components
- [ ] `src/app/components/interviews/interview-start/interview-start.component.ts`
- [ ] `src/app/components/interviews/interview-start/interview-start.component.html`
- [ ] `src/app/components/interviews/interview-start/interview-start.component.css`
- [ ] `src/app/components/interviews/interview-preflight/interview-preflight.component.ts`
- [ ] `src/app/components/interviews/interview-preflight/interview-preflight.component.html`
- [ ] `src/app/components/interviews/interview-preflight/interview-preflight.component.css`
- [ ] `src/app/components/interviews/interview-session/interview-session.component.ts`
- [ ] `src/app/components/interviews/interview-session/interview-session.component.html`
- [ ] `src/app/components/interviews/interview-session/interview-session.component.css`
- [ ] `src/app/components/interviews/interview-result/interview-result.component.ts`
- [ ] `src/app/components/interviews/interview-result/interview-result.component.html`
- [ ] `src/app/components/interviews/interview-result/interview-result.component.css`
- [ ] `src/app/components/interviews/interview-history/interview-history.component.ts`
- [ ] `src/app/components/interviews/interview-history/interview-history.component.html`
- [ ] `src/app/components/interviews/interview-history/interview-history.component.css`

### 24.8 Assessment Components
- [ ] `src/app/components/assessments/assessment-domain/assessment-domain.component.ts`
- [ ] `src/app/components/assessments/assessment-domain/assessment-domain.component.html`
- [ ] `src/app/components/assessments/assessment-domain/assessment-domain.component.css`
- [ ] `src/app/components/assessments/assessment-start/assessment-start.component.ts`
- [ ] `src/app/components/assessments/assessment-start/assessment-start.component.html`
- [ ] `src/app/components/assessments/assessment-start/assessment-start.component.css`
- [ ] `src/app/components/assessments/assessment-session/assessment-session.component.ts`
- [ ] `src/app/components/assessments/assessment-session/assessment-session.component.html`
- [ ] `src/app/components/assessments/assessment-session/assessment-session.component.css`
- [ ] `src/app/components/assessments/assessment-result/assessment-result.component.ts`
- [ ] `src/app/components/assessments/assessment-result/assessment-result.component.html`
- [ ] `src/app/components/assessments/assessment-result/assessment-result.component.css`
- [ ] `src/app/components/assessments/assessment-history/assessment-history.component.ts`
- [ ] `src/app/components/assessments/assessment-history/assessment-history.component.html`
- [ ] `src/app/components/assessments/assessment-history/assessment-history.component.css`

### 24.9 Subscription Components
- [ ] `src/app/components/premium/premium.component.ts`
- [ ] `src/app/components/premium/premium.component.html`
- [ ] `src/app/components/premium/premium.component.css`
- [ ] `src/app/components/subscription-success/subscription-success.component.ts`
- [ ] `src/app/components/subscription-success/subscription-success.component.html`
- [ ] `src/app/components/subscription-success/subscription-success.component.css`

### 24.10 Admin Components
- [ ] `src/app/admin/components/admin-navbar/admin-navbar.component.ts`
- [ ] `src/app/admin/components/admin-navbar/admin-navbar.component.html`
- [ ] `src/app/admin/components/admin-navbar/admin-navbar.component.css`
- [ ] `src/app/admin/components/admin-dashboard/admin-dashboard.component.ts`
- [ ] `src/app/admin/components/admin-dashboard/admin-dashboard.component.html`
- [ ] `src/app/admin/components/admin-dashboard/admin-dashboard.component.css`
- [ ] `src/app/admin/components/admin-users/admin-users.component.ts`
- [ ] `src/app/admin/components/admin-users/admin-users.component.html`
- [ ] `src/app/admin/components/admin-users/admin-users.component.css`
- [ ] `src/app/admin/components/admin-interviews/admin-interviews.component.ts`
- [ ] `src/app/admin/components/admin-interviews/admin-interviews.component.html`
- [ ] `src/app/admin/components/admin-interviews/admin-interviews.component.css`
- [ ] `src/app/admin/components/admin-assessments/admin-assessments.component.ts`
- [ ] `src/app/admin/components/admin-assessments/admin-assessments.component.html`
- [ ] `src/app/admin/components/admin-assessments/admin-assessments.component.css`
- [ ] `src/app/admin/components/admin-questions/admin-questions.component.ts`
- [ ] `src/app/admin/components/admin-questions/admin-questions.component.html`
- [ ] `src/app/admin/components/admin-questions/admin-questions.component.css`

---

## 25. Testing

### 25.1 What Is Frontend Testing?
### 25.2 Component Tests
### 25.3 TestBed
### 25.4 Existing Spec Files
- `src/app/app.component.spec.ts`
- `src/app/components/login/login.component.spec.ts`
- `src/app/components/register/register.component.spec.ts`
- `src/app/components/reset-password/reset-password.component.spec.ts`
- `src/app/components/dashboard/dashboard.component.spec.ts`
- `src/app/components/shared/user-navbar/user-navbar.component.spec.ts`

### 25.5 What Good Tests Should Check
### 25.6 What Happens If There Are No Tests
### 25.7 Alternatives: Cypress, Playwright, Jest, Vitest, Angular Testing Library

---

## 26. Build, Deployment, And Production Behavior

### 26.1 Development Build
### 26.2 Production Build
### 26.3 Source Maps
### 26.4 Output Hashing
### 26.5 Bundle Size Budgets
### 26.6 Static Files
### 26.7 API URL In Production
### 26.8 Browser Caching
### 26.9 What Happens If Production Config Is Wrong
### 26.10 Alternatives: Docker hosting, Nginx, Azure Static Web Apps, Netlify, Vercel

---

## 27. Common Angular Mistakes To Watch In This Project

### 27.1 Forgetting To Import A Standalone Dependency
### 27.2 Calling API Directly From Many Components
### 27.3 Not Handling Loading State
### 27.4 Not Handling Error State
### 27.5 Forgetting To Unsubscribe From Long-Lived Streams
### 27.6 Trusting Frontend Guards As Real Security
### 27.7 Storing Too Much In Local Storage
### 27.8 Using `any` Too Much
### 27.9 Duplicating Mapping Logic
### 27.10 Letting Template Logic Become Too Complex

---

## 28. Suggested Study Order

### Phase 1: Understand The App Skeleton
1. `package.json`
2. `angular.json`
3. `src/index.html`
4. `src/main.ts`
5. `src/app/app.config.ts`
6. `src/app/app.component.ts`
7. `src/app/app.component.html`
8. `src/app/app.routes.ts`

### Phase 2: Understand Login And Security
1. `src/app/models/user.model.ts`
2. `src/app/services/auth.service.ts`
3. `src/app/interceptors/auth.interceptor.ts`
4. `src/app/guards/auth.guard.ts`
5. `src/app/guards/admin.guard.ts`
6. Login/register/reset-password components

### Phase 3: Understand User Experience
1. Dashboard
2. User dashboard
3. User navbar
4. Profile
5. Notifications

### Phase 4: Understand Main Product Features
1. Interview start
2. Interview preflight
3. Interview session
4. Interview result
5. Interview history
6. Assessment domain
7. Assessment start
8. Assessment session
9. Assessment result
10. Assessment history

### Phase 5: Understand Business Features
1. Premium page
2. Subscription success
3. Subscription service
4. Payment and subscription records

### Phase 6: Understand Admin Features
1. Admin guard
2. Admin navbar
3. Admin dashboard
4. Admin users
5. Admin interviews
6. Admin assessments
7. Admin questions
8. Admin service

### Phase 7: Improve And Test
1. Existing tests
2. Missing tests
3. Refactoring opportunities
4. Better state management options
5. Better error handling options

---

## 29. Study Template For Each Topic

Use this same structure when expanding any topic:

```text
Topic:

1. Simple explanation
2. Real-life scenario
3. Angular meaning
4. What this project does with it
5. Why this project uses it
6. What happens if we do not use it
7. Alternatives
8. Exact files involved
9. Line-by-line code explanation
10. Common mistakes
11. Mini exercise
12. Interview-style questions
```

---

## 30. First Topic To Expand Next

Recommended first full lesson:

```text
Angular App Startup:
index.html -> main.ts -> app.config.ts -> app.component.ts -> app.component.html -> app.routes.ts
```

Why start here:

```text
Because every page, component, guard, service, and API call depends on understanding how Angular first enters the browser and decides which page to show.
```
