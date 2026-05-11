# Topic 5: Angular Templates And HTML Binding

This lesson explains Angular templates and binding using your actual project HTML files.

Project frontend path:

```text
Frontend/
```

Main idea:

```text
An Angular template is the HTML file of a component. Binding is how that HTML talks to the component TypeScript class.
```

In simple words:

```text
TypeScript stores data and methods.
HTML displays that data and calls those methods.
Binding is the bridge between them.
```

---

## 5.1 What Is An Angular Template?

### Simple Explanation

An Angular template is the component's HTML.

Example:

```text
login.component.html
```

is the template for:

```text
LoginComponent
```

### Easy Scenario

Think of a component like a restaurant counter.

- TypeScript is the staff behind the counter.
- HTML template is the counter screen/menu the customer sees.
- Binding is the communication between customer actions and staff logic.

When user clicks a button:

```text
HTML receives click -> Angular calls TypeScript method
```

When TypeScript changes a value:

```text
TypeScript state changes -> Angular updates HTML
```

### What Templates Are Doing In This Project

Templates display:

- Login/register forms.
- Dashboard stats.
- Navbar links.
- Notifications.
- Interview setup chips.
- Assessment questions.
- Admin tables.
- Loading states.
- Error messages.
- Buttons and navigation.

### Why Templates Are Important

Without templates:

- The user cannot see forms, buttons, tables, or pages.
- Component TypeScript would have logic but no visible UI.
- The app would have no browser experience.

---

## 5.2 Template And Component Connection

### Example: Login

Component class:

```text
src/app/components/login/login.component.ts
```

Template:

```text
src/app/components/login/login.component.html
```

CSS:

```text
src/app/components/login/login.component.css
```

The connection happens in the component decorator:

```ts
@Component({
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
```

### What This Means

The template can use public/protected properties and methods from the component class.

Example TypeScript:

```ts
errorMsg = '';
showPassword = false;

togglePasswordVisibility() {
  this.showPassword = !this.showPassword;
}
```

Example HTML:

```html
<div *ngIf="errorMsg">{{ errorMsg }}</div>
<button (click)="togglePasswordVisibility()">Toggle</button>
```

### Why This Connection Matters

The HTML does not work alone.

It depends on the TypeScript class for:

- Data.
- Form objects.
- Boolean flags.
- Arrays.
- Methods.
- Service-driven results.

---

## 5.3 Interpolation: `{{ value }}`

### Simple Explanation

Interpolation displays a TypeScript value in the HTML.

Syntax:

```html
{{ value }}
```

### Easy Scenario

If TypeScript has:

```ts
errorMsg = 'Invalid credentials';
```

HTML:

```html
{{ errorMsg }}
```

shows:

```text
Invalid credentials
```

### Project Example: Login Error

File:

```text
src/app/components/login/login.component.html
```

Code:

```html
<p class="text-error uppercase">{{ errorMsg }}</p>
```

Meaning:

```text
Show the current error message from LoginComponent.
```

### Project Example: User Dashboard Greeting

File:

```text
src/app/components/user-dashboard/user-dashboard.component.html
```

Code:

```html
<h1 class="font-editorial">{{ getGreeting() }}, {{ getDisplayName(user) }}</h1>
```

Meaning:

```text
Call getGreeting() and getDisplayName(user), then show their returned text.
```

Example output:

```text
Good Morning, shobhit
```

### Project Example: Assessment Question

File:

```text
src/app/components/assessments/assessment-session/assessment-session.component.html
```

Code:

```html
<h2 class="font-editorial q-text">{{ currentQuestion.text }}</h2>
```

Meaning:

```text
Show the current assessment question text.
```

### What If We Do Not Use Interpolation?

We would have to manually update DOM text using JavaScript.

Angular interpolation is cleaner and safer.

### Alternative Ways

Alternatives:

- Property binding for element properties.
- Direct DOM manipulation, usually not recommended.
- Pipes for formatted display.

---

## 5.4 Property Binding: `[property]="value"`

### Simple Explanation

Property binding sets an HTML property using a TypeScript expression.

Syntax:

```html
[property]="expression"
```

### Easy Scenario

