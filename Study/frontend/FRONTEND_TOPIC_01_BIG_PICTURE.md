# Topic 1: Big Picture - What The Frontend Is

This lesson explains the frontend of this Angular project in simple, scenario-based language.

Project frontend path:

```text
Frontend/
```

Main idea:

```text
This frontend is the website part of your project. It is what the user sees, clicks, types into, and navigates through.
```

Your frontend is built with Angular. It talks to the backend through API calls. The backend does the heavy work like login, saving interviews, checking assessments, managing subscriptions, and admin data.

---

## 1.1 What Is A Frontend?

### Simple Explanation

A frontend is the visible part of an application.

Think of a restaurant:

- The frontend is the dining area, menu, table, waiter, buttons, forms, pages, and visible experience.
- The backend is the kitchen, billing system, storage room, and staff logic behind the scenes.

In a website:

- The frontend shows pages.
- The frontend accepts user input.
- The frontend sends requests to the backend.
- The frontend displays the backend response.

### Easy Scenario

Suppose a candidate opens your website and clicks **Login**.

The frontend is responsible for:

1. Showing the login form.
2. Taking the email and password.
3. Calling the backend login API.
4. Receiving the login result.
5. Saving the token if login succeeds.
6. Moving the user to the dashboard.
7. Showing an error if login fails.

The frontend does not directly check the password from the database. That is backend work.

### What It Is Doing In This Project

In your project, the frontend contains:

- Public pages like landing, login, register, reset password.
- Protected user pages like dashboard, interview, assessment, premium, profile.
- Admin pages like users, interviews, assessments, and questions.
- Services that call backend APIs.
- Guards that stop unauthenticated users from opening protected pages.
- An interceptor that attaches the JWT token to API requests.
- Models that define the shape of data used by the frontend.

Important frontend folders:

```text
src/app/components/
src/app/admin/
src/app/services/
src/app/guards/
src/app/interceptors/
src/app/models/
```

### Why We Use A Frontend

We use a frontend because users need a friendly way to interact with the system.

Without a frontend, a user would need to manually call backend APIs using tools like Postman or curl. That is not practical for normal users.

### What If We Do Not Use A Frontend?

If there is no frontend:

- Users cannot easily login, register, or reset password.
- Candidates cannot easily start interviews or assessments.
- Admins cannot easily manage users or questions.
- The backend may still exist, but it becomes hard to use.
- The project becomes more like an API-only system.

### Alternative Ways

Instead of Angular frontend, we could use:

- React
- Vue
- Svelte
- Plain HTML, CSS, JavaScript
- Server-rendered pages using ASP.NET MVC/Razor
- Mobile app frontend using Flutter, React Native, or native Android/iOS

Angular is useful here because your app has many pages, forms, routes, protected screens, services, and reusable UI parts.

---

## 1.2 What Is Angular?

### Simple Explanation

Angular is a frontend framework.

A framework gives us a structured way to build a large app. It gives rules, tools, and ready-made features so we do not have to build everything from zero.

Angular helps with:

- Components
- Routing
- Forms
- API calls
- Dependency injection
- Guards
- Interceptors
- Templates
- Styling
- Testing
- Build and deployment

### Easy Scenario

Imagine building a house.

Without a framework:

```text
You buy bricks, cement, wires, doors, pipes, and try to connect everything manually.
```

With Angular:

```text
Angular gives you a house-building system: rooms, wiring rules, plumbing rules, entry points, and ways for rooms to communicate.
```

In Angular terms:

- A page is usually a component.
- A service handles shared logic and API calls.
- A route decides which component should show.
- A guard decides whether the user can open a route.
- An interceptor modifies API requests globally.

### What Angular Is Doing In This Project

Your project uses Angular to organize the frontend into clear parts.

Examples:

```text
src/main.ts
```

Starts the Angular app.

```text
src/app/app.config.ts
```

Registers app-level providers like routing and HTTP client.

```text
src/app/app.routes.ts
```

Defines which page opens for each URL.

```text
src/app/components/login/login.component.ts
```

Controls the login page behavior.

```text
src/app/services/auth.service.ts
```

Handles login, register, logout, token storage, and current user data.

```text
src/app/guards/auth.guard.ts
```

Protects pages from users who are not logged in.

```text
src/app/interceptors/auth.interceptor.ts
```

Adds the JWT token to backend API requests.

### Why We Use Angular In This Project

Angular is suitable because this project has:

- Many pages.
- User and admin areas.
- Authentication.
- Protected routes.
- Forms.
- API-heavy features.
- Reusable navbar.
- Multiple feature flows like interview, assessment, subscription, and admin.

Angular helps keep these pieces organized.

### What If We Do Not Use Angular?

