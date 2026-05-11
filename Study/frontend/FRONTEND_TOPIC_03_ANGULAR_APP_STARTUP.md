# Topic 3: Angular App Startup

This lesson explains how your Angular frontend starts when the browser opens the website.

Project frontend path:

```text
Frontend/
```

Main startup chain:

```text
src/index.html
    -> src/main.ts
        -> src/app/app.config.ts
            -> src/app/app.component.ts
                -> src/app/app.component.html
                    -> src/app/app.routes.ts
                        -> selected page component
```

Main idea:

```text
Angular startup is the process where the browser loads one HTML shell, Angular attaches itself to <app-root>, app-level features are registered, and the router decides which page component should appear.
```

---

## 3.1 Big Picture: What Happens When The Angular App Starts?

### Simple Explanation

When a user opens your frontend website, the browser does not directly open `login.component.html` or `dashboard.component.html`.

The browser first loads one main HTML file:

```text
src/index.html
```

Inside that file, there is a custom-looking tag:

```html
<app-root></app-root>
```

Angular finds this tag and replaces it with your running Angular app.

After that, Angular uses routing to decide which page should appear.

### Easy Scenario

User opens:

```text
http://localhost:4200/login
```

Startup flow:

1. Browser loads `index.html`.
2. Browser sees `<app-root></app-root>`.
3. Angular starts from `main.ts`.
4. Angular bootstraps the root `App` component.
5. Angular loads global app configuration from `app.config.ts`.
6. Router reads current URL: `/login`.
7. Router checks `app.routes.ts`.
8. Router finds:

```ts
{ path: 'login', component: LoginComponent }
```

9. Angular displays `LoginComponent` inside `<router-outlet>`.

### What Startup Is Doing In This Project

Startup connects these things:

- Browser HTML shell.
- Angular root component.
- App providers.
- Router.
- HTTP client.
- Auth interceptor.
- Route list.
- Page components.

### Why Startup Matters

If startup is broken, nothing else matters.

Login, dashboard, interview, assessment, premium, profile, and admin pages all depend on this startup chain working correctly.

### What If Startup Is Wrong?

If startup is wrong:

- Page can become blank.
- Routes may not work.
- HTTP requests may fail.
- Guards may not run.
- Interceptor may not attach JWT token.
- Components may never load.

---

## 3.2 `src/index.html`

File:

```text
src/index.html
```

### Your Project Code

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>The Interview - Candidate Excellence Program</title>
    <meta name="description" content="Architected AI-powered preparation for high-stakes professional communication. Engage with our proprietary neural engine simulating behavioral and technical panels." />
    <base href="/" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="icon" type="image/x-icon" href="favicon.ico" />
  </head>
  <body>
    <app-root></app-root>
  </body>