Button should be disabled while loading.

TypeScript:

```ts
isLoading = true;
```

HTML:

```html
<button [disabled]="isLoading">Save</button>
```

Result:

```text
Button becomes disabled.
```

### Project Example: Login Submit Button

File:

```text
src/app/components/login/login.component.html
```

Code:

```html
<button 
  type="submit" 
  [disabled]="isLoading || loginForm.invalid"
>
```

Meaning:

```text
Disable the login button if login is loading or the form is invalid.
```

### Project Example: Password Input Type

Code:

```html
<input 
  [type]="showPassword ? 'text' : 'password'"
/>
```

Meaning:

```text
If showPassword is true, input type is text.
If showPassword is false, input type is password.
```

### Project Example: Router Link Property

File:

```text
src/app/components/user-dashboard/user-dashboard.component.html
```

Code:

```html
<a [routerLink]="row.link" class="row-link">{{ row.domain }}</a>
```

Meaning:

```text
Use row.link from TypeScript as the navigation target.
```

### What If We Use Normal HTML Attribute Instead?

This:

```html
disabled="isLoading"
```

does not evaluate the TypeScript variable the same way.

Angular binding:

```html
[disabled]="isLoading"
```

actually reads the component property.

### Alternative

For fixed values, normal HTML attributes are okay:

```html
type="submit"
```

For dynamic values, use property binding.

---

## 5.5 Event Binding: `(event)="method()"`

### Simple Explanation

Event binding runs TypeScript code when the user does something.

Syntax:

```html
(event)="method()"
```

### Common Events

| Event | Meaning |
| --- | --- |
| `(click)` | User clicked |
| `(ngSubmit)` | Form submitted |
| `(change)` | Input/select changed |
| `(keyup.enter)` | User pressed Enter key |
| `(ngModelChange)` | `ngModel` value changed |

### Project Example: Login Form Submit

File:

```text
src/app/components/login/login.component.html
```

Code:

```html
<form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
```

Meaning:

```text
When form submits, call onSubmit() in LoginComponent.
```

### Project Example: Toggle Password

Code:

```html
<button type="button" class="password-toggle" (click)="togglePasswordVisibility()">
```

Meaning:

```text
When user clicks password eye button, call togglePasswordVisibility().
```

### Project Example: Notification Bell

File:

```text
src/app/components/shared/user-navbar/user-navbar.component.html
```

Code:

```html
<button class="notification-bell" (click)="toggleNotifications()" title="Notifications">
```

Meaning:

```text
Clicking the bell opens or closes the notification panel.
```

### Project Example: Assessment Option

File:

```text
src/app/components/assessments/assessment-session/assessment-session.component.html
```

Code:

```html
<div class="option-card" (click)="selectOption('A')">
```

Meaning:

```text
When user clicks option A, call selectOption('A').
```

### What If We Do Not Use Event Binding?

User actions would not call component methods.

Buttons would appear but not do anything useful.

### Alternative

Plain JavaScript uses:

```js
button.addEventListener('click', handler)
```

Angular event binding is easier because it connects directly to component methods.

---

## 5.6 Two-Way Binding: `[(ngModel)]`

### Simple Explanation

Two-way binding means:

```text
TypeScript value updates HTML input.
HTML input updates TypeScript value.
```

Syntax:

```html
[(ngModel)]="property"
```

This is often called banana-in-a-box syntax because it uses:

```text
[()]
```

### Project Example: Interview Role

File:

```text
src/app/components/interviews/interview-start/interview-start.component.html
```

Code:

```html
<input
  name="role"
  [(ngModel)]="form.role"
  (ngModelChange)="onConfigChange()"
  required
/>
```

Meaning:

```text
The input shows form.role.
When user types, form.role updates.
When form.role changes, onConfigChange() runs.
```

### TypeScript Side

File:

```text
src/app/components/interviews/interview-start/interview-start.component.ts
```

Code:

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

### Project Example: Custom Skill

HTML:

```html
<input
  name="customSkill"
  [(ngModel)]="customSkill"
  type="text"
  placeholder="Add custom skill"
  (keyup.enter)="addCustomSkill()"
/>
```

Meaning:

```text
Input value and customSkill property stay synchronized.
Pressing Enter calls addCustomSkill().
```

### What Is Required For `ngModel`?

The component must import:

```ts
FormsModule
```

Example:

```ts
imports: [CommonModule, FormsModule, RouterModule, UserNavbarComponent]
```

### What If `FormsModule` Is Missing?

Angular will not understand:

```html
[(ngModel)]
```

and the template will fail.

### Alternative

Reactive forms use:

```html
[formGroup]="loginForm"
formControlName="email"
```

Your login/register forms use reactive forms. Your interview start form uses `ngModel`.

---

## 5.7 Reactive Form Binding

### Simple Explanation

Reactive forms connect HTML inputs to a `FormGroup` created in TypeScript.

### Project Example: Login

HTML:

```html
<form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
```

Input:

```html
<input 
  id="email" 
  type="email" 
  formControlName="email"
/>
```

TypeScript:

```ts
this.loginForm = this.fb.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', Validators.required]
});
```

### Meaning

The HTML form is connected to this TypeScript object:

```ts
loginForm
```

The email input is connected to:

```ts
loginForm.controls.email
```

The password input is connected to:

```ts
loginForm.controls.password
```

### Project Example: Register

File:

```text
src/app/components/register/register.component.html
```

Code:

```html
<form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="register-form">
```

Inputs:

```html
<input formControlName="fullName" type="text" />
<input formControlName="email" type="email" />
<input formControlName="password" [type]="showPassword ? 'text' : 'password'" />
<input formControlName="confirmPassword" [type]="showConfirmPassword ? 'text' : 'password'" />
<input formControlName="terms" id="terms" type="checkbox" />
```

### Why Reactive Forms Are Useful

Reactive forms are useful when:

- Validation is important.
- Form has multiple fields.
- Form needs custom validation.
- Submit button depends on form validity.

### Project Example: Register Button Disabled

```html
<button type="submit" [disabled]="registerForm.invalid || isLoading">
```

Meaning:

```text
Do not allow account creation if form is invalid or request is loading.
```

### What If Form Binding Is Wrong?

If HTML says:

```html
formControlName="emailAddress"
```

but TypeScript form has:

```ts
email
```

Angular cannot connect the input correctly.

---

## 5.8 Conditional Rendering: `*ngIf`

### Simple Explanation

`*ngIf` shows or hides an HTML element based on a condition.

Syntax:

```html
<div *ngIf="condition">Content</div>
```

### Project Example: Login Error

```html
<div class="error-alert" *ngIf="errorMsg">
  <p class="text-error uppercase">{{ errorMsg }}</p>
</div>
```

Meaning:

```text
Only show the error box when errorMsg has text.
```

### Project Example: User Navbar

```html
<header class="dashboard-nav" *ngIf="currentUser$ | async as user">
```

Meaning:

```text
Only show navbar after currentUser$ produces a user value.
Also create a local template variable named user.
```

### Project Example: Premium Upgrade Link

```html
<a *ngIf="user.isPremium !== true" routerLink="/premium" class="upgrade-cta label-caps">Upgrade</a>
```

Meaning:

```text
Show Upgrade link only for non-premium users.
```

### Project Example: Admin Panel Link

```html
<a *ngIf="user.role === 'Admin'" routerLink="/admin" class="admin-cta label-caps">Admin Panel</a>
```

Meaning:

```text
Show Admin Panel link only for admin users.
```

### Project Example: Assessment Main Area

```html
<main class="question-area" *ngIf="currentQuestion">
```

Meaning:

```text
Only show question UI if a current question exists.
```

### What If We Do Not Use `*ngIf`?

Possible problems:

- Error boxes show when there is no error.
- UI tries to read data before it exists.
- User sees admin links when they should not.
- Template may crash from null/undefined data.

### Alternative

Newer Angular also supports block syntax:

```html
@if (condition) {
  <div>Content</div>
}
```

Your project currently uses classic `*ngIf`.

---

## 5.9 List Rendering: `*ngFor`

### Simple Explanation

`*ngFor` repeats HTML for each item in an array.

Syntax:

```html
<div *ngFor="let item of items">{{ item }}</div>
```