If we use only plain JavaScript:

- Routing becomes manual.
- Form handling becomes repetitive.
- API call organization becomes harder.
- User login state becomes harder to manage.
- Large project structure can become messy.
- Reusable components become more difficult to maintain.

### Alternative Ways

Angular alternatives:

| Alternative | How It Compares |
| --- | --- |
| React | Flexible and popular, but needs more library choices for routing/forms/state |
| Vue | Easier learning curve, good for small to medium apps |
| Svelte | Very simple and fast, compiles components differently |
| Plain JS | Good for small pages, difficult for large apps |
| ASP.NET Razor | Backend renders HTML, useful when frontend is less interactive |

---

## 1.3 What Is A Single Page Application?

### Simple Explanation

A Single Page Application, or SPA, loads one main HTML page and then changes the visible screen using JavaScript.

The browser does not reload the whole website for every page.

Angular changes the component shown inside:

```html
<router-outlet></router-outlet>
```

### Easy Scenario

Suppose the user moves from:

```text
/login
```

to:

```text
/user-dashboard
```

In a traditional website:

```text
The browser asks the server for a completely new HTML page.
```

In an Angular SPA:

```text
Angular sees the URL change, finds the matching route, and swaps the component on the screen.
```

### What SPA Means In This Project

This project has one Angular app, but many screens:

```text
/
/login
/register
/user-dashboard
/interviews/start
/interviews/:id/session
/assessments/:id/session
/premium
/profile
/admin
/admin/users
```

These are handled by:

```text
src/app/app.routes.ts
```

And displayed through:

```text
src/app/app.component.html
```

which contains:

```html
<router-outlet></router-outlet>
```

### Why We Use SPA Style

SPA style is useful because:

- Navigation feels faster.
- UI can keep state between screens.
- Angular can manage complex flows smoothly.
- API calls can happen in the background.
- Components can be reused across pages.

### What If We Do Not Use SPA?

If this was not an SPA:

- Each page might be rendered by the backend.
- Every navigation could reload the full page.
- The frontend might feel slower.
- Sharing state between pages would be harder.
- Some interactive flows like interview sessions may feel less smooth.

### Alternative Ways

Alternatives to SPA:

- Multi-page app with server-rendered pages.
- Hybrid app with server-rendering plus JavaScript.
- Angular Universal/server-side rendering.
- Static site plus API calls.

---

## 1.4 How Browser, Angular, API Gateway, And Backend Services Work Together

### Simple Explanation

Your project is not only frontend. It is a full system.

The usual flow is:

```text
Browser -> Angular frontend -> API Gateway -> Backend service -> Database/AI/payment/etc.
```

Then the response comes back:

```text
Backend service -> API Gateway -> Angular frontend -> Browser screen
```

### Easy Scenario: Login

1. User types email and password in Angular login page.
2. Login component calls `AuthService.login`.
3. `AuthService` sends HTTP POST request.
4. API Gateway receives the request.
5. Identity backend checks the credentials.
6. Backend returns a token and user information.
7. Angular stores the token.
8. Angular navigates to user dashboard.

### What This Project Uses

Frontend API base URL:

```text
src/environments/environment.ts
```

Current value:

```ts
apiUrl: 'http://localhost:5190/api'
```

That means frontend services call backend through:

```text
http://localhost:5190/api
```

Example:

```text
AuthService uses /auth
InterviewService uses interview-related endpoints
AssessmentService uses assessment-related endpoints
SubscriptionService uses subscription endpoints
AdminService uses admin endpoints
```

### Why Use API Gateway?

An API Gateway gives one main door to the backend.

Instead of frontend needing to know many backend service URLs, it can call one base URL.

Example:

```text
Frontend calls:
http://localhost:5190/api/auth/login
```

The gateway can forward it to the correct backend service.

### What If We Do Not Use API Gateway?

Without an API Gateway:

- Frontend may need separate URLs for identity, interview, assessment, subscription, and admin services.
- Deployment config becomes harder.
- Cross-origin problems may increase.
- Service discovery becomes more complicated.
- Frontend becomes more tightly connected to backend structure.

### Alternative Ways

Alternatives:

- Frontend calls each backend service directly.
- Backend-for-frontend pattern.
- Reverse proxy using Nginx.
- Monolithic backend with one API.
- GraphQL gateway.

---

## 1.5 Project Scenario: Candidate Uses The Website

### Story

A candidate opens the website to practice interviews and assessments.

### Step By Step

