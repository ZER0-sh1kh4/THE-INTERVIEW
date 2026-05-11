# Topic 4: Angular Components

This lesson explains Angular components using your actual project files.

Project frontend path:

```text
Frontend/
```

Main idea:

```text
An Angular component is one visible part of the website. It usually has TypeScript for logic, HTML for structure, and CSS for styling.
```

In your project, almost every screen is a component:

```text
Login page = LoginComponent
Landing page = DashboardComponent
User dashboard page = UserDashboardComponent
Interview start page = InterviewStartComponent
Admin users page = AdminUsersComponent
Navbar = UserNavbarComponent
```

---

## 4.1 What Is A Component?

### Simple Explanation

A component is a reusable block of UI.

It can be:

- A full page.
- A navbar.
- A form.
- A table.
- A card.
- A session screen.
- A result screen.

### Easy Scenario

Think of a website like a building.

- The whole website is the building.
- Pages are rooms.
- Components are parts of rooms: door, table, light, form, notice board.

In Angular:

```text
LoginComponent is the login room.
UserNavbarComponent is the navigation bar.
InterviewStartComponent is the interview setup room.
```

### What A Component Usually Contains

Most Angular components have three files:

```text
component.ts
component.html
component.css
```

Example:

```text
src/app/components/login/login.component.ts
src/app/components/login/login.component.html
src/app/components/login/login.component.css
```

### What Each File Does

| File | Purpose |
| --- | --- |
| `.ts` | Logic, data, methods, service calls, navigation |
| `.html` | Visible structure, inputs, buttons, bindings |
| `.css` | Styling, layout, colors, spacing, responsive behavior |

### What Components Are Doing In This Project

Your frontend is divided into components so each feature has its own place.

Examples:

| Component | Job |
| --- | --- |
| `DashboardComponent` | Shows public landing page |
| `LoginComponent` | Handles login form and login API flow |
| `UserNavbarComponent` | Shows user navigation, notifications, logout |
| `InterviewStartComponent` | Lets user configure and start an interview |
| `AssessmentSessionComponent` | Shows assessment questions and answer options |
| `PremiumComponent` | Handles subscription upgrade/cancel UI |
| `AdminUsersComponent` | Lets admin manage users |

### Why We Use Components

Components help because:

- Code is organized by feature.
- UI can be reused.
- Each component has a clear responsibility.
- Bugs are easier to locate.
- Styling can stay close to the related UI.
- Tests can target one part at a time.

### What If We Do Not Use Components?

If everything is written in one giant file:

- Login, register, interviews, assessments, admin, profile, and navbar logic all mix together.
- Debugging becomes confusing.
- One small change may break unrelated pages.
- Reusing UI becomes difficult.
- The project becomes harder to explain and maintain.

### Alternatives

Other frontend frameworks have similar ideas:

| Framework | Similar Concept |
| --- | --- |
| React | Component |
| Vue | Component |
| Svelte | Component |
| ASP.NET MVC/Razor | Partial views/views |
| Plain JS | Manually managed DOM sections |

Angular components are strongly structured with decorators, templates, styles, imports, and dependency injection.

---

## 4.2 Component Class, Template, And CSS

### Simple Explanation

An Angular component normally has:

```text
Class = brain
Template = face/body
CSS = clothes/style
```

### Example: Login Component

Files:

```text
src/app/components/login/login.component.ts
src/app/components/login/login.component.html
src/app/components/login/login.component.css
```

### What Each File Does In Login

#### TypeScript

```text
login.component.ts
```

Controls:

- Login form.
- Loading state.
- Error message.
- Password visibility.
- Calling `AuthService`.
- Navigating after successful login.

#### HTML

```text
login.component.html
```

Shows:

- Email input.
- Password input.
- Login button.
- Error alert.
- Register link.
- Forgot password link.
- Image and layout.

#### CSS

```text
login.component.css
```

Controls:

- Grid layout.
- Header spacing.
- Form styling.
- Button styling.
- Responsive behavior.
- Error alert styling.

### Why Split Into Three Files?

This split keeps responsibilities clean.

| Concern | File |
| --- | --- |
| What should happen? | `.ts` |
| What should appear? | `.html` |
| How should it look? | `.css` |

### What If Everything Was In One File?

Angular supports inline templates and styles, but for larger components it becomes hard to read.

Example:

```ts
@Component({
  template: `<form>...</form>`,
  styles: [`button { color: red; }`]
})
```

This is okay for tiny components, but your project pages are large, so separate files are better.

---

## 4.3 Component Decorator: `@Component`

### Simple Explanation

The `@Component` decorator tells Angular:

```text
This class is not just a normal TypeScript class.
This class controls a piece of UI.
Here is its selector, HTML file, CSS file, and imports.
```

### Example From Login Component

```ts
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
}
```

### Line-By-Line Explanation

```ts
@Component({
```

Starts the Angular component metadata.

```ts
selector: 'app-login',
```

This is the component tag name.

If another template uses:

```html
<app-login></app-login>
```

Angular knows it means `LoginComponent`.

In your route file, login is opened by route, not by writing `<app-login>` manually:

```ts
{ path: 'login', component: LoginComponent }
```

Still, the selector identifies the component.

```ts
standalone: true,
```

Means this component does not need to be declared inside an Angular NgModule.

Your project uses modern standalone Angular.

```ts
imports: [CommonModule, ReactiveFormsModule, RouterModule],
```

This tells Angular what features this component template can use.

For login:

- `CommonModule` gives common directives like `*ngIf`.
- `ReactiveFormsModule` gives `[formGroup]` and `formControlName`.
- `RouterModule` gives `routerLink`.

```ts
templateUrl: './login.component.html',
```

Connects the HTML file.

```ts
styleUrl: './login.component.css'
```

Connects the CSS file.

```ts
export class LoginComponent {
```

The class contains the component's data and methods.

### What If `imports` Is Missing Something?

If login HTML uses:

```html
<form [formGroup]="loginForm">
```

but `ReactiveFormsModule` is not imported, Angular will not understand `[formGroup]`.

If HTML uses:

```html
<a routerLink="/register">
```

but `RouterModule` is not imported, Angular will not understand `routerLink`.

### Alternative: NgModule-Based Components

Older Angular apps declare components in modules:

```ts
@NgModule({
  declarations: [LoginComponent],
  imports: [CommonModule, ReactiveFormsModule]
})
```

Your app uses standalone components, where each component declares its own imports.

---

## 4.4 Component Selector

### Simple Explanation

A selector is the custom HTML tag for a component.

Example:

```ts
selector: 'app-user-navbar'
```

means Angular can render the navbar using:

```html
<app-user-navbar></app-user-navbar>
```

### Example From Project

`UserNavbarComponent` has:

```ts
selector: 'app-user-navbar'
```

`InterviewStartComponent` imports it:

```ts
imports: [CommonModule, FormsModule, RouterModule, UserNavbarComponent]
```

Then the interview start HTML can use the navbar component.

### Why Selectors Matter

Selectors allow one component to be used inside another.

This is how reusable UI is built.

### What If Selector Names Conflict?

If two components use the same selector, Angular can become confused or throw an error.

Good selector names should be unique and descriptive.

### Naming Style In Your Project

Your project uses the `app-` prefix:

```text
app-login
app-dashboard
app-user-navbar
app-interview-start
app-admin-users
```

This matches `angular.json`:

```json
"prefix": "app"
```

---

## 4.5 Standalone Component Imports

### Simple Explanation

Standalone components must import the Angular features and components they use in their template.

### Example: Dashboard Component

File:

```text
src/app/components/dashboard/dashboard.component.ts
```

Code:

```ts
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {}
```

### Why Dashboard Imports `RouterModule`

The dashboard page likely has navigation links like:

```html
<a routerLink="/login">Login</a>
```

For `routerLink` to work, the component imports:

```ts
RouterModule
```

### Example: Login Component Imports

```ts
imports: [CommonModule, ReactiveFormsModule, RouterModule]
```

Why:

| Import | Needed For |
| --- | --- |
| `CommonModule` | `*ngIf`, common Angular directives |
| `ReactiveFormsModule` | `[formGroup]`, `formControlName` |
| `RouterModule` | `routerLink` |

### Example: Interview Start Imports

```ts
imports: [CommonModule, FormsModule, RouterModule, UserNavbarComponent]
```

Why:

| Import | Needed For |
| --- | --- |
| `CommonModule` | Common Angular directives |
| `FormsModule` | `[(ngModel)]` template-driven forms |
| `RouterModule` | Route links/navigation helpers in template |
| `UserNavbarComponent` | Shows shared user navbar |