### Project Example: Interview Experience Levels

File:

```text
src/app/components/interviews/interview-start/interview-start.component.html
```

Code:

```html
<label class="choice-row" *ngFor="let level of experienceLevels">
```

TypeScript:

```ts
experienceLevels = ['Fresher', '1-3 years', '3-5 years', '5+ years'];
```

Meaning:

```text
Create one radio choice for each experience level.
```

### Project Example: Tech Skill Chips

HTML:

```html
<button
  type="button"
  class="skill-chip"
  *ngFor="let skill of techOptions"
  [class.selected]="form.techStack.includes(skill)"
  (click)="toggleTech(skill)"
>
  {{ skill }}
</button>
```

TypeScript:

```ts
techOptions = ['Angular', 'Node.js', 'Java', 'SQL', 'TypeScript', 'AWS', 'C#', '.NET'];
```

Meaning:

```text
Create one button chip for each tech skill.
Selected chips get selected class.
Clicking chip toggles it in the form.
```

### Project Example: Assessment Question Map

HTML:

```html
<button
  *ngFor="let q of questions; let i = index"
  class="map-btn"
  [class.active]="i === currentIndex"
  [class.answered]="answers[q.id]"
  (click)="goToQuestion(i)"
>
  {{ i + 1 }}
</button>
```

Meaning:

```text
Create one map button for each question.
Use i as question index.
Mark active button for current question.
Mark answered button if answer exists.
Click moves to that question.
```

### Project Example: Dashboard Stats

```html
<article class="stat-card" *ngFor="let item of stats; let index = index">
  <span class="label-caps">{{ item.label }}</span>
  <strong class="font-editorial">{{ index === 3 ? getPlanLabel(user) : item.value }}</strong>
</article>
```

Meaning:

```text
Create one stat card for each item in stats.
For the fourth card, show plan label instead of item.value.
```

### What If We Do Not Use `*ngFor`?

You would manually repeat HTML.

Bad:

```html
<button>Angular</button>
<button>Node.js</button>
<button>Java</button>
```

Better:

```html
<button *ngFor="let skill of techOptions">{{ skill }}</button>
```

Because if the array changes, the UI updates automatically.

### Alternative

Newer Angular supports:

```html
@for (item of items; track item.id) {
  <div>{{ item.name }}</div>
}
```

Your project currently uses classic `*ngFor`.

---

## 5.10 CSS Class Binding

### Simple Explanation

Class binding adds or removes a CSS class based on a condition.

Syntax:

```html
[class.class-name]="condition"
```

### Project Example: Premium Plan

File:

```text
src/app/components/shared/user-navbar/user-navbar.component.html
```

Code:

```html
<span class="plan-pill" [class.premium-active]="user.isPremium === true">
```

Meaning:

```text
Always use class plan-pill.
Also add class premium-active when user is premium.
```

### Project Example: Interview Type Selected

```html
<button
  type="button"
  class="type-card"
  *ngFor="let type of interviewTypes"
  [class.selected]="form.interviewType === type"
>
```

Meaning:

```text
Highlight the selected interview type.
```

### Project Example: Assessment Option Selected

```html
<div class="option-card" [class.selected]="answers[currentQuestion.id] === 'A'" (click)="selectOption('A')">
```

Meaning:

```text
If user's selected answer for current question is A, apply selected style to option A.
```

### Why Class Binding Is Useful

It lets the UI visually respond to state.

Examples:

- Selected option.
- Active question.
- Answered question.
- Premium status.
- Unread notification.
- Loading spinner.

### What If We Do Not Use Class Binding?

The user may not know:

- Which option is selected.
- Which page is active.
- Which question is answered.
- Whether they are premium.
- Which notification is unread.

---

## 5.11 Style Binding And Inline Styles

### Simple Explanation

Style binding changes an inline CSS style from TypeScript.

Syntax:

```html
[style.color]="textColor"
```

Your project mostly uses CSS classes instead of style binding, which is usually cleaner.

### Project Inline Style Example

File:

```text
src/app/components/interviews/interview-start/interview-start.component.html
```

Code:

```html
<span *ngIf="warmUpStatus === 'ready'" style="color: #4caf50;">Questions ready</span>
```

