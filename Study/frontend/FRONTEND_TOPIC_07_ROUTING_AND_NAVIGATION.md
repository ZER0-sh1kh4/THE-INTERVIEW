# Topic 7: Routing And Navigation

This lesson explains Angular routing and navigation using your actual project files.

Project frontend path:

```text
Frontend/
```

Main idea:

```text
Routing decides which component should appear for which URL. Navigation is how the user moves from one route to another.
```

In your project:

```text
/login shows LoginComponent
/user-dashboard shows UserDashboardComponent
/interviews/:id/session shows InterviewSessionComponent
/admin/users shows AdminUsersComponent
```

---

## 7.1 What Is Routing?

### Simple Explanation

Routing is the website's map.

It tells Angular:

```text
If the browser URL is this path, show this component.
```

Example:

```ts
{ path: 'login', component: LoginComponent }
```

Meaning:

```text
When user opens /login, show LoginComponent.
```

### Easy Scenario

Think of a shopping mall.

- The mall has many shops.
- A map tells you where each shop is.
- The route file is Angular's mall map.

In your app:

- `/login` is the login shop.
- `/premium` is the subscription shop.
- `/interviews/start` is the interview setup shop.
- `/admin/users` is the admin users shop.

### What Routing Is Doing In This Project

Routing allows this Angular app to feel like many pages while still being one frontend app.

The main route file is:

```text
src/app/app.routes.ts
```

The root template contains:

```html
<router-outlet></router-outlet>
```

That is where Angular displays the currently matched route component.

### Why We Use Routing

Routing helps because:

- Each page has its own URL.
- Users can bookmark pages.
- Browser back/forward buttons work.
- Protected pages can use guards.
- Features stay separated by route.
- Components load based on app state and URL.

### What If We Do Not Use Routing?

Without routing:

- You would manually show/hide components.
- Browser URLs would not represent screens.
- Refreshing a page would be harder.
- User could not directly open `/profile` or `/admin/users`.
- Guards would not protect navigation.

### Alternatives

Alternatives:

- Manual component switching.
- Server-rendered pages.
- Multiple separate HTML pages.
- Another framework router like React Router or Vue Router.

Angular Router is the standard routing solution for Angular apps.

---

## 7.2 Where Routing Is Registered

Routing is defined in:

```text
src/app/app.routes.ts
```

but it is registered in:

```text
src/app/app.config.ts
```

### App Config Code

```ts
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes)
  ],
};
```

### Simple Explanation

`app.routes.ts` creates the map.

`provideRouter(routes)` gives that map to Angular.

### What If `provideRouter(routes)` Is Missing?

Angular will not know about your route list.

Problems:

- `router-outlet` cannot display route components.
- `routerLink` may fail.
- `router.navigate(...)` may fail.
- URLs like `/login` and `/admin` will not work correctly.

---

## 7.3 `router-outlet`

File:

```text
src/app/app.component.html
```

Code:

```html
<router-outlet></router-outlet>
```

### Simple Explanation

`router-outlet` is a placeholder.

It says:

```text
Angular router, place the current page component here.
```

### Easy Scenario

If URL is:

```text
/login
```

then `<router-outlet>` displays:

```text
LoginComponent
```

If URL is:

```text
/admin/users
```

then `<router-outlet>` displays:

```text
AdminUsersComponent
```

### What If `router-outlet` Is Missing?

Angular can match a route, but it has nowhere to put the component.

Result:

- Blank page.
- Route may change but UI does not show.

---

## 7.4 `src/app/app.routes.ts`

File:

```text
src/app/app.routes.ts
```

This file imports components and guards, then exports route definitions.

### First Import

```ts
import { Routes } from '@angular/router';
```

`Routes` is the Angular type for an array of route objects.

### Component Imports

Example:

```ts
import { LoginComponent } from './components/login/login.component';
import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';
```

These imports let route objects point to real components.

### Guard Imports

```ts
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';
```

These protect selected routes.

### Route Array

```ts
export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'login', component: LoginComponent },
  ...
];
```

This exports the app's route map.

---

## 7.5 Route Object Anatomy

### Simple Route

```ts
{ path: 'login', component: LoginComponent }
```

### Meaning

| Part | Meaning |
| --- | --- |
| `path: 'login'` | URL path after domain |
| `component: LoginComponent` | Component to show |

Full URL:

```text
http://localhost:4200/login
```

### Protected Route

```ts
{ path: 'user-dashboard', component: UserDashboardComponent, canActivate: [authGuard] }
```

### Meaning

| Part | Meaning |
| --- | --- |
| `path` | URL path |
| `component` | Component to show if allowed |
| `canActivate` | Guard checks before opening |

Before Angular shows `UserDashboardComponent`, it runs:

```text
authGuard
```

### Dynamic Route

```ts
{ path: 'interviews/:id/session', component: InterviewSessionComponent, canActivate: [authGuard] }
```

### Meaning

`:id` is a dynamic value.

This route can match:

```text
/interviews/1/session
/interviews/45/session
/interviews/900/session
```

---

## 7.6 Public Routes

Public routes are routes that anyone can open.

Your public routes:

```ts
{ path: '', component: DashboardComponent },
{ path: 'login', component: LoginComponent },
{ path: 'register', component: RegisterComponent },
{ path: 'reset-password', component: ResetPasswordComponent },
```

### Table

| URL | Component | Purpose |
| --- | --- | --- |
| `/` | `DashboardComponent` | Public landing page |
| `/login` | `LoginComponent` | User/admin login |
| `/register` | `RegisterComponent` | Candidate registration |
| `/reset-password` | `ResetPasswordComponent` | Password reset flow |

### Why These Are Public

Users need to access these before logging in.

### What If Login Route Was Guarded?

Unauthenticated users could not open login.

That would trap users outside the app.

### What If Register Route Was Guarded?

New users could not create accounts.

---

## 7.7 Protected User Routes

Protected user routes use:

```ts
canActivate: [authGuard]
```

### Your Protected User Routes

```ts
{ path: 'user-dashboard', component: UserDashboardComponent, canActivate: [authGuard] },
{ path: 'interviews/start', component: InterviewStartComponent, canActivate: [authGuard] },
{ path: 'interviews/history', component: InterviewHistoryComponent, canActivate: [authGuard] },
{ path: 'interviews/:id/preflight', component: InterviewPreflightComponent, canActivate: [authGuard] },
{ path: 'interviews/:id/session', component: InterviewSessionComponent, canActivate: [authGuard] },
{ path: 'interviews/:id/result', component: InterviewResultComponent, canActivate: [authGuard] },
{ path: 'assessments/domain', component: AssessmentDomainComponent, canActivate: [authGuard] },
{ path: 'assessments/history', component: AssessmentHistoryComponent, canActivate: [authGuard] },
{ path: 'assessments/start', component: AssessmentStartComponent, canActivate: [authGuard] },
{ path: 'assessments/:id/session', component: AssessmentSessionComponent, canActivate: [authGuard] },
{ path: 'assessments/:id/result', component: AssessmentResultComponent, canActivate: [authGuard] },
{ path: 'premium', component: PremiumComponent, canActivate: [authGuard] },
{ path: 'subscription/success', component: SubscriptionSuccessComponent, canActivate: [authGuard] },
{ path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
```

### Why These Need Login

These pages contain user-specific data:

- User dashboard.
- Interview sessions.
- Interview results.
- Assessment sessions.
- Assessment results.
- Payment/subscription data.
- Profile.

### Scenario: User Opens `/profile` Without Login

Flow:

```text
User opens /profile
    -> router matches ProfileComponent route
        -> route has authGuard
            -> authGuard checks token
                -> no token
                    -> redirect to /login
```

### What If These Routes Were Public?

Users might open UI screens without login.

Important:

```text
Backend must still protect real data.
Frontend guards protect user flow and UI access, but backend authorization is the true security layer.
```

---

## 7.8 Admin Routes

Admin routes use:

```ts
canActivate: [adminGuard]
```

### Your Admin Routes

```ts
{ path: 'admin', component: AdminDashboardComponent, canActivate: [adminGuard] },
{ path: 'admin/users', component: AdminUsersComponent, canActivate: [adminGuard] },
{ path: 'admin/interviews', component: AdminInterviewsComponent, canActivate: [adminGuard] },
{ path: 'admin/assessments', component: AdminAssessmentsComponent, canActivate: [adminGuard] },
{ path: 'admin/questions', component: AdminQuestionsComponent, canActivate: [adminGuard] },
```

### Table