### What If We Forget Imports?

Angular template errors happen.

Examples:

- `routerLink` unknown.
- `ngModel` unknown.
- `formGroup` unknown.
- `app-user-navbar` unknown.
- `ngIf` unknown.

### Alternative

Old module-based Angular imports common features at NgModule level.

Standalone Angular imports dependencies directly in the component, which makes the dependency list easier to see.

---

## 4.6 Component State

### Simple Explanation

State means data that can change while the component is running.

Examples:

- Is the form loading?
- What error message should show?
- Is password visible?
- Which tech stack is selected?
- Is notification dropdown open?

### Login Component State

```ts
loginForm: FormGroup;
isLoading = false;
errorMsg = '';
showPassword = false;
```

Meaning:

| State | Meaning |
| --- | --- |
| `loginForm` | Holds email/password form controls |
| `isLoading` | True while login request is running |
| `errorMsg` | Error text shown in UI |
| `showPassword` | Controls password input type |

### Template Uses This State

Error message:

```html
<div class="error-alert" *ngIf="errorMsg">
  <p class="text-error uppercase">{{ errorMsg }}</p>
</div>
```

Meaning:

```text
If errorMsg has text, show the error alert.
```

Password input:

```html
[type]="showPassword ? 'text' : 'password'"
```

Meaning:

```text
If showPassword is true, show actual text.
Otherwise hide password.
```

Submit button:

```html
[disabled]="isLoading || loginForm.invalid"
```

Meaning:

```text
Disable button while loading or when form is invalid.
```

### Interview Start Component State

```ts
experienceLevels = ['Fresher', '1-3 years', '3-5 years', '5+ years'];
interviewTypes = ['Technical', 'HR', 'Mixed'];
techOptions = ['Angular', 'Node.js', 'Java', 'SQL', 'TypeScript', 'AWS', 'C#', '.NET'];
difficulties = ['Easy', 'Medium', 'Hard'];
questionCounts = [5, 10, 20];
```

These arrays feed dropdowns, chips, or selectable UI options.

```ts
form: InterviewStartForm = {
  role: '',
  experience: '1-3 years',
  interviewType: 'Technical',
  techStack: ['Angular', 'TypeScript'],
  difficulty: 'Medium',
  numberOfQuestions: 5
};
```

This is the form state sent to the backend when starting an interview.

### User Navbar State

```ts
currentUser$: Observable<User | null | undefined>;
notifications$: Observable<Notification[]>;
unreadCount$: Observable<number>;
showNotifications = false;
```

This component uses observable state from services.

It displays:

- Current user.
- Notifications.
- Unread count.
- Notification dropdown state.

### Why Component State Matters

State connects user actions to UI updates.

If user clicks "show password":

```text
showPassword changes -> input type changes -> UI updates
```

If login fails:

```text
errorMsg changes -> error alert appears
```

### What If State Is Not Managed Clearly?

The UI becomes unpredictable.

Examples:

- Loading spinner never stops.
- Error message remains after success.
- Button stays disabled.
- Wrong user data appears.
- Old interview options are submitted.

### Alternatives

State can live in:

- Component properties.
- Services.
- Browser storage.
- URL/query parameters.
- Angular signals.
- RxJS Observables.
- NgRx/store libraries.

Your project mostly uses component properties plus services and RxJS.

---

## 4.7 Component Methods

### Simple Explanation

Methods are functions inside the component class.

They usually run when:

- User clicks a button.
- User submits a form.
- Component loads.
- Component is destroyed.
- UI state must be calculated.

### Login Method: Toggle Password

```ts
togglePasswordVisibility() {
  this.showPassword = !this.showPassword;
}
```

### What It Does

If `showPassword` is false, it becomes true.

If it is true, it becomes false.

The HTML uses:

```html
[type]="showPassword ? 'text' : 'password'"
```

So the password input changes between hidden and visible.

### Login Method: Submit

```ts
onSubmit() {
  if (this.loginForm.invalid) return;

  this.isLoading = true;
  this.errorMsg = '';

  this.authService.login(this.loginForm.value).subscribe({
    next: () => {
      this.isLoading = false;
      this.authService.currentUser$.subscribe(user => {
        if (user) {
          if (user.role === 'Admin') {
            this.router.navigate(['/admin']);
          } else {
            this.router.navigate(['/user-dashboard']);
          }
        }
      });
    },
    error: (err) => {
      this.isLoading = false;
      this.errorMsg = 'Invalid credentials. Please try again.';
    }
  });
}
```