1. Candidate opens `/`.
2. Angular shows `DashboardComponent`.
3. Candidate clicks register.
4. Angular shows `RegisterComponent`.
5. Candidate submits registration form.
6. `AuthService.register` calls backend.
7. Backend returns JWT token.
8. Angular stores the token in `localStorage`.
9. Candidate is redirected to `/user-dashboard`.
10. `authGuard` allows access because token exists.
11. Candidate starts an interview from `/interviews/start`.
12. Frontend calls interview backend.
13. Candidate answers questions in `/interviews/:id/session`.
14. Frontend submits answers.
15. Candidate sees result in `/interviews/:id/result`.
16. Candidate can see old attempts in `/interviews/history`.

### Frontend Parts Involved

```text
DashboardComponent
RegisterComponent
AuthService
authGuard
UserDashboardComponent
InterviewStartComponent
InterviewSessionComponent
InterviewResultComponent
InterviewHistoryComponent
InterviewService
Interview models
```

### Why This Flow Is Good

This flow is good because each part has a clear job:

- Component shows UI.
- Service talks to backend.
- Guard protects route.
- Model defines data shape.
- Router moves between screens.

### What If Everything Was In One File?

If all logic was written in one giant file:

- Login code, interview code, admin code, and UI code would mix together.
- Debugging would become painful.
- Reusing logic would be hard.
- New features would create more confusion.

---

## 1.6 Project Scenario: Admin Uses The Website

### Story

An admin opens the website to manage users, interviews, assessments, and questions.

### Step By Step

1. Admin logs in.
2. Angular stores the JWT token.
3. Admin opens `/admin`.
4. `adminGuard` checks whether the logged-in user has admin role.
5. If admin is valid, Angular shows `AdminDashboardComponent`.
6. Admin opens `/admin/users`.
7. Angular shows `AdminUsersComponent`.
8. Admin can update roles, premium status, or active status.
9. Admin opens `/admin/questions`.
10. Angular shows `AdminQuestionsComponent`.
11. Admin can create, edit, or delete MCQ questions.

### Frontend Parts Involved

```text
AdminDashboardComponent
AdminUsersComponent
AdminInterviewsComponent
AdminAssessmentsComponent
AdminQuestionsComponent
AdminNavbarComponent
AdminService
Admin models
adminGuard
authInterceptor
```

### Why Admin Area Is Separate

Admin pages are separate because admin users have different responsibilities.

Candidate pages are for practice.

Admin pages are for management.

Keeping them separate makes the app easier to understand and safer to control.

### What If Admin And User Pages Were Mixed?

If admin and user pages were mixed:

- Candidate UI would become confusing.
- Admin-only controls might appear in the wrong places.
- Role checks would become harder to maintain.
- Security mistakes would be easier to make.

### Alternative Ways

Admin alternatives:

- Separate Angular app only for admin.
- Lazy-loaded admin routes.
- Role-based components inside the same pages.
- Backend-rendered admin panel.

---

## 1.7 Full Frontend Request Flow From Button Click To Backend Response

### Simple Flow

Most frontend actions follow this pattern:

```text
User action -> Component method -> Service method -> HTTP request -> Backend -> Response -> Component state -> Template updates
```

### Example: Login Button

The user clicks login.

Angular flow:

```text
login.component.html
    -> calls method in login.component.ts
        -> calls AuthService.login()
            -> HttpClient sends request
                -> auth.interceptor may attach token if available
                    -> backend responds
                        -> AuthService maps response
                            -> component navigates or shows error
```

### Example: Open Protected Dashboard

The user opens:

```text
/user-dashboard
```

Angular flow:

```text
Router checks app.routes.ts
    -> sees UserDashboardComponent
        -> sees canActivate: [authGuard]
            -> authGuard checks token/current user
                -> if valid, page opens
                -> if invalid, user goes to /login
```

### Example: Start Assessment

The user configures assessment and clicks start.

Angular flow:

```text
assessment-start.component.html
    -> startAssessment()
        -> AssessmentService.startAssessment()
            -> backend creates assessment
                -> response is saved for session
                    -> router navigates to /assessments/:id/session
```

### Why This Flow Matters

When you understand this flow, debugging becomes much easier.

If something breaks, you can ask:

- Did the button call the right method?
- Did the method call the right service?
- Did the service call the right API?
- Did the interceptor attach token?
- Did backend return success or error?
- Did the component update its state?
- Did the template display that state?

---

## 1.8 What Happens When The App Loads In The Browser

### Step By Step

1. Browser opens the Angular app.
2. Browser loads `index.html`.
3. Angular runs `src/main.ts`.
4. `main.ts` calls `bootstrapApplication`.
5. Angular starts the root `App` component.
6. Angular uses `appConfig`.
7. `appConfig` registers router and HTTP client.
8. Root template shows `<router-outlet>`.
9. Router checks current URL.
10. Router loads the matching component.

### Project Files Involved