Meaning:

```text
Show this text in green when questions are ready.
```

### Better Alternative

Instead of inline style:

```html
style="color: #4caf50;"
```

you can use a CSS class:

```html
<span class="text-success">Questions ready</span>
```

Then define:

```css
.text-success {
  color: #4caf50;
}
```

### Why Classes Are Usually Better

CSS classes:

- Keep styling in CSS files.
- Are easier to reuse.
- Are easier to change globally.
- Keep HTML cleaner.

---

## 5.12 Router Links In Templates

### Simple Explanation

`routerLink` navigates inside Angular without doing a full browser page reload.

### Project Example: Navbar Links

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
Clicking these links changes Angular route without reloading the whole site.
routerLinkActive adds active class when current route matches.
```

### Project Example: Register Link

```html
<a routerLink="/login" class="login-link label-caps uppercase">
  ALREADY HAVE AN ACCOUNT? LOGIN
</a>
```

Meaning:

```text
Clicking this navigates to /login.
```

### Project Example: Button With RouterLink

```html
<button type="button" class="primary-action label-caps" routerLink="/interviews/start">
  Start Interview
</button>
```

Meaning:

```text
Button navigates to the interview start route.
```

### What If We Use Normal `href`?

Normal link:

```html
<a href="/login">Login</a>
```

can cause full page reload.

Angular link:

```html
<a routerLink="/login">Login</a>
```

uses Angular router and keeps SPA behavior.

### What Is Required For `routerLink`?

Component must import:

```ts
RouterModule
```

---

## 5.13 Pipes

### Simple Explanation

Pipes format values in templates.

Syntax:

```html
{{ value | pipeName }}
```

### Project Example: Async Pipe

File:

```text
src/app/components/shared/user-navbar/user-navbar.component.html
```

Code:

```html
*ngIf="currentUser$ | async as user"
```

Meaning:

```text
Subscribe to currentUser$ observable.
When a user value arrives, store it as local variable user.
Automatically unsubscribe when component is destroyed.
```

### Project Example: Notification Count

```html
<span class="unread-indicator" *ngIf="((unreadCount$ | async) ?? 0) > 0"></span>
```

Meaning:

```text
Read unreadCount$ observable.
If value is null/undefined, use 0.
Show indicator only if count is greater than 0.
```

### Project Example: Date Pipe

```html
<div class="note-time">{{ note.createdAt | date:'shortTime' }}</div>
```

Meaning:

```text
Format notification createdAt as a short time.
```

### Project Example: Number Pipe

File:

```text
src/app/components/assessments/assessment-session/assessment-session.component.html
```

Code:

```html
<div class="time">{{ timerMinutes | number:'2.0' }}:{{ timerSeconds | number:'2.0' }}</div>
```

Meaning:

```text
Show timer minutes and seconds with at least 2 digits.
```

Example:

```text
04:09
```

instead of:

```text
4:9
```

### Why Pipes Are Useful

Pipes keep formatting out of the TypeScript class.

Instead of writing formatting methods for every date/time/number, templates can use pipes.

### Alternatives

Alternatives:

- Format values in TypeScript methods.
- Custom pipes.
- Utility functions.

Pipes are preferred for template display formatting.

---

## 5.14 Template Reference Variables

### Simple Explanation

Template reference variables create a local variable inside HTML.

Syntax:

```html
#name="something"
```

### Project Example: Interview Form

File:

```text
src/app/components/interviews/interview-start/interview-start.component.html
```

Code:

```html
<form class="setup-form" #interviewForm="ngForm" (ngSubmit)="startInterview()">
```

Meaning:

```text
Create a local template variable named interviewForm.
It represents the ngForm object.
```

Later:

```html
[disabled]="isLoading || interviewForm.invalid"
```

Meaning:

```text
Disable the button if the form is loading or invalid.
```

### Why This Is Useful

The HTML can check form validity without needing extra TypeScript code.

### What If Reference Variable Is Wrong?

If you write:

```html
[disabled]="interviewForm.invalid"
```

but never define:

```html
#interviewForm="ngForm"
```

Angular will not know what `interviewForm` is.

---

## 5.15 `ng-template` And `else`

### Simple Explanation

`ng-template` defines a chunk of HTML that Angular can show conditionally.

### Project Example: User Dashboard Recent Interviews

File:

```text
src/app/components/user-dashboard/user-dashboard.component.html
```

Code:

```html
<a *ngIf="getStatusLink(row); else plainDomain" [routerLink]="row.link" class="row-link">{{ row.domain }}</a>
<ng-template #plainDomain>{{ row.domain }}</ng-template>
```

Meaning:

```text
If getStatusLink(row) is true, show clickable domain link.
Otherwise show plain domain text from plainDomain template.
```

Another example:

```html
<a *ngIf="getStatusLink(row); else plainStatus" [routerLink]="row.link" class="row-pill row-pill-link">{{ row.status }}</a>
<ng-template #plainStatus><span class="row-pill">{{ row.status }}</span></ng-template>
```

Meaning:

```text
If status should link somewhere, show clickable status.
Otherwise show plain status pill.
```

### Why This Is Useful

It avoids duplicating large blocks and keeps conditional UI cleaner.

### Alternative

Use two separate `*ngIf` blocks:

```html
<a *ngIf="getStatusLink(row)">...</a>
<span *ngIf="!getStatusLink(row)">...</span>
```

That works, but `else` is often cleaner.

---

## 5.16 Safe Handling Of Possibly Missing Values

### Simple Explanation

Sometimes data is not loaded yet.

Templates must avoid reading properties from `null` or `undefined`.

### Project Example: Async User

```html
<header class="dashboard-nav" *ngIf="currentUser$ | async as user">
```

This means navbar content only renders after `user` exists.

Inside this block, it is safer to use:

```html
{{ getDisplayName(user) }}
```

because `user` is available.

### Project Example: Nullish Coalescing

```html
*ngIf="((unreadCount$ | async) ?? 0) > 0"
```

Meaning:

```text
If async value is null or undefined, treat it as 0.
```

### Project Example: Optional Chaining

Register template:

```html
<div *ngIf="registerForm.errors?.['mismatch'] && registerForm.get('confirmPassword')?.touched">
```

Meaning:

```text
Safely check mismatch error and confirmPassword touched state.
If errors or control are missing, do not crash.
```

### Why This Matters

Frontend data often arrives later from API calls.

Without safe checks:

- Page can crash.
- Console errors appear.
- User sees blank or broken UI.

---

## 5.17 Loading, Empty, Success, And Error States

### Simple Explanation

Good templates show different UI for different states.

Common states:

| State | Meaning |
| --- | --- |
| Loading | Request/action is in progress |
| Empty | There is no data |
| Success | Action completed |
| Error | Something went wrong |

### Project Example: Login Loading

```html
<button 
  [disabled]="isLoading || loginForm.invalid"
  [class.is-loading]="isLoading"