| URL | Component | Purpose |
| --- | --- | --- |
| `/admin` | `AdminDashboardComponent` | Admin overview |
| `/admin/users` | `AdminUsersComponent` | Manage users |
| `/admin/interviews` | `AdminInterviewsComponent` | View/manage interviews |
| `/admin/assessments` | `AdminAssessmentsComponent` | View/manage assessments |
| `/admin/questions` | `AdminQuestionsComponent` | Manage question bank |

### Scenario: Candidate Opens `/admin/users`

Flow:

```text
Candidate opens /admin/users
    -> router matches AdminUsersComponent route
        -> route has adminGuard
            -> adminGuard checks token and role
                -> role is not admin
                    -> redirect to /user-dashboard
```

### Why Admin Routes Are Separate

Admin area has different responsibilities:

- User management.
- Question management.
- Interview/assessment oversight.
- Subscription/payment visibility.

Keeping routes separate makes role control clearer.

### What If Admin Routes Used Only `authGuard`?

Any logged-in user might open admin UI pages.

Even if backend blocks API data, the frontend experience would still expose admin screens.

`adminGuard` improves frontend route control.

---

## 7.9 Wildcard Route

Your route file ends with:

```ts
{ path: '**', redirectTo: '' }
```

### Simple Explanation

`**` means:

```text
Match anything that did not match earlier.
```

### Easy Scenario

User opens:

```text
/random-page
```

No route matches.

Then wildcard catches it and redirects to:

```text
/
```

### Why Wildcard Route Is Last

Route order matters.

If wildcard came first, it would catch every URL before real routes could match.

Bad:

```ts
{ path: '**', redirectTo: '' },
{ path: 'login', component: LoginComponent }
```

Then `/login` may never reach `LoginComponent`.

### Alternative

Instead of redirecting to home, you could create a 404 page:

```ts
{ path: '**', component: NotFoundComponent }
```

Your project currently redirects unknown routes to the landing page.

---

## 7.10 Route Parameters: `:id`

### Simple Explanation

Route parameters let one route handle many IDs.

Example:

```ts
{ path: 'interviews/:id/session', component: InterviewSessionComponent }
```

`:id` is a placeholder.

### Real URLs

This one route can match:

```text
/interviews/1/session
/interviews/25/session
/interviews/301/session
```

### Your Dynamic Interview Routes

```ts
{ path: 'interviews/:id/preflight', component: InterviewPreflightComponent, canActivate: [authGuard] },
{ path: 'interviews/:id/session', component: InterviewSessionComponent, canActivate: [authGuard] },
{ path: 'interviews/:id/result', component: InterviewResultComponent, canActivate: [authGuard] },
```

### Your Dynamic Assessment Routes

```ts
{ path: 'assessments/:id/session', component: AssessmentSessionComponent, canActivate: [authGuard] },
{ path: 'assessments/:id/result', component: AssessmentResultComponent, canActivate: [authGuard] },
```

### Why Route Params Are Useful

Interview and assessment sessions are specific records.

The frontend needs to know:

```text
Which interview?
Which assessment?
```

The ID in the URL answers that.

### Programmatic Navigation With Params

Interview start:

```ts
this.router.navigate(['/interviews', interview.id, 'preflight']);
```

If `interview.id` is `42`, Angular creates:

```text
/interviews/42/preflight
```

Assessment start:

```ts
this.router.navigate(['/assessments', response.assessmentId, 'session']);
```

If `response.assessmentId` is `8`, Angular creates:

```text
/assessments/8/session
```

### Reading Params

Components that need route IDs import:

```ts
ActivatedRoute
```

Examples from your project:

```text
interview-preflight.component.ts
interview-session.component.ts
interview-result.component.ts
assessment-session.component.ts
assessment-result.component.ts
```

They use route params to know which backend record to load.

### What If IDs Were Not In The URL?

The app would need another way to remember the selected interview/assessment:

- Service state.
- Session storage.
- Query params.
- Backend current-session lookup.

URL params are better here because they make pages directly addressable.

---

## 7.11 Query Parameters

### Simple Explanation

Query parameters are extra information after `?` in the URL.

Example:

```text
/assessments/start?domain=Python
```

Here:

```text
domain = Python
```

### Project Example: Assessment Domain Selection

File:

```text
src/app/components/assessments/assessment-domain/assessment-domain.component.ts
```

Code:

```ts
selectDomain(domain: string) {
  this.router.navigate(['/assessments/start'], { queryParams: { domain } });
}
```