### Step-By-Step

```ts
if (this.loginForm.invalid) return;
```

If form is invalid, stop immediately.

Example:

- Email missing.
- Email format invalid.
- Password missing.

```ts
this.isLoading = true;
```

Show loading state and prevent repeated submission.

```ts
this.errorMsg = '';
```

Clear previous error.

```ts
this.authService.login(this.loginForm.value).subscribe({
```

Call auth service and subscribe to the async result.

```ts
next: () => {
```

Runs if login succeeds.

```ts
this.isLoading = false;
```

Stop loading.

```ts
this.authService.currentUser$.subscribe(user => {
```

Wait for user details to be available.

```ts
if (user.role === 'Admin') {
  this.router.navigate(['/admin']);
} else {
  this.router.navigate(['/user-dashboard']);
}
```

Navigate admin users to admin dashboard and normal users to user dashboard.

```ts
error: (err) => {
  this.isLoading = false;
  this.errorMsg = 'Invalid credentials. Please try again.';
}
```

If login fails, stop loading and show error.

### Interview Start Method: Start Interview

```ts
startInterview(): void {
  if (!this.form.role.trim() || this.form.techStack.length === 0) {
    this.errorMsg = 'Please enter a job role and select at least one tech stack item.';
    return;
  }

  this.isLoading = true;
  this.errorMsg = '';
  this.infoMsg = 'Preparing your interview...';

  this.interviewService.createInterviewSession(this.form).subscribe({
    next: interview => {
      this.isLoading = false;
      if (!interview.id) {
        this.errorMsg = 'Interview was created, but the session id could not be resolved. Please try again.';
        return;
      }

      this.router.navigate(['/interviews', interview.id, 'preflight']);
    },
    error: error => {
      this.isLoading = false;
      this.infoMsg = '';
      const message = error?.error?.message || error?.error?.Message || 'Unable to start interview. Please try again.';
      if (error?.status === 403 || message.toLowerCase().includes('upgrade to premium')) {
        this.router.navigate(['/premium'], {
          queryParams: { reason: 'interview-limit' },
          state: { upgradeMessage: message }
        });
        return;
      }

      this.errorMsg = message;
    }
  });
}
```

### What It Does In The Website

When user clicks start interview:

1. Validate role and tech stack.
2. Show loading message.
3. Call backend through `InterviewService`.
4. If backend returns interview id, navigate to preflight page.
5. If free-user limit is hit, navigate to premium page.
6. Otherwise show error.

### Why Methods Are Needed

HTML should not contain complex logic.

The template should say:

```html
(click)="startInterview()"
```

The TypeScript class should handle the full logic.

### What If We Put Too Much Logic In HTML?

Template becomes hard to read.

Bad style:

```html
<button (click)="isLoading = true; errorMsg = ''; service.call(...).subscribe(...)">
```

Better:

```html
<button (click)="startInterview()">
```

---

## 4.8 Constructor And Dependency Injection

### Simple Explanation

The constructor is where Angular gives the component the services it needs.

This is called dependency injection.

### Login Constructor

```ts
constructor(
  private fb: FormBuilder,
  private authService: AuthService,
  private router: Router
) {
  this.loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });
}
```

### What Angular Injects

| Dependency | Why Login Needs It |
| --- | --- |
| `FormBuilder` | Create reactive login form |
| `AuthService` | Call login API |
| `Router` | Navigate after login |

### What Constructor Does Here

It creates the login form:

```ts
this.loginForm = this.fb.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', Validators.required]
});
```

Meaning:

- Email starts empty and is required.
- Email must be valid email format.
- Password starts empty and is required.

### User Navbar Constructor

```ts
constructor(
  private authService: AuthService, 
  private router: Router,
  private notificationService: NotificationService
) {
  this.currentUser$ = this.authService.currentUser$;
  this.notifications$ = this.notificationService.notifications$;
  this.unreadCount$ = this.notificationService.unreadCount$;
  
  this.notificationService.fetchNotifications().pipe(
    catchError(() => of([] as Notification[]))
  ).subscribe();
}
```

### What It Does

The navbar receives:

- Auth service for current user and logout.
- Router for navigation.
- Notification service for notifications.

It connects observables:

```ts
this.currentUser$ = this.authService.currentUser$;
```

Then it fetches notifications once when navbar is created.

### Why Dependency Injection Is Useful

Without dependency injection, components would manually create services:

```ts
this.authService = new AuthService(...)
```

That would be messy because services need their own dependencies like `HttpClient`.

Angular DI manages that for us.

### What If Service Is Not Provided?

Angular will throw an injection error.

Example:

```text
No provider for AuthService
```

In your project, services often use:

```ts
@Injectable({ providedIn: 'root' })
```

which makes them available app-wide.

---

## 4.9 Component Lifecycle

### Simple Explanation

Lifecycle means important moments in a component's life.

Main moments:

```text
Component is created.
Component is initialized.
Component is displayed.
Component is destroyed.
```

### Common Lifecycle Hooks

| Hook | Meaning |
| --- | --- |
| `constructor` | Class is created and dependencies are injected |
| `ngOnInit` | Component is initialized; good place to load data |
| `ngOnDestroy` | Component is about to be removed; good place to clean up |

### Example: InterviewStartComponent

```ts
export class InterviewStartComponent implements OnInit, OnDestroy {
```

This says the component uses two lifecycle hooks:

```text
ngOnInit
ngOnDestroy
```

### `ngOnInit`

```ts
ngOnInit(): void {
  // Warm-up deferred until user types a role ...
}
```

This runs after Angular creates the component.

In this component, it currently does not call the backend immediately. The comment explains why:

```text
Calling warm-up immediately with an empty role would waste backend/API limits.
```

### `ngOnDestroy`

```ts
ngOnDestroy(): void {
  if (this.warmUpTimer) {
    clearTimeout(this.warmUpTimer);
  }
}
```

This runs when the component is removed from the screen.

It clears a timer.

### Why Cleanup Matters

The component uses:

```ts
setTimeout(() => this.triggerWarmUp(), 3000)
```

If user leaves the page before the timer runs, the timer should be cleared.

Otherwise:

- API may be called after user left the page.
- Memory can be wasted.
- Unexpected state changes can happen.

### What If We Do Not Clean Up?

Possible issues:

- Old timers continue running.
- API calls fire from closed pages.
- Memory leaks.
- Duplicate actions.
- Strange UI behavior.

### Alternatives

Other cleanup patterns:

- Use RxJS `takeUntilDestroyed`.
- Use `DestroyRef`.
- Use `async` pipe in templates.
- Avoid manual timers when possible.

---

## 4.10 Component Communication

### Simple Explanation

Components often need to share information.

Angular has several ways:

- Parent passes data to child.
- Child sends event to parent.
- Shared service stores data.
- Router passes route params.
- Browser storage stores temporary state.

### Project Example: Shared Service Communication

`UserNavbarComponent` gets current user from `AuthService`:

```ts
this.currentUser$ = this.authService.currentUser$;
```

Login updates current user inside `AuthService`.

Navbar reads it from the same service.

This means login and navbar do not need to talk directly.

### Project Example: Router Communication

Interview start navigates with id:

```ts
this.router.navigate(['/interviews', interview.id, 'preflight']);
```

This creates URL:

```text
/interviews/123/preflight
```

The next component can read `123` from route params.

### Project Example: Component Reuse

`InterviewStartComponent` imports:

```ts
UserNavbarComponent
```

This lets the interview page display the shared user navbar.

### Why Communication Matters

Without communication:

- Login page cannot affect navbar.
- Interview start cannot move to session page.
- Admin pages cannot share admin service logic.
- User state would be duplicated everywhere.

### Alternatives

Communication options:

| Method | Good For |
| --- | --- |
| `@Input` | Parent sends data to child |
| `@Output` | Child sends event to parent |
| Shared service | App-wide or feature-wide state |
| Router params | URL-based state like ids |
| Query params | Filters, reasons, small navigation context |
| NgRx/store | Large app global state |

---

## 4.11 Page Components vs Shared Components

### Page Component

A page component is usually connected to a route.

Example:

```ts
{ path: 'login', component: LoginComponent }
```

`LoginComponent` is a page component.

### Shared Component

A shared component is usually used inside other components.

Example:

```text
UserNavbarComponent
```

It appears on user pages and gives navigation/logout/notification behavior.

### Project Page Components

Examples:

```text
DashboardComponent
LoginComponent
RegisterComponent
ResetPasswordComponent
UserDashboardComponent
InterviewStartComponent
InterviewSessionComponent
AssessmentStartComponent
PremiumComponent
ProfileComponent
AdminDashboardComponent
AdminUsersComponent
```

### Project Shared Components

Examples:

```text
UserNavbarComponent
AdminNavbarComponent
```

### Why Separate Them?

Page components own screen-level behavior.

Shared components own reusable UI behavior.

This keeps code organized.

---

## 4.12 HTML Template Bindings In Components

### Simple Explanation

The HTML template reads data and calls methods from the component class.

### Interpolation

```html
{{ errorMsg }}
```

Shows a TypeScript value in HTML.

### Property Binding

```html
[disabled]="isLoading || loginForm.invalid"
```

Sets an HTML property based on component state.

### Event Binding

```html
(click)="togglePasswordVisibility()"
```

Runs a component method when user clicks.

### Form Binding

```html
[formGroup]="loginForm"
formControlName="email"
```

Connects HTML form to the component's reactive form.

### Conditional Rendering

```html
*ngIf="errorMsg"
```

Shows the element only when `errorMsg` has a value.

### Router Link

```html
routerLink="/register"
```

Navigates to another Angular route without full page reload.

### What If Template Uses A Missing Property?

If HTML says:

```html
{{ userName }}
```

but the component class has no `userName`, Angular strict template checking can report an error.

This is good because it catches mistakes early.

---

## 4.13 Component CSS

### Simple Explanation

Component CSS controls how that component looks.

Example:

```text
login.component.css
```

styles:

- Login form.
- Header.
- Grid layout.
- Buttons.
- Inputs.
- Error messages.
- Responsive behavior.

### Example CSS

```css
.login-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: var(--spacing-gutter);
}
```

Meaning:

```text
Use a 12-column grid for the login page layout.
```

```css
.error-alert {
  margin-bottom: 2.5rem;
  padding: 1rem;
  border: 1px solid rgba(186, 26, 26, 0.3);
  background-color: rgba(186, 26, 26, 0.05);
}
```

Meaning:

```text
Style the login error box with spacing, red border, and light red background.
```

```css
@media (min-width: 768px) {
  .login-form-wrapper { grid-column: 2 / span 5; }
}
```

Meaning:

```text
On larger screens, place the login form in specific grid columns.
```

### Why Component CSS Is Useful

It keeps page-specific styles close to the page.

Login styles stay with login.

Navbar styles stay with navbar.

Admin styles stay with admin components.

### What If All CSS Is Global?

Problems:

- Class names can conflict.
- One page's styles can accidentally affect another page.
- CSS file becomes huge.
- It is harder to delete old styles safely.

### Alternatives

Styling alternatives:

- Global CSS only.
- SCSS.
- CSS modules-style approaches.
- Tailwind CSS.
- Bootstrap.
- Angular Material.
- Design system components.

---

## 4.14 Component Example 1: `DashboardComponent`

File:

```text
src/app/components/dashboard/dashboard.component.ts
```

Code:

```ts
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {}
```

### What It Does

This is the public landing page component.

It mostly acts as a UI shell.

### Why The Class Is Empty

The page likely does not need much TypeScript logic.

It can show content and links using HTML/CSS.

Empty class is okay when the component is mostly static UI.

### What If We Added Logic?

If the dashboard later needs:

- Dynamic stats.
- User-specific buttons.
- API-driven content.
- Animations controlled by state.

then properties and methods can be added to the class.

---

## 4.15 Component Example 2: `LoginComponent`

File:

```text
src/app/components/login/login.component.ts
```

### What It Does

`LoginComponent` controls the login screen.

It handles:

- Email input.
- Password input.
- Form validation.
- Loading state.
- Error state.
- Show/hide password.
- Login API call.
- Navigation after login.

### User Scenario

1. User opens `/login`.
2. Router displays `LoginComponent`.
3. User types email/password.
4. User clicks login.
5. Component validates the form.
6. Component calls `AuthService.login`.
7. Auth service stores token and loads user.
8. Component navigates based on role.

### Why This Component Uses Reactive Forms

Reactive forms make validation and form state explicit in TypeScript:

```ts
this.fb.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', Validators.required]
});
```

This is useful for login because validation rules are clear.

### What If We Did Not Validate?

The user could submit:

- Empty email.
- Invalid email.
- Empty password.