>
  <span class="btn-text">LOGIN</span>
  <div class="loading-spinner" *ngIf="isLoading"></div>
</button>
```

Meaning:

```text
Disable button while loading.
Add loading CSS class.
Show spinner when loading.
```

### Project Example: Notification Empty State

```html
<div *ngIf="(notifications$ | async)?.length === 0" class="no-notifications">
  No notifications
</div>
```

Meaning:

```text
If notifications array is empty, show No notifications.
```

### Project Example: Interview Warmup State

```html
<p class="warmup-status label-caps" *ngIf="warmUpStatus">
  <span *ngIf="warmUpStatus === 'warming'" class="text-gold">Pre-generating questions...</span>
  <span *ngIf="warmUpStatus === 'ready'" style="color: #4caf50;">Questions ready</span>
  <span *ngIf="warmUpStatus === 'failed'" style="color: rgba(26,26,26,0.4);">Questions will generate on start</span>
</p>
```

Meaning:

```text
Show different warm-up messages depending on warmUpStatus.
```

### Project Example: Assessment Submit Loading

```html
<button class="submit-btn label-caps" (click)="submitAssessment()" [disabled]="isSubmitting">
  {{ isSubmitting ? 'SUBMITTING...' : 'SUBMIT ASSESSMENT' }}