### What It Does

If user selects `Python`, Angular navigates to:

```text
/assessments/start?domain=Python
```

Then `AssessmentStartComponent` can read the query param and pre-fill the selected domain.

### Project Example: Premium Reason

Interview start redirects to premium:

```ts
this.router.navigate(['/premium'], {
  queryParams: { reason: 'interview-limit' },
  state: { upgradeMessage: message }
});
```

This creates:

```text
/premium?reason=interview-limit
```

### Why Query Params Are Useful

Query params are good for extra context:

- Selected domain.
- Upgrade reason.
- Checkout session id.
- Filter/search values.

### Route Params vs Query Params

| Type | Example | Good For |
| --- | --- | --- |
| Route param | `/interviews/42/result` | Required identity of page/resource |
| Query param | `/premium?reason=interview-limit` | Extra optional context |

### What If We Use Query Params For Everything?

URLs can become messy.

Required page identity usually belongs in route params.

Optional context usually belongs in query params.

---

## 7.12 Template Navigation With `routerLink`

### Simple Explanation

`routerLink` navigates without full browser reload.

### Project Example: Dashboard Buttons

File:

```text
src/app/components/user-dashboard/user-dashboard.component.html
```

Code:

```html
<button type="button" class="primary-action label-caps" routerLink="/interviews/start">
  Start Interview
</button>
```

Meaning:

```text
Clicking button navigates to /interviews/start.
```

### Project Example: User Navbar

File:

```text
src/app/components/shared/user-navbar/user-navbar.component.html
```

Code:

```html
<a routerLink="/user-dashboard" routerLinkActive="active">Dashboard</a>
<a routerLink="/interviews/history" routerLinkActive="active">History</a>
<a routerLink="/assessments/domain" routerLinkActive="active">Assessments</a>
<a routerLink="/interviews/start" routerLinkActive="active">Interview</a>
<a routerLink="/premium" routerLinkActive="active">Subscription</a>
<a routerLink="/profile" routerLinkActive="active">Profile</a>
```

Meaning:

```text
These are internal Angular navigation links.
```

### Project Example: Admin Navbar

File:

```text
src/app/admin/components/admin-navbar/admin-navbar.component.html
```

Code:

```html
<a routerLink="/admin" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Dashboard</a>
<a routerLink="/admin/users" routerLinkActive="active">Users</a>
<a routerLink="/admin/interviews" routerLinkActive="active">Interviews</a>
<a routerLink="/admin/assessments" routerLinkActive="active">Assessments</a>
<a routerLink="/admin/questions" routerLinkActive="active">Question Bank</a>
```

### What Is `routerLinkActive`?

`routerLinkActive="active"` adds the `active` CSS class when that link matches the current route.

CSS can then highlight the active page.

### What Is `routerLinkActiveOptions`?

```html
[routerLinkActiveOptions]="{exact: true}"
```

Means:

```text
Only mark /admin active when URL is exactly /admin.
```

Without exact matching, `/admin/users` might also make `/admin` look active.

### What If We Use `href` Instead?

```html
<a href="/login">Login</a>
```

can trigger a full page reload.

Angular internal navigation should use:

```html
<a routerLink="/login">Login</a>
```

### What Is Required For `routerLink`?

The component must import:

```ts
RouterModule
```

---

## 7.13 Dynamic Template Links

### Simple Explanation

Sometimes the route is stored in component data.

Then you use property binding:

```html
[routerLink]="row.link"
```

### Project Example: User Dashboard Recent Interviews

File:

```text
src/app/components/user-dashboard/user-dashboard.component.html
```

Code:

```html
<a *ngIf="getStatusLink(row); else plainDomain" [routerLink]="row.link" class="row-link">{{ row.domain }}</a>
```

Meaning:

```text
If the row has a valid action link, make domain clickable using row.link.
```

### Project Example: History Pages

Your project has dynamic action links like:

```html
<a class="action-link label-caps" [routerLink]="row.actionLink">{{ row.actionLabel }}</a>
```

Meaning:

```text
Use each row's own action link.
```

### Why Dynamic Links Are Useful

Tables and history pages contain different records.

One row may link to:

```text
/interviews/12/result
```

Another row may link to:

```text
/interviews/17/session
```

Dynamic links avoid hardcoding each row.

---

## 7.14 Programmatic Navigation With `Router`