```text
src/index.html
src/main.ts
src/app/app.config.ts
src/app/app.component.ts
src/app/app.component.html
src/app/app.routes.ts
```

### Easy Visual

```text
index.html
    -> main.ts
        -> app.config.ts
            -> app.component.ts
                -> app.component.html
                    -> router-outlet
                        -> route component
```

### What If Startup Fails?

If startup fails:

- The page may stay blank.
- Console may show Angular errors.
- Routes may not work.
- HTTP services may not work.
- Components may not render.

Common reasons:

- Wrong imports.
- Broken component.
- Missing provider.
- Syntax error.
- Routing error.

---

## 1.9 What Happens When The User Refreshes The Page

### Simple Explanation

Refresh means the browser reloads the app from the beginning.

Angular starts again from:

```text
index.html -> main.ts -> App component -> router
```

### Important Point

Component memory is lost on refresh.

But data saved in browser storage can remain.

This project uses:

```text
localStorage
sessionStorage
```

### Login Refresh Scenario

If the user is logged in and refreshes `/user-dashboard`:

1. Angular starts again.
2. Router sees `/user-dashboard`.
3. Route has `authGuard`.
4. `authGuard` checks token in `localStorage`.
5. If token exists but user is not loaded yet, guard calls `getMe()`.
6. Backend confirms user.
7. Dashboard opens.

### Why This Is Needed

Without this behavior:

- Logged-in users may be kicked out after refresh.
- User role may be unknown.
- Admin pages may fail.
- Premium status may be wrong.

### What If We Store Nothing?

If token is not stored anywhere:

- Refresh would forget login.
- User would need to login again.
- Protected routes would redirect to login.

### Alternative Ways

Alternatives:

- Store token in HTTP-only cookie.
- Use refresh tokens.
- Use server session.
- Use OAuth/OIDC provider.
- Use state management plus backend session check.

---

## 1.10 What Happens When The User Logs Out

### Simple Explanation

Logout means the frontend forgets the current user and token.

The project logout behavior is in:

```text
src/app/services/auth.service.ts
```

### Current Logout Logic

The service removes the token:

```ts
localStorage.removeItem('token');
```

It clears session storage:

```ts
sessionStorage.clear();
```

It updates current user state:

```ts
this.currentUserSubject.next(null);
```

### Scenario

1. User clicks logout in navbar.
2. Navbar calls `AuthService.logout()`.
3. Token is removed.
4. Session data is cleared.
5. Current user becomes `null`.
6. Router sends user to `/login`.
7. Protected pages become unavailable.

### Why Logout Matters

Logout is important because:

- It protects user account data.
- It prevents another person using the same browser from accessing protected pages.
- It clears session-specific interview or assessment data.
- It resets frontend auth state.

### What If Logout Only Navigates To Login?

If logout only moves the user to `/login` but does not remove token:

- The user may still be technically logged in.
- Protected pages may still open.
- API requests may still include old token.
- The UI can become inconsistent.

### Alternatives

Logout alternatives:

- Also call backend logout endpoint.
- Revoke refresh token.
- Clear HTTP-only cookie.
- Log out from identity provider.
- Clear selected app caches only instead of all session storage.

---

## Topic 1 Summary

Your Angular frontend is the visible website of the project.

It is responsible for:

- Showing pages.
- Handling clicks and forms.
- Navigating between screens.
- Calling backend APIs.
- Saving login state.
- Protecting user and admin routes.
- Displaying interview, assessment, subscription, profile, notification, and admin data.

The backend is still responsible for:

- Real authentication.
- Real authorization.
- Database operations.
- Business rules.
- Payment/subscription processing.
- Interview and assessment evaluation.

The most important mental model:

```text
Component shows UI.
Service talks to backend.
Router changes page.
Guard protects page.
Interceptor attaches token.
Model describes data.
Template displays state.
CSS makes it look right.
```

---

## Mini Exercise

Open this file:

```text
src/app/app.routes.ts
```

Try to answer:

1. Which routes can anyone open?
2. Which routes need login?
3. Which routes need admin role?
4. Which routes use `:id`?
5. What page opens when the URL does not match any route?

---

## Interview-Style Questions

1. What is the role of the frontend in a full-stack application?
2. Why is Angular useful for this project?
3. What is a Single Page Application?
4. What does `router-outlet` do?
5. Why do we use services instead of writing API calls directly inside every component?
6. Why do protected routes need guards?
7. Why is frontend route protection not enough for real security?
8. What does an HTTP interceptor do?
9. What happens when a user refreshes a protected route?
10. What should happen during logout?

---

## Next Topic

Recommended next lesson:

```text
Topic 2: Project Setup And Tooling
```

Files to study next:

```text
package.json
angular.json
tsconfig.json
src/environments/environment.ts
```