</button>
```

Meaning:

```text
Disable submit button while submitting.
Change button text during submit.
```

### Why State UI Matters

Without these states:

- User may click repeatedly.
- User may not know if app is working.
- Errors may be invisible.
- Empty lists look broken.

---

## 5.18 Template Forms In This Project

### Reactive Forms

Used in:

```text
login.component.html
register.component.html
reset-password.component.html
```

Typical syntax:

```html
<form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
<input formControlName="email" />
```

### Template-Driven Forms

Used in:

```text
interview-start.component.html
assessment-start.component.html
```

Typical syntax:

```html
<form #interviewForm="ngForm" (ngSubmit)="startInterview()">
<input name="role" [(ngModel)]="form.role" required />
```

### Why Both Exist

Reactive forms are good for structured validation.

Template-driven forms are simple for beginner-friendly forms and direct object binding.

### Which One Should You Use?

For small/simple forms:

```text
Template-driven forms can be okay.
```

For bigger validation-heavy forms:

```text
Reactive forms are usually better.
```

---

## 5.19 What This Topic Is Doing In Your Project

Templates are where your project becomes visible to the user.

| Template Feature | Project Usage |
| --- | --- |
| Interpolation | Display names, errors, questions, scores, labels |
| Property binding | Disable buttons, dynamic input types, dynamic router links |
| Event binding | Button clicks, form submits, option selection |
| Two-way binding | Interview/assessment setup forms |
| Reactive form binding | Login/register/reset-password forms |
| `*ngIf` | Loading/error/user/admin/empty-state display |
| `*ngFor` | Lists, chips, stats, rows, question map |
| Class binding | Selected/active/unread/premium states |
| Pipes | Async observable values, dates, numbers |
| Router directives | Navigation between pages |
| `ng-template` | Conditional linked/plain table cells |

---

## 5.20 What If We Do Not Use Angular Binding?

Without Angular binding:

- HTML cannot easily read component data.
- Button clicks cannot easily call component methods.
- Forms cannot easily sync with TypeScript.
- Lists must be manually duplicated.
- Loading/error states become harder.
- UI may not update when state changes.

You would need manual DOM code like:

```js
document.querySelector(...)
element.addEventListener(...)
element.innerText = ...
```

Angular binding avoids that boilerplate.

---

## 5.21 Alternatives

### React

React uses JSX:

```jsx
<button disabled={isLoading} onClick={submit}>Submit</button>
```

### Vue

Vue uses directives:

```html
<button :disabled="isLoading" @click="submit">Submit</button>
```

### Plain JavaScript

Plain JS uses manual DOM operations:

```js
button.disabled = isLoading;
button.addEventListener('click', submit);
```

### Angular New Control Flow

New Angular supports:

```html
@if (errorMsg) {
  <div>{{ errorMsg }}</div>
}

@for (skill of techOptions; track skill) {
  <button>{{ skill }}</button>
}
```

Your project currently uses classic Angular template syntax:

```html
*ngIf
*ngFor
```

---

## 5.22 Common Template Mistakes

### Mistake 1: Forgetting Required Imports

If template uses:

```html
*ngIf
*ngFor
```

component needs:

```ts
CommonModule
```

If template uses:

```html
[(ngModel)]
```

component needs:

```ts
FormsModule
```

If template uses:

```html
[formGroup]
```

component needs:

```ts
ReactiveFormsModule
```

If template uses:

```html
routerLink
```

component needs:

```ts
RouterModule
```

### Mistake 2: Calling Heavy Methods In Template

Templates may call methods often during change detection.

This is okay for simple methods like:

```html
{{ getDisplayName(user) }}
```

But avoid expensive calculations in templates.

### Mistake 3: Missing `name` With `ngModel`

Template-driven forms need `name`:

```html
<input name="role" [(ngModel)]="form.role" />
```

Without `name`, Angular form tracking can fail.

### Mistake 4: Reading Data Before It Exists

Risky:

```html
{{ currentQuestion.text }}
```

if `currentQuestion` can be undefined.

Safer:

```html
<main *ngIf="currentQuestion">
  {{ currentQuestion.text }}