### Simple Explanation

Sometimes navigation happens after logic, not directly from clicking a link.

Then the TypeScript class uses:

```ts
Router
```

### Example: Login Success

File:

```text
src/app/components/login/login.component.ts
```

Code:

```ts
if (user.role === 'Admin') {
  this.router.navigate(['/admin']);
} else {
  this.router.navigate(['/user-dashboard']);
}
```

Meaning:

```text
After login, send admins to admin dashboard and normal users to user dashboard.
```

### Example: Interview Created

```ts
this.router.navigate(['/interviews', interview.id, 'preflight']);
```

Meaning:

```text
After backend creates interview, open that interview's preflight page.
```

### Example: Assessment Created

```ts
this.router.navigate(['/assessments', response.assessmentId, 'session']);
```

Meaning:

```text
After backend creates assessment, open assessment session page.
```

### Example: Logout

File:

```text
src/app/components/shared/user-navbar/user-navbar.component.ts
```

Code:

```ts
logout(): void {
  this.authService.logout();
  this.router.navigate(['/login']);
}
```

Meaning:

```text
Clear auth state and move user to login page.
```

### Example: Notification Action URL

```ts
this.router.navigateByUrl(note.actionUrl);
```

Meaning:

```text
Notification contains a URL string. Navigate directly to it.
```

### `navigate` vs `navigateByUrl`

| Method | Example | Best For |
| --- | --- | --- |
| `navigate` | `this.router.navigate(['/interviews', id, 'result'])` | Building route from array parts |
| `navigateByUrl` | `this.router.navigateByUrl('/profile')` | Navigating to a full URL string |

### What If We Navigate Before Backend Success?

User may land on a page before data exists.

Example:

```text
Go to /interviews/undefined/preflight
```

Your interview start code checks:

```ts
if (!interview.id) {
  this.errorMsg = 'Interview was created, but the session id could not be resolved. Please try again.';
  return;
}
```

That prevents bad navigation.

---

## 7.15 Guards In Routing

Guards decide whether a route can open.

Your project uses:

```text
src/app/guards/auth.guard.ts
src/app/guards/admin.guard.ts
```

### What Is `canActivate`?

`canActivate` means:

```text
Before opening this route, run this guard.
```

Example:

```ts
{ path: 'profile', component: ProfileComponent, canActivate: [authGuard] }
```

### Guard Return Values

A guard can return:

- `true`: allow route.
- `false`: block route.
- Observable of true/false.
- Promise of true/false.
- Redirect behavior by calling router.

Your guards return observables because user loading can require an API call.

---

## 7.16 `authGuard`

File:

```text
src/app/guards/auth.guard.ts
```

### Code

```ts
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  if (!token) {
    router.navigate(['/login']);
    return of(false);
  }

  const user = authService.currentUserValue;
  const userRequest$ = (user === undefined)
    ? authService.getMe().pipe(catchError(() => of(null)))
    : of(user);

  return userRequest$.pipe(
    map(u => {
      if (!u) {
        router.navigate(['/login']);
        return false;
      }
      return true;
    })
  );
};
```

### Step-By-Step

```ts
const authService = inject(AuthService);
const router = inject(Router);
```

Gets the auth service and router inside a functional guard.

```ts
const token = authService.getToken();
```

Reads token from storage through `AuthService`.

```ts
if (!token) {
  router.navigate(['/login']);
  return of(false);
}
```

If there is no token, redirect to login and block route.

```ts
const user = authService.currentUserValue;
```

Reads current user state.

```ts
const userRequest$ = (user === undefined)
  ? authService.getMe().pipe(catchError(() => of(null)))
  : of(user);
```

If token exists but user data is not loaded yet, call backend `getMe()`.

If user is already loaded, use existing user.

```ts
if (!u) {
  router.navigate(['/login']);
  return false;
}
return true;
```

If user is invalid, redirect to login.

Otherwise allow route.

### Scenario: Refresh `/user-dashboard`

1. Browser reloads Angular app.
2. Auth service current user may be `undefined`.
3. Token still exists in localStorage.
4. `authGuard` sees token.
5. Guard calls `getMe()`.
6. Backend confirms user.
7. Guard allows dashboard.

### Why This Guard Is Useful

It prevents unauthenticated users from opening protected user pages.

### What If `authGuard` Is Removed?

User could open UI screens like:

```text
/profile
/premium
/interviews/start
```

without being logged in.

Backend should still reject protected API calls, but frontend flow would be weaker.

---

## 7.17 `adminGuard`

File:

```text
src/app/guards/admin.guard.ts
```

### Code

```ts
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  if (!token) {
    router.navigate(['/login']);
    return of(false);
  }

  const user = authService.currentUserValue;
  const userRequest$ = (user === undefined)
    ? authService.getMe().pipe(catchError(() => of(null)))
    : of(user);

  return userRequest$.pipe(
    map(u => {
      if (!u || u.role?.toLowerCase() !== 'admin') {
        const target = u ? '/user-dashboard' : '/login';
        router.navigate([target]);
        return false;
      }
      return true;
    })
  );
};
```

### Step-By-Step

The start is similar to `authGuard`:

```text
Check token.
If no token, go to login.
If user is not loaded, call getMe().
```

Then it checks role:

```ts
if (!u || u.role?.toLowerCase() !== 'admin') {
```

Meaning:

```text
If there is no user, or user's role is not admin, block the route.
```

Redirect target:

```ts
const target = u ? '/user-dashboard' : '/login';
```

Meaning:

```text
If user exists but is not admin, send to user dashboard.
If user does not exist, send to login.
```

### Scenario: Admin Opens `/admin/questions`

1. Token exists.
2. User is loaded or fetched.
3. Role is admin.
4. Guard returns true.
5. `AdminQuestionsComponent` opens.

### Scenario: Candidate Opens `/admin/questions`

1. Token exists.
2. User is loaded or fetched.
3. Role is candidate/user.
4. Guard redirects to `/user-dashboard`.
5. Route is blocked.

### Why `adminGuard` Is Separate From `authGuard`

`authGuard` only checks if user is logged in.

`adminGuard` checks if user is logged in and has admin role.

They represent different access levels.

---

## 7.18 Navigation Flows In Your Project

### Flow 1: Public Landing To Login

Template:

```html
<button routerLink="/login">LOGIN</button>
```

Flow:

```text
User clicks Login
    -> Angular router goes to /login
        -> LoginComponent appears
```

### Flow 2: Login To Correct Dashboard

TypeScript:

```ts
if (user.role === 'Admin') {
  this.router.navigate(['/admin']);
} else {
  this.router.navigate(['/user-dashboard']);
}
```

Flow:

```text
Login succeeds
    -> user role loaded
        -> Admin goes to /admin
        -> Candidate goes to /user-dashboard
```

### Flow 3: Candidate Starts Interview

TypeScript:

```ts
this.router.navigate(['/interviews', interview.id, 'preflight']);
```

Flow:

```text
User submits interview setup
    -> backend creates interview
        -> frontend gets interview id
            -> navigate to /interviews/:id/preflight
```

### Flow 4: Preflight To Session

TypeScript:

```ts
this.router.navigate(['/interviews', this.interviewId, 'session']);
```

Flow:

```text
User passes preflight/checks
    -> open live interview session route
```

### Flow 5: Session To Result

TypeScript examples:

```ts
this.router.navigate(['/interviews', this.interviewId, 'result']);
```

or:

```ts
this.router.navigate(['/assessments', this.assessmentId, 'result']);
```

Flow:

```text
User submits session
    -> backend stores/evaluates answers
        -> frontend opens result page
```

### Flow 6: Assessment Domain To Assessment Start

TypeScript:

```ts
this.router.navigate(['/assessments/start'], { queryParams: { domain } });
```

Flow:

```text
User selects Python
    -> navigate to /assessments/start?domain=Python
        -> start page reads domain query param
```

### Flow 7: Limit Reached To Premium

TypeScript:

```ts
this.router.navigate(['/premium'], {
  queryParams: { reason: 'interview-limit' },
  state: { upgradeMessage: message }
});
```

Flow:

```text
Free user reaches limit
    -> frontend sends user to premium page
        -> query param explains why
        -> navigation state can carry message
```

### Flow 8: Notification To Action URL

TypeScript:

```ts
this.router.navigateByUrl(note.actionUrl);
```

Flow:

```text
User clicks notification
    -> notification has actionUrl
        -> router opens that URL
```

---

## 7.19 `ActivatedRoute`

### Simple Explanation

`ActivatedRoute` gives a component information about the current route.

It can read:

- Route params like `:id`.
- Query params like `?domain=Python`.
- Route data.

### Project Files Using `ActivatedRoute`

Examples:

```text
interview-preflight.component.ts
interview-session.component.ts
interview-result.component.ts
assessment-start.component.ts
assessment-session.component.ts
assessment-result.component.ts
premium.component.ts
subscription-success.component.ts
```

### Query Param Example

Assessment start listens to query params:

```ts
this.route.queryParams.subscribe(params => {
  if (params['domain']) {
    this.form.domain = params['domain'];
  }
});
```

Meaning:

```text
If URL has ?domain=Python, set form.domain to Python.
```

### Why `ActivatedRoute` Matters

Without it, components would not know:

- Which interview ID is in the URL.
- Which assessment ID is in the URL.
- Why user was sent to premium page.
- Which checkout session succeeded.
- Which assessment domain was selected.

---

## 7.20 Browser Refresh And Routing

### Scenario: Refresh `/interviews/42/session`

Flow:

```text
Browser reloads Angular app
    -> index.html loads
        -> Angular starts
            -> router reads /interviews/42/session
                -> authGuard runs
                    -> token exists
                        -> getMe() may run
                            -> InterviewSessionComponent opens
                                -> component reads id 42
```

### Why This Is Important

Users expect refresh to keep them on the same page.

Angular routes support that if:

- Dev server or production server serves Angular app for routes.
- Guards allow the route.
- Component can reload required data.

### Possible Problem In Production

If user refreshes:

```text
/profile
```

the server must return Angular's `index.html`.

If server instead searches for a real `/profile` file, it may return 404.

This is a deployment configuration issue for SPAs.

---

## 7.21 What This Topic Is Doing In Your Project

| Routing Feature | Project Usage |
| --- | --- |
| `app.routes.ts` | Main route map |
| `router-outlet` | Displays current route component |
| `provideRouter(routes)` | Registers router globally |
| Public routes | Landing, login, register, reset password |
| `authGuard` | Protects user features |
| `adminGuard` | Protects admin features |
| Route params | Interview/assessment IDs |
| Query params | Domain selection, premium reason, checkout session |
| `routerLink` | Template navigation |
| `routerLinkActive` | Active nav link styling |
| `router.navigate` | Logic-based navigation |
| `navigateByUrl` | Navigation from stored URL strings |
| Wildcard route | Redirect unknown paths to home |

---

## 7.22 What If We Do Not Use Routing Properly?

### Missing Route

User clicks link but page does not open.

### Wrong Component

URL opens the wrong screen.

### Missing Guard

Protected UI may be accessible without correct role.

### Wrong Guard

Normal user may be blocked from user page, or admin page may be exposed.

### Bad Dynamic Route

Route IDs may not match, causing pages like:

```text
/interviews/undefined/session
```

### Wildcard In Wrong Place

All routes may redirect home.

### Using `href` Instead Of `routerLink`

SPA navigation becomes full page reloads.

---

## 7.23 Alternatives And Improvements

### Lazy Loading

Large apps often lazy-load feature routes:

```text
admin routes load only when admin area opens.
interview routes load only when interview area opens.
```

Benefit:

```text
Smaller initial bundle.
```

### Child Routes

Admin routes could be grouped:

```text
/admin
  /users
  /interviews
  /assessments
  /questions
```

with a shared admin layout.

### Route Resolvers

Resolvers can load data before opening a route.

Example:

```text
Load interview result before showing result component.
```

### 404 Page

Instead of:

```ts
{ path: '**', redirectTo: '' }
```

you could use:

```ts
{ path: '**', component: NotFoundComponent }
```

### Permission-Based Route Data

Routes can store metadata like:

```ts
data: { roles: ['Admin'] }
```

Then a generic role guard can read it.

Your current separate `authGuard` and `adminGuard` are simpler for learning.

---

## 7.24 Common Routing Mistakes

### Mistake 1: Forgetting `RouterModule`

If a standalone component template uses:

```html
routerLink
```

the component must import:

```ts
RouterModule
```

### Mistake 2: Putting Wildcard Route Too Early

Wildcard must stay last.

### Mistake 3: Not Checking IDs Before Navigation

Bad:

```ts
this.router.navigate(['/interviews', interview.id, 'session']);
```

if `interview.id` might be missing.

Better:

```ts
if (!interview.id) {
  show error;
  return;
}
```