Then backend would reject the request, but user experience would be worse.

### Alternative

Could use template-driven forms with `FormsModule` and `ngModel`.

Your interview start page uses that style.

---

## 4.16 Component Example 3: `UserNavbarComponent`

File:

```text
src/app/components/shared/user-navbar/user-navbar.component.ts
```

### What It Does

This component shows user navigation.

It handles:

- Current user display.
- Plan label.
- Notifications.
- Marking notifications as read.
- Navigation from notification click.
- Logout.

### Important State

```ts
currentUser$: Observable<User | null | undefined>;
notifications$: Observable<Notification[]>;
unreadCount$: Observable<number>;
showNotifications = false;
```

This component mostly uses observables from services.

### Important Methods

```ts
toggleNotifications()
```

Opens/closes the notification dropdown and fetches/marks notifications.

```ts
onNotificationClick(note: Notification)
```

Navigates if notification has an action URL.

```ts
getDisplayName(user: User | null)
```

Creates a user-friendly display name from email.

```ts
logout()
```

Logs out and navigates to `/login`.

### Why Navbar Is A Shared Component

Many user pages need the same navbar.

Instead of copying navbar HTML into every page, the project uses one component.

### What If Navbar Logic Was Copied Everywhere?

Problems:

- Logout code duplicated.
- Notification code duplicated.
- UI changes must be repeated.
- Bugs become inconsistent.

---

## 4.17 Component Example 4: `InterviewStartComponent`

File:

```text
src/app/components/interviews/interview-start/interview-start.component.ts
```

### What It Does

This component lets the user configure an AI mock interview.

It handles:

- Job role.
- Experience level.
- Interview type.
- Tech stack.
- Difficulty.
- Number of questions.
- Backend warm-up.
- Starting interview session.
- Premium-limit redirect.

### Why It Uses `OnInit` And `OnDestroy`

It uses lifecycle hooks because it has timer-based behavior.

```ts
implements OnInit, OnDestroy
```

`ngOnDestroy` clears the warm-up timer when user leaves.

### Why It Uses `FormsModule`

It uses template-driven form state:

```ts
form: InterviewStartForm = { ... }
```

The HTML can bind inputs directly to this object using `ngModel`.

### Why It Uses A Model

```ts
InterviewStartForm
```

defines the expected shape of the form.

This helps TypeScript catch mistakes.

### Why It Calls A Service

It does not call `HttpClient` directly.

It calls:

```ts
this.interviewService.createInterviewSession(this.form)
```

This keeps backend API details inside `InterviewService`.

### What If This Component Called Backend Directly?

It would still work, but:

- Component becomes too large.
- API endpoint logic is duplicated.
- Testing becomes harder.
- Changing backend URL becomes harder.

---

## 4.18 Component Responsibilities In This Project

### Good Component Responsibility

A component should:

- Display UI.
- Hold page-specific state.
- React to user actions.
- Call services for backend work.
- Navigate when needed.
- Show loading/error/success state.

### Component Should Usually Not

A component should usually not:

- Contain repeated API URL strings.
- Contain backend business rules.
- Directly manipulate the DOM too much.
- Store unrelated global state.
- Become thousands of lines long.

### Project Pattern

Your project usually follows:

```text
Component
    -> Service
        -> Backend API
```

Example:

```text
LoginComponent
    -> AuthService
        -> /auth/login
```

Example:

```text
InterviewStartComponent
    -> InterviewService
        -> interview backend endpoints
```

---

## 4.19 Common Component Mistakes

### Mistake 1: Forgetting Standalone Imports

Symptom:

```text
Angular says a directive/component is unknown.
```

Fix:

```ts
imports: [CommonModule, RouterModule, FormsModule]
```

as needed.

### Mistake 2: Too Much Logic In Template

Bad:

```html
<button (click)="a = true; b = ''; service.call().subscribe(...)">
```

Better:

```html
<button (click)="startInterview()">
```

### Mistake 3: Not Handling Loading State

Without loading state, users may click the same button many times.

### Mistake 4: Not Handling Error State

Without error state, users do not know what went wrong.

### Mistake 5: Not Cleaning Timers Or Long Subscriptions

Can cause memory leaks or unexpected API calls.

### Mistake 6: Putting API Details In Every Component

Better:

```text
Put API calls in services.
Let components call services.
```

### Mistake 7: Using `any` Everywhere

Better:

```text
Use models/interfaces like InterviewStartForm, User, Notification.
```

---

## 4.20 What If We Do Not Use Components Properly?

If components are not designed well:

- Pages become hard to maintain.
- UI and API logic mix too much.
- State bugs increase.
- Reusable UI gets duplicated.
- Tests become difficult.
- New features take longer.

Good components make the project easier to grow.

---

## 4.21 Alternative Component Design Approaches

### Smart And Presentational Components

Smart components:

- Load data.
- Call services.
- Handle navigation.

Presentational components:

- Receive data.
- Display UI.
- Emit events.

Your project mostly uses page components that are both smart and visual.

### Container And Child Components

A big page can be split into smaller child components.

Example:

```text
InterviewSessionComponent
    -> QuestionPanelComponent
    -> TimerComponent
    -> AnswerBoxComponent
```

This can help if a component becomes too large.

### Signal-Based Components

Angular also supports signals for reactive state:

```ts
title = signal('frontend');
```

Your root `App` component already has a basic signal:

```ts
protected readonly title = signal('frontend');
```

Most project state currently uses class properties and RxJS.

### State Store Approach

For larger apps, shared state can move into:

- NgRx.
- Signal store.
- ComponentStore.
- Custom facade services.

Your current service-based approach is simpler and good for learning.

---

## 4.22 File-By-File Study Checklist For Components

When opening any component, study in this order:

1. What route or parent component uses it?
2. What files belong to it?
3. What does the selector say?
4. What does it import?
5. What state properties does it define?
6. What services does it inject?
7. What lifecycle hooks does it use?
8. What methods respond to user actions?
9. What service calls does it make?
10. What routes does it navigate to?
11. What template bindings use its state?
12. What CSS classes style its UI?
13. What happens if this component is removed?
14. What could be split or improved later?

---

## 4.23 Mini Exercise

Open:

```text
src/app/components/login/login.component.ts
```

Answer:

1. What is the selector?
2. Which modules are imported in `imports`?
3. What state variables are defined?
4. Which services are injected?
5. What does `togglePasswordVisibility()` do?
6. What does `onSubmit()` do?

Open:

```text
src/app/components/shared/user-navbar/user-navbar.component.ts
```

Answer:

1. Why is this a shared component?
2. Which observables does it expose?
3. What happens when user clicks logout?
4. What happens when user clicks a notification?

Open:

```text
src/app/components/interviews/interview-start/interview-start.component.ts
```

Answer:

1. What lifecycle hooks does it use?
2. Why does it clear `warmUpTimer`?
3. What does `toggleTech()` do?
4. What service starts the interview?
5. Where does it navigate after interview creation?

---

## 4.24 Interview-Style Questions

1. What is an Angular component?
2. What are the three common files of a component?
3. What does the `@Component` decorator do?
4. What is a selector?
5. Why do standalone components have an `imports` array?
6. What is component state?
7. What is the difference between component state and service state?
8. What is dependency injection?
9. What is `ngOnInit` used for?
10. What is `ngOnDestroy` used for?
11. Why should API calls usually be placed in services?
12. What does `routerLink` do in a component template?
13. What does event binding like `(click)` do?
14. What does property binding like `[disabled]` do?
15. Why should shared UI like a navbar be its own component?

---

## Topic 4 Summary

Angular components are the building blocks of your frontend.

Most important mental model:

```text
Component TypeScript = logic and state.
Component HTML = visible structure and bindings.
Component CSS = styling and layout.
Component decorator = tells Angular how the class connects to HTML/CSS/imports.
Component imports = features this standalone component can use.
Component services = shared/backend logic used by the component.
```

In your project:

```text
DashboardComponent is mostly static UI.
LoginComponent manages a reactive login form.
UserNavbarComponent shares user/notification/logout UI.
InterviewStartComponent manages a feature form, timers, service calls, and navigation.
```

Once you understand components, the rest of Angular becomes much easier because routes, guards, services, and templates all connect back to components.

---

## Next Topic

Recommended next lesson:

```text
Topic 5: Angular Templates And HTML Binding
```

Files to study next:

```text
src/app/components/login/login.component.html
src/app/components/register/register.component.html
src/app/components/shared/user-navbar/user-navbar.component.html
src/app/components/interviews/interview-start/interview-start.component.html
src/app/components/assessments/assessment-session/assessment-session.component.html
```