</main>
```

### Mistake 5: Using `href` Instead Of `routerLink`

For internal Angular routes, prefer:

```html
routerLink="/login"
```

instead of:

```html
href="/login"
```

### Mistake 6: Too Much Logic In HTML

Small expressions are okay.

Very complex logic should move to TypeScript methods or computed state.

---

## 5.23 File-By-File Template Study Checklist

When studying any `.html` file, ask:

1. Which component class owns this template?
2. Which properties from TypeScript are displayed with `{{ }}`?
3. Which buttons call methods with `(click)`?
4. Which forms call methods with `(ngSubmit)`?
5. Which fields use `[formGroup]`, `formControlName`, or `[(ngModel)]`?
6. Which elements are conditionally shown with `*ngIf`?
7. Which lists are repeated with `*ngFor`?
8. Which CSS classes are dynamic with `[class...]`?
9. Which pipes are used?
10. Which links navigate with `routerLink`?
11. Which UI states are handled: loading, error, empty, success?
12. What would break if one binding was removed?

---

## 5.24 Mini Exercise

Open:

```text
src/app/components/login/login.component.html
```

Answer:

1. Where is interpolation used?
2. Which button uses `[disabled]`?
3. Which form uses `(ngSubmit)`?
4. Which input uses dynamic `[type]`?
5. Which link uses `routerLink`?

Open:

```text
src/app/components/shared/user-navbar/user-navbar.component.html
```

Answer:

1. Where is the `async` pipe used?
2. Which links use `routerLinkActive`?
3. Which elements use `*ngIf`?
4. Which elements use `*ngFor`?
5. Which class bindings show premium/unread/clickable state?

Open:

```text
src/app/components/interviews/interview-start/interview-start.component.html
```

Answer:

1. Which inputs use `[(ngModel)]`?
2. Which arrays are rendered using `*ngFor`?
3. Which buttons call TypeScript methods?
4. Which button is disabled when form is invalid?
5. Which messages depend on `warmUpStatus`?

Open:

```text
src/app/components/assessments/assessment-session/assessment-session.component.html
```

Answer:

1. How is the timer formatted?
2. How is the question map created?
3. How does the selected answer get highlighted?
4. How does the modal appear?
5. How does the finish button appear only on the last question?

---

## 5.25 Interview-Style Questions

1. What is an Angular template?
2. What is interpolation?
3. What is property binding?
4. What is event binding?
5. What is two-way binding?
6. What is the difference between `[(ngModel)]` and `formControlName`?
7. What does `*ngIf` do?
8. What does `*ngFor` do?
9. What does `[class.selected]` do?
10. What is the `async` pipe used for?
11. What does `routerLink` do?
12. Why should internal navigation use `routerLink` instead of `href`?
13. What is a template reference variable?
14. Why do we use loading and error states in templates?
15. What happens if a template reads data before it is loaded?

---

## Topic 5 Summary

Angular templates are where component state becomes visible and user actions enter the app.

Most important mental model:

```text
{{ value }} displays data.
[property]="value" updates element properties.
(event)="method()" responds to user actions.
[(ngModel)]="value" syncs input and TypeScript both ways.
*ngIf shows or hides UI.
*ngFor repeats UI.
[class.name]="condition" changes visual state.
Pipes format or unwrap values.
routerLink navigates inside Angular.
```

In your project:

```text
Login/register templates handle forms and validation UI.
Navbar template shows user, plan, notifications, and navigation.
Interview start template uses ngModel, loops, chips, and event handlers.
Assessment session template uses question map, selected answers, timer pipes, and modal states.
Dashboard template uses async user data, repeated stat cards, tables, and conditional links.
```

Once you understand templates and binding, you can read any Angular HTML file and know how it connects to the TypeScript component.

---

## Next Topic

Recommended next lesson:

```text
Topic 6: Styling And Layout
```

Files to study next:

```text
src/styles.css
src/app/components/login/login.component.css
src/app/components/shared/user-navbar/user-navbar.component.css
src/app/components/user-dashboard/user-dashboard.component.css
src/app/components/interviews/interview-start/interview-start.component.css
src/app/components/assessments/assessment-session/assessment-session.component.css
```