</html>
```

### Simple Explanation

`index.html` is the first HTML file loaded by the browser.

It is not a normal full page with all content written inside it. Instead, it is a shell that gives Angular a place to start.

### Line-By-Line Explanation

```html
<!doctype html>
```

Tells the browser this is a modern HTML document.

Without it, browser rendering can behave strangely.

```html
<html lang="en">
```

Starts the HTML document and says the page language is English.

This helps browsers, screen readers, and search engines.

```html
<head>
```

The head contains page metadata. It does not show visible page UI directly.

```html
<meta charset="utf-8" />
```

Tells the browser to use UTF-8 character encoding.

This helps display text correctly.

```html
<title>The Interview - Candidate Excellence Program</title>
```

This is the browser tab title.

In your project, the tab title is:

```text
The Interview - Candidate Excellence Program
```

```html
<meta name="description" content="..." />
```

This describes the website for search engines and previews.

It does not show directly inside the page.

```html
<base href="/" />
```

Very important for Angular routing.

It tells Angular that routes start from the root of the site.

Example:

```text
/login
/user-dashboard
/admin/users
```

Without correct `base href`, route links and refresh behavior can break.

```html
<meta name="viewport" content="width=device-width, initial-scale=1" />
```

Makes the site responsive on mobile devices.

It tells the browser:

```text
Use the device width as the page width.
Do not pretend the screen is a desktop-sized page.
```

```html
<link rel="icon" type="image/x-icon" href="favicon.ico" />
```

Sets the small browser tab icon.

```html
<body>
```

The body contains visible page content.

```html
<app-root></app-root>
```

This is the most important line in `index.html`.

Angular searches for this tag because your root component says:

```ts
selector: 'app-root'
```

Angular then mounts the whole application here.

### What This File Is Doing In Your Project

`index.html` gives Angular:

- Browser metadata.
- App title.
- Routing base.
- Mobile viewport setup.
- Favicon.
- The root mounting point.

### Why We Use `index.html`

The browser needs an HTML document first. Angular cannot start from nothing.

`index.html` is that first document.

### What If We Remove `<app-root>`?

Angular will start, but it will not find the place where the root component should appear.

Possible result:

- Blank page.
- Angular error in console.

### What If We Change `<app-root>` To Something Else?

If you change it to:

```html
<my-app></my-app>
```

but `app.component.ts` still has:

```ts
selector: 'app-root'
```

Angular will not match it.

To make that work, you would also need to change the component selector.

### Alternatives

In most Angular apps, `index.html` is required.

Alternatives in other frameworks:

- React also uses an HTML root element like `<div id="root"></div>`.
- Vue uses something like `<div id="app"></div>`.
- Server-rendered apps may generate full HTML on the backend.

---

## 3.3 `src/main.ts`

File:

```text
src/main.ts
```

### Your Project Code

```ts
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app.component';

bootstrapApplication(App, appConfig).catch((err) => console.error(err));
```

### Simple Explanation

`main.ts` is the TypeScript entry point of the Angular app.

If `index.html` is the door, `main.ts` is the switch that turns the Angular app on.

### Line-By-Line Explanation

```ts
import { bootstrapApplication } from '@angular/platform-browser';
```

This imports Angular's function for starting a standalone Angular application in the browser.

`bootstrapApplication` means:

```text
Start Angular and attach the root component to the page.
```

```ts
import { appConfig } from './app/app.config';
```

Imports the global app configuration.

This config contains app-wide providers like:

- Router.
- HTTP client.
- Auth interceptor.
- Browser error listeners.

```ts
import { App } from './app/app.component';
```

Imports the root component.

Your root component class is named:

```ts
App
```

This root component uses selector:

```ts
app-root
```

which matches:

```html
<app-root></app-root>
```

inside `index.html`.

```ts
bootstrapApplication(App, appConfig)
```

This starts Angular using:

- Root component: `App`
- Global config: `appConfig`

Angular now knows:

```text
Which component starts the app.
Which app-level services/providers should exist.
```

```ts
.catch((err) => console.error(err));
```

If Angular startup fails, this catches the error and prints it to the browser console.

### What This File Is Doing In Your Project

`main.ts` connects:

```text
Angular browser platform
    -> root App component
        -> appConfig providers