### Mistake 4: Using Query Params For Required IDs

Better:

```text
/interviews/42/result
```

than:

```text
/interview-result?id=42
```

for core resource identity.

### Mistake 5: Trusting Frontend Guards As Full Security

Frontend guards can be bypassed by technical users.

Backend APIs must always check authentication and authorization.

### Mistake 6: Using `href` For Internal Navigation

Use `routerLink` for internal Angular pages.

### Mistake 7: Forgetting To Handle Refresh

Protected pages should be able to reload user state from token.

Your guards handle this with:

```ts
authService.getMe()
```

when current user is undefined.

---

## 7.25 File-By-File Routing Study Checklist

When studying routing, open files in this order:

1. `src/app/app.config.ts`
2. `src/app/app.component.html`
3. `src/app/app.routes.ts`
4. `src/app/guards/auth.guard.ts`
5. `src/app/guards/admin.guard.ts`
6. `src/app/components/shared/user-navbar/user-navbar.component.html`
7. `src/app/admin/components/admin-navbar/admin-navbar.component.html`
8. Components that use `Router`
9. Components that use `ActivatedRoute`

For each route, ask:

1. What URL is this?
2. Which component opens?
3. Is it public, authenticated, or admin-only?
4. Does it have a route param?
5. Does it use query params?
6. Which links navigate to it?
7. Which TypeScript methods navigate to it?
8. What happens on refresh?
9. What happens if user is not logged in?
10. What happens if user is not admin?

---

## 7.26 Mini Exercise

Open:

```text
src/app/app.routes.ts
```

Answer:

1. Which routes are public?
2. Which routes use `authGuard`?
3. Which routes use `adminGuard`?
4. Which routes contain `:id`?
5. What happens for unknown routes?

Open:

```text
src/app/components/shared/user-navbar/user-navbar.component.html
```

Answer:

1. Which routes are shown in the user navbar?
2. What does `routerLinkActive="active"` do?
3. Which link appears only for admins?
4. Which link appears only for non-premium users?

Open:

```text
src/app/components/assessments/assessment-domain/assessment-domain.component.ts
```

Answer:

1. Which route opens after selecting a domain?
2. Which query param is sent?
3. Why is a query param useful here?

Open:

```text
src/app/guards/auth.guard.ts
```

Answer:

1. What happens if there is no token?
2. Why does the guard call `getMe()`?
3. What does it return when user is valid?

Open:

```text
src/app/guards/admin.guard.ts
```

Answer:

1. What role is required?
2. Where does a normal user get redirected?
3. Where does an invalid/no-user case get redirected?

---

## 7.27 Interview-Style Questions

1. What is Angular routing?
2. What is `router-outlet`?
3. What does `provideRouter(routes)` do?
4. What is the difference between `routerLink` and `router.navigate`?
5. What is `routerLinkActive`?
6. What is a route parameter?
7. What is a query parameter?
8. When should you use route params instead of query params?
9. What does `canActivate` do?
10. What is the purpose of `authGuard`?
11. What is the purpose of `adminGuard`?
12. Why should wildcard route be last?
13. Why should internal links use `routerLink` instead of `href`?
14. What is `ActivatedRoute` used for?
15. Why are frontend guards not enough for real security?

---

## Topic 7 Summary

Routing is the map of your Angular website.

Most important mental model:

```text
app.routes.ts defines the URL map.
provideRouter(routes) registers the map.
router-outlet displays the matched component.
routerLink navigates from templates.
router.navigate navigates from TypeScript.
ActivatedRoute reads route/query values.
authGuard protects logged-in pages.
adminGuard protects admin pages.
```

In your project:

```text
Public routes are for landing and auth.
User routes are protected by authGuard.
Admin routes are protected by adminGuard.
Interview and assessment session/result pages use :id route params.
Premium and assessment start flows use query params for extra context.
Wildcard route redirects unknown URLs home.
```

Once you understand routing, you can trace how a user moves through the whole frontend from landing page to login, dashboard, interview, assessment, premium, profile, and admin screens.

---

## Next Topic

Recommended next lesson:

```text
Topic 8: Guards And Access Control
```

Files to study next:

```text
src/app/guards/auth.guard.ts
src/app/guards/admin.guard.ts
src/app/services/auth.service.ts
src/app/app.routes.ts
src/app/interceptors/auth.interceptor.ts
```