```

### Why We Use `main.ts`

Angular needs a clear TypeScript entry file.

Your `angular.json` points to it:

```json
"browser": "src/main.ts"
```

So when Angular builds or serves the app, it starts from `src/main.ts`.

### What If `main.ts` Is Missing?

The app cannot start because Angular does not know what to bootstrap.

### What If We Bootstrap The Wrong Component?

If you bootstrap another component instead of `App`, then that component becomes the root of the whole app.

That could break routing if the component does not contain `<router-outlet>`.

### Alternative: Old NgModule Startup

Older Angular apps often used:

```ts
platformBrowserDynamic().bootstrapModule(AppModule)
```

Your project uses the modern standalone style:

```ts
bootstrapApplication(App, appConfig)
```

This is cleaner because there is no root `AppModule`.

---

## 3.4 `src/app/app.config.ts`

File:

```text
src/app/app.config.ts
```

### Your Project Code

```ts
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(), 
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor]))
  ],
};
```

### Simple Explanation

`app.config.ts` is where app-wide Angular features are registered.

Think of it as the app's main service desk.

It says:

```text
This app needs routing.
This app needs HTTP.
This app needs the auth interceptor.
This app should listen to global browser errors.
```

### Line-By-Line Explanation

```ts
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
```

Imports:

- `ApplicationConfig`: the TypeScript type for the app configuration object.
- `provideBrowserGlobalErrorListeners`: Angular helper for listening to browser-level errors.

```ts
import { provideRouter } from '@angular/router';
```

Imports Angular router provider.

Without this, Angular routes will not work.

```ts
import { provideHttpClient, withInterceptors } from '@angular/common/http';
```

Imports HTTP setup functions.

- `provideHttpClient` enables Angular's `HttpClient`.
- `withInterceptors` registers interceptors for HTTP requests.

```ts
import { routes } from './app.routes';
```

Imports the route list.

This is the map of URLs to components.

```ts
import { authInterceptor } from './interceptors/auth.interceptor';
```

Imports your custom auth interceptor.

This interceptor adds JWT tokens to API calls and handles some `401` errors.

```ts
export const appConfig: ApplicationConfig = {
```

Creates and exports the app configuration object.

`main.ts` imports this object and passes it to:

```ts
bootstrapApplication(App, appConfig)
```

```ts
providers: [
```

Providers are app-level things Angular can inject or use globally.

```ts
provideBrowserGlobalErrorListeners(),
```

Registers global browser error handling support.

```ts
provideRouter(routes),
```

Registers Angular routing using your route array.

This makes URLs like `/login`, `/profile`, and `/admin/users` work.

```ts
provideHttpClient(withInterceptors([authInterceptor]))
```

Registers Angular's HTTP client and attaches your auth interceptor.

This is why services can inject `HttpClient`.

This is also why API requests can automatically get:

```text
Authorization: Bearer <token>
```

### What This File Is Doing In Your Project

It enables:

- Routing.
- HTTP API calls.
- Global HTTP auth behavior.
- Browser error handling.

### Why We Use `app.config.ts`

Modern standalone Angular apps do not need a root `AppModule`.

So app-level setup goes into:

```text
app.config.ts
```

### What If We Remove `provideRouter(routes)`?

Routing will break.

Problems:

- `<router-outlet>` cannot show route components.
- `routerLink` may fail.
- `router.navigate()` may fail.
- URLs like `/login` or `/admin` will not work properly.

### What If We Remove `provideHttpClient(...)`?

Services using `HttpClient` will fail.

Examples affected:

- `AuthService`
- `InterviewService`
- `AssessmentService`
- `SubscriptionService`
- `NotificationService`
- `AdminService`

### What If We Remove `authInterceptor`?

API calls can still happen, but protected API calls may fail because the JWT token will not be attached automatically.

You would need to manually add headers in every service call.

### Alternatives

Alternative app setup styles:

- Old Angular `AppModule`.
- Multiple environment-specific configs.
- Route-level providers.
- Feature-level providers.
- Multiple interceptors registered together.

---

## 3.5 `src/app/app.component.ts`

File:

```text
src/app/app.component.ts
```

### Your Project Code

```ts
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class App {
  protected readonly title = signal('frontend');
}
```

### Simple Explanation

This is the root Angular component.

Every Angular app needs a starting component. In this project, that starting component is:

```ts
App
```

Its job is very small:

```text
Provide a place where routed pages can appear.
```

### Line-By-Line Explanation

```ts
import { Component, signal } from '@angular/core';
```

Imports:

- `Component`: used to define an Angular component.
- `signal`: Angular reactive state primitive.

```ts
import { RouterOutlet } from '@angular/router';
```

Imports `RouterOutlet`.

This is needed because the component HTML uses:

```html
<router-outlet></router-outlet>
```

Since this is a standalone component, dependencies must be listed in `imports`.

```ts
@Component({
```

Starts the component decorator.

The decorator tells Angular how this class connects to HTML and CSS.

```ts
selector: 'app-root',
```

This means Angular can place this component wherever it sees:

```html
<app-root></app-root>
```

Your `index.html` has exactly that.

```ts
imports: [RouterOutlet],
```

This tells Angular that this component template is allowed to use `RouterOutlet`.

Without it, `<router-outlet>` would not be recognized.

```ts
templateUrl: './app.component.html',
```

This tells Angular where the HTML template for this component is.

```ts
styleUrl: './app.component.css',
```

This tells Angular where the CSS file for this component is.

```ts
export class App {
```

Defines the component class.

This class can hold data and methods for the template.

```ts
protected readonly title = signal('frontend');
```

Creates a signal named `title` with value:

```text
frontend
```

In your current root template, this title is not being displayed. It is probably default starter code from Angular.

### What This File Is Doing In Your Project

It connects:

```text
<app-root> from index.html
    -> App component
        -> app.component.html
            -> <router-outlet>
```

### Why Root Component Is Small

The root component is small because actual pages are handled by routing.

Instead of putting all UI directly in the root component, your project uses route components like:

- `DashboardComponent`
- `LoginComponent`
- `UserDashboardComponent`
- `InterviewSessionComponent`
- `AdminDashboardComponent`

This keeps the app organized.

### What If We Put Everything In App Component?

The app would become messy.

Problems:

- All pages in one class.
- Too many variables.
- Too many methods.
- Hard to maintain.
- Hard to test.
- Hard to control access per page.

### What If We Remove `RouterOutlet` From Imports?

Angular may complain that `router-outlet` is not known.

Because standalone components must import what their templates use.

### Alternative

Alternative root component designs:

- Root component contains a shared layout and navbar.
- Root component handles global loading spinner.
- Root component contains toast notifications.
- Root component contains only router outlet, like this project.

Your project chooses a clean root component and puts navbars inside feature pages.

---

## 3.6 `src/app/app.component.html`

File:

```text
src/app/app.component.html
```

### Your Project Code

```html
<router-outlet></router-outlet>
```

### Simple Explanation

`router-outlet` is a placeholder.

It means:

```text
Angular router, put the current page component here.
```

### Easy Scenario

If current URL is:

```text
/login
```

Angular places:

```text
LoginComponent
```

inside `<router-outlet>`.

If current URL is:

```text
/profile
```

Angular places:

```text
ProfileComponent
```

inside `<router-outlet>`.

If current URL is:

```text
/admin/users
```

Angular places:

```text
AdminUsersComponent
```

inside `<router-outlet>`.

### What This File Is Doing In Your Project

It allows the whole app to behave like many pages while still being one Angular application.

### Why We Use `router-outlet`

Without `router-outlet`, Angular can know the route, but it has no place to display the selected component.

### What If We Remove It?

The app may load, but pages will not appear.

You may see a blank screen because the router has no outlet.

### Alternatives

Alternatives:

- Multiple named outlets for advanced layouts.
- Manual conditional rendering, not recommended for full app routing.
- Server-rendered page navigation.

For this app, one main router outlet is the correct simple design.

---

## 3.7 `src/app/app.routes.ts`

File:

```text
src/app/app.routes.ts
```

### Simple Explanation

`app.routes.ts` is the map of the website.

It tells Angular:

```text
If the URL is this, show this component.
If this route is protected, run this guard first.
If no route matches, redirect home.
```

### First Import

```ts
import { Routes } from '@angular/router';
```

`Routes` is the Angular type for a route array.

It helps TypeScript check that the route structure is valid.

### Component Imports

Your file imports many components:

```ts
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
```

These imports let routes point to actual page components.

Example:

```ts
{ path: 'login', component: LoginComponent }
```

Angular can only use `LoginComponent` here because it was imported.

### Guard Imports

```ts
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';
```

These guards protect routes.

- `authGuard` checks if user is logged in.
- `adminGuard` checks if user is an admin.

### Route Array

```ts
export const routes: Routes = [
```

This exports the route list.

`app.config.ts` imports it:

```ts
import { routes } from './app.routes';
```

Then registers it:

```ts
provideRouter(routes)
```

### Public Routes

```ts
{ path: '', component: DashboardComponent },
{ path: 'login', component: LoginComponent },
{ path: 'register', component: RegisterComponent },
{ path: 'reset-password', component: ResetPasswordComponent },
```

These routes can be opened without login.

| URL | Component | Meaning |
| --- | --- | --- |
| `/` | `DashboardComponent` | Landing/home page |
| `/login` | `LoginComponent` | Login page |
| `/register` | `RegisterComponent` | Register page |
| `/reset-password` | `ResetPasswordComponent` | Password reset page |

### Protected User Routes

Example:

```ts
{ path: 'user-dashboard', component: UserDashboardComponent, canActivate: [authGuard] },
```

This means:

```text
When user opens /user-dashboard,
run authGuard first.
If authGuard allows, show UserDashboardComponent.
If authGuard rejects, redirect to login.
```

Protected user routes include:

| URL | Component |
| --- | --- |
| `/user-dashboard` | `UserDashboardComponent` |
| `/interviews/start` | `InterviewStartComponent` |
| `/interviews/history` | `InterviewHistoryComponent` |
| `/interviews/:id/preflight` | `InterviewPreflightComponent` |
| `/interviews/:id/session` | `InterviewSessionComponent` |
| `/interviews/:id/result` | `InterviewResultComponent` |
| `/assessments/domain` | `AssessmentDomainComponent` |
| `/assessments/history` | `AssessmentHistoryComponent` |
| `/assessments/start` | `AssessmentStartComponent` |
| `/assessments/:id/session` | `AssessmentSessionComponent` |
| `/assessments/:id/result` | `AssessmentResultComponent` |
| `/premium` | `PremiumComponent` |
| `/subscription/success` | `SubscriptionSuccessComponent` |
| `/profile` | `ProfileComponent` |

### Route Parameters

Routes like this contain `:id`:

```ts
{ path: 'interviews/:id/session', component: InterviewSessionComponent, canActivate: [authGuard] },
```

`:id` is a route parameter.

It means the real URL can be:

```text
/interviews/12/session
/interviews/45/session
/interviews/99/session
```

Angular treats `12`, `45`, or `99` as the interview id.

### Why Route Parameters Are Useful

Interview and assessment pages need to know which specific interview or assessment is being opened.

Instead of creating separate routes for every id, Angular uses one pattern:

```text
/interviews/:id/session
```

### Admin Routes

```ts
{ path: 'admin', component: AdminDashboardComponent, canActivate: [adminGuard] },
{ path: 'admin/users', component: AdminUsersComponent, canActivate: [adminGuard] },
{ path: 'admin/interviews', component: AdminInterviewsComponent, canActivate: [adminGuard] },
{ path: 'admin/assessments', component: AdminAssessmentsComponent, canActivate: [adminGuard] },
{ path: 'admin/questions', component: AdminQuestionsComponent, canActivate: [adminGuard] },
```

These routes use `adminGuard`.

That means candidate users should not be able to open them.

### Wildcard Route

```ts
{ path: '**', redirectTo: '' }
```

This catches unknown routes.

Example:

```text
/something-that-does-not-exist
```

Angular redirects to:

```text
/
```

### Why Routes Are Centralized

Centralized routes make it easy to see the app map in one place.

You can answer:

- Which pages exist?
- Which pages are public?
- Which pages require login?
- Which pages require admin?
- Which routes have ids?
- What happens for unknown URLs?

### What If Routes Are Missing?

If a route is missing:

- URL will not show the expected page.
- Wildcard route may redirect to home.
- Navigation buttons may appear broken.

### What If Guard Is Missing On Protected Route?

The frontend may allow users to open the page.

Important:

```text
Backend must still protect real data.
Frontend guards improve user flow, but backend authorization is the real security layer.
```

### Alternatives

Alternative routing designs:

- Lazy-loaded routes.
- Nested child routes.
- Route resolvers.
- Separate admin route file.
- Route-level providers.
- Module-based routing in older Angular apps.

Your project currently uses one top-level route array. That is simple and clear for learning.

---

## 3.8 Full Startup Scenario: User Opens Home Page

User opens:

```text
http://localhost:4200/
```

Flow:

```text
Browser loads index.html
    -> finds <app-root>
        -> main.ts bootstraps App
            -> app.config.ts registers router
                -> router reads current URL "/"
                    -> app.routes.ts matches path ""
                        -> DashboardComponent appears inside router-outlet
```

Result:

```text
Landing/home dashboard page is shown.
```

---

## 3.9 Full Startup Scenario: User Opens Login Page

User opens:

```text
http://localhost:4200/login
```

Flow:

```text
Browser loads index.html
    -> Angular starts App
        -> router reads "/login"
            -> route path "login" matches
                -> LoginComponent appears inside router-outlet
```

No guard runs because login is public.

---

## 3.10 Full Startup Scenario: User Opens Protected Dashboard

User opens:

```text
http://localhost:4200/user-dashboard
```

Flow:

```text
Browser loads index.html
    -> Angular starts App
        -> router reads "/user-dashboard"
            -> route requires authGuard
                -> authGuard checks token/user
                    -> if allowed, UserDashboardComponent appears
                    -> if rejected, navigate to /login
```

This is how refresh on protected pages can still work if token is stored.

---

## 3.11 Full Startup Scenario: Admin Opens Admin Page

Admin opens:

```text
http://localhost:4200/admin/users
```

Flow:

```text
Browser loads index.html
    -> Angular starts App
        -> router reads "/admin/users"
            -> route requires adminGuard
                -> adminGuard checks login and role
                    -> if user is admin, AdminUsersComponent appears
                    -> if not admin, redirect away
```

This keeps admin screens separate from candidate screens.

---

## 3.12 Full Startup Scenario: User Opens Unknown URL

User opens:

```text
http://localhost:4200/random-page
```

Flow:

```text
Router checks route list
    -> no route matches
        -> wildcard route "**" matches
            -> redirectTo: ""
                -> DashboardComponent appears
```

This prevents the user from staying on a broken route inside the app.

---

## 3.13 What This Topic Is Doing In Your Project

This startup system makes the whole frontend possible.

| Startup Part | Project Job |
| --- | --- |
| `index.html` | Gives browser the HTML shell and `<app-root>` |
| `main.ts` | Starts Angular |
| `app.config.ts` | Registers routing, HTTP, interceptor, error listeners |
| `app.component.ts` | Defines root app component |
| `app.component.html` | Provides `<router-outlet>` |
| `app.routes.ts` | Maps URLs to page components |

---

## 3.14 What If We Do Not Use This Startup Structure?

### Without `index.html`

Browser has no first page to load.

### Without `<app-root>`

Angular has no place to mount the root component.

### Without `main.ts`

Angular does not start.

### Without `app.config.ts`

Global features like router and HTTP may not be registered.

### Without `App` Component

There is no root component to bootstrap.

### Without `<router-outlet>`

Route components have no place to appear.

### Without `app.routes.ts`

Angular does not know which component belongs to which URL.

---

## 3.15 Alternative Ways To Structure Startup

### Alternative 1: Old NgModule Style

Older Angular apps use:

```text
AppModule
```

and start with:

```ts
platformBrowserDynamic().bootstrapModule(AppModule)
```

Your app uses modern standalone Angular:

```ts
bootstrapApplication(App, appConfig)
```

### Alternative 2: Root Layout In App Component

Some apps put navbar/footer directly in `app.component.html`:

```html
<app-navbar></app-navbar>
<router-outlet></router-outlet>
<app-footer></app-footer>
```

Your app keeps root minimal and uses separate navbars inside user/admin areas.

### Alternative 3: Lazy Routes

Large apps may split routes by feature:

```text
auth.routes.ts
admin.routes.ts
interview.routes.ts
assessment.routes.ts
```

Then load them only when needed.

Your app currently keeps all top-level routes in one file, which is easier for study.

### Alternative 4: Server-Side Rendering

Angular can also render pages on the server first.

That is useful for SEO or faster first load, but it adds complexity.

Your current app is a browser-based Angular SPA.

---

## 3.16 Common Mistakes

### Mistake 1: Selector Mismatch

`index.html` has:

```html
<app-root></app-root>
```

but component has:

```ts
selector: 'my-root'
```

Result:

```text
Angular cannot mount the root component correctly.
```

### Mistake 2: Forgetting RouterOutlet Import

If root component template uses:

```html
<router-outlet></router-outlet>
```

then standalone component must import:

```ts
imports: [RouterOutlet]
```

### Mistake 3: Forgetting `provideRouter(routes)`

Without this, Angular router is not registered.

### Mistake 4: Forgetting `provideHttpClient`

Services that inject `HttpClient` will fail.

### Mistake 5: Wrong Route Order

Wildcard route:

```ts
{ path: '**', redirectTo: '' }
```

should stay at the end.

If it appears before real routes, it can catch everything too early.

### Mistake 6: Missing Guard On Protected Pages

If a protected page does not include `canActivate`, users may open UI screens they should not see.

Again, backend must still protect actual data.

---

## 3.17 Mini Exercise

Open:

```text
src/index.html
```

Answer:

1. What tag does Angular use to mount the app?
2. What is the browser tab title?
3. Why is `<base href="/">` important?

Open:

```text
src/main.ts
```

Answer:

1. Which function starts Angular?
2. Which component is bootstrapped?
3. Which config object is passed during startup?

Open:

```text
src/app/app.config.ts
```

Answer:

1. Which provider enables routing?
2. Which provider enables HTTP?
3. Which interceptor is registered?

Open:

```text
src/app/app.routes.ts
```

Answer:

1. Which component opens for `/login`?
2. Which component opens for `/admin/questions`?
3. Which routes contain `:id`?
4. Which routes use `authGuard`?
5. Which routes use `adminGuard`?
6. What happens for an unknown route?

---

## 3.18 Interview-Style Questions

1. What is the role of `index.html` in an Angular app?
2. What is the purpose of `<app-root>`?
3. What does `bootstrapApplication` do?
4. What is `app.config.ts` used for?
5. Why does this project use `provideRouter(routes)`?
6. Why does this project use `provideHttpClient(withInterceptors(...))`?
7. What is the root component in this project?
8. What does `<router-outlet>` do?
9. Why is the wildcard route placed at the end?
10. What is the difference between a public route and a guarded route?

---

## Topic 3 Summary

Your Angular app starts in this order:

```text
Browser opens index.html.
Angular finds <app-root>.
main.ts bootstraps App.
app.config.ts registers global providers.
App template shows router-outlet.
Router checks app.routes.ts.
Matched page component appears.
```

The most important mental model:

```text
index.html gives Angular a place.
main.ts starts Angular.
app.config.ts gives Angular app-wide abilities.
app.component.ts defines the root component.
app.component.html gives routes a display area.
app.routes.ts decides which page appears.
```

---

## Next Topic

Recommended next lesson:

```text
Topic 4: Angular Components
```

Files to study next:

```text
src/app/components/login/login.component.ts
src/app/components/login/login.component.html
src/app/components/login/login.component.css
src/app/components/user-dashboard/user-dashboard.component.ts
src/app/components/shared/user-navbar/user-navbar.component.ts
```
