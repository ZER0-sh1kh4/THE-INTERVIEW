# Topic 6: Styling And Layout

This lesson explains CSS styling and layout in your Angular frontend using your actual project files.

Project frontend path:

```text
Frontend/
```

Main idea:

```text
HTML decides what appears, TypeScript decides what happens, and CSS decides how everything looks and fits on the screen.
```

In your project, CSS creates the visual identity of the website:

- Editorial typography.
- Premium gold/black/cream color system.
- Sticky navbars.
- Responsive grids.
- Form layouts.
- Dashboard cards and tables.
- Interview setup controls.
- Assessment session sidebar and question area.
- Loading, selected, active, error, and disabled states.

---

## 6.1 What Is CSS?

### Simple Explanation

CSS means Cascading Style Sheets.

CSS tells the browser how HTML should look.

HTML:

```html
<button>Login</button>
```

CSS:

```css
button {
  background: black;
  color: white;
  padding: 1rem;
}
```

Result:

```text
The plain button becomes a styled black button.
```

### Easy Scenario

Think of HTML as a person.

CSS is the clothing, hairstyle, posture, and presentation.

Without CSS, the person still exists, but there is no polished appearance.

### What CSS Is Doing In This Project

Your CSS is doing many jobs:

- Setting fonts.
- Setting colors.
- Creating layouts.
- Making pages responsive.
- Styling buttons.
- Styling forms.
- Styling navbars.
- Showing selected states.
- Showing loading states.
- Showing error states.
- Creating full-page dashboards and sessions.

### Why We Use CSS

Without CSS:

- Pages would look plain.
- Forms would be hard to scan.
- Buttons would not feel clickable.
- Layouts would stack awkwardly.
- Mobile experience would be poor.
- Dashboard and session screens would lose structure.

### Alternatives

Alternatives or additions to plain CSS:

- SCSS/Sass.
- Tailwind CSS.
- Bootstrap.
- Angular Material.
- CSS-in-JS in other frameworks.
- Design system libraries.

Your project currently uses plain CSS with CSS variables and component-level styles.

---

## 6.2 Global CSS: `src/styles.css`

File:

```text
src/styles.css
```

### Simple Explanation

Global CSS applies to the whole Angular app.

It is registered in:

```text
angular.json
```

as:

```json
"styles": ["src/styles.css"]
```

This means Angular loads `src/styles.css` for every page.

### What Global CSS Should Usually Contain

Global CSS is good for:

- Fonts.
- CSS variables.
- Body background.
- Common text utilities.
- Common button/link resets.
- Shared animation classes.
- Global layout helpers.

### Your Project Global CSS Starts With Font Imports

```css
@import url('https://fonts.googleapis.com/css2?family=Newsreader:ital,opsz,wght@0,6..72,200..800;1,6..72,200..800&family=Playfair+Display:ital,wght@0,400..900;1,400..900&family=Inter:wght@100..900&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap');
```

### What This Does

It loads fonts from Google Fonts:

- `Newsreader`
- `Playfair Display`
- `Inter`
- `Material Symbols Outlined`

Your project mostly uses:

```css
--font-editorial: 'Playfair Display', serif;
--font-body: 'Inter', sans-serif;
```

### Why Fonts Matter

Fonts set the personality of the website.

In your project:

- `Playfair Display` gives an editorial/premium feel.
- `Inter` gives clean readable body text.
- Material Symbols provide icons like `notifications`, `arrow_forward`, `timer`, `workspace_premium`.

### What If Fonts Fail To Load?

The app still works, but:

- Browser uses fallback fonts.
- Design looks different.
- Icons may not render correctly if Material Symbols fail.

---

## 6.3 CSS Variables: Design Tokens

### Simple Explanation

CSS variables store reusable style values.

They are also called design tokens.

Your project defines them inside:

```css
:root {
}
```

`:root` means the top-level document, so these variables are available everywhere.

### Your Project Variables

```css
:root {
  --color-primary: #1A1A1A;
  --color-secondary: #D4AF37;
  --color-background: #F9F8F6;
  --color-on-background: #1A1A1A;
  --color-surface: #F9F8F6;
  --color-outline: rgba(26, 26, 26, 0.1);
  --color-outline-strong: #747878;

  --font-editorial: 'Playfair Display', serif;
  --font-body: 'Inter', sans-serif;

  --spacing-gutter: 24px;
  --spacing-unit: 4px;
  --section-py-lg: 8rem;
  --section-py-md: 6rem;
}
```

### What Each Variable Means

| Variable | Meaning |
| --- | --- |
| `--color-primary` | Main dark text/background color |
| `--color-secondary` | Gold accent color |
| `--color-background` | Main page background |
| `--color-on-background` | Text color on background |
| `--color-surface` | Surface/card-like background |
| `--color-outline` | Light border/grid line |
| `--font-editorial` | Display/headline font |
| `--font-body` | Normal readable font |
| `--spacing-gutter` | Reusable grid gap |
| `--section-py-lg` | Large section vertical padding |

### How Variables Are Used

Example:

```css
body {
  background-color: var(--color-background);
  color: var(--color-on-background);
  font-family: var(--font-body);
}
```

Meaning:

```text
Use the global background color, global text color, and body font.
```

Example:

```css
.text-gold {
  color: var(--color-secondary);
}
```

Meaning:

```text
Use the project gold color.
```

### Why We Use CSS Variables

Variables help because:

- Colors stay consistent.
- Fonts stay consistent.
- Spacing stays consistent.
- One change updates many places.
- Design system becomes easier to maintain.

### What If We Do Not Use Variables?

You may repeat values everywhere:

```css
color: #D4AF37;
border-color: #D4AF37;
background: rgba(212, 175, 55, 0.14);
```

If the brand color changes, you must update many files manually.

### Alternative

Alternatives:

- SCSS variables.
- Tailwind config tokens.
- Design system token JSON.
- CSS custom properties from a theme file.

Your current CSS variables are simple and beginner-friendly.

---

## 6.4 Global Resets And Base Styles

### Box Sizing

Your global CSS:

```css
*, *::before, *::after {
  box-sizing: border-box;
}
```

### Simple Explanation

This makes width calculations easier.

Normally, if an element has:

```css
width: 100px;
padding: 20px;
border: 1px solid;
```

the final width can become more than `100px`.

With:

```css
box-sizing: border-box;
```

the padding and border are included inside the declared width.

### Why This Matters

It prevents layouts from unexpectedly overflowing.

### Body Base Style

```css
body {
  margin: 0;
  padding: 0;
  background-color: var(--color-background);
  color: var(--color-on-background);
  font-family: var(--font-body);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
```

### What It Does

```css
margin: 0;
padding: 0;
```

Removes browser default spacing.

```css
background-color: var(--color-background);
```

Sets the app background.

```css
color: var(--color-on-background);
```

Sets default text color.

```css
font-family: var(--font-body);
```

Sets default font to Inter.

### Button Reset

```css
button {
  background: transparent;
  border: none;
  cursor: pointer;
  font-family: inherit;
  color: inherit;
}
```

### Why This Exists

Browsers style buttons differently by default.

This reset makes buttons start from a clean base.

Then each component gives buttons their real design.

### Link Reset

```css
a {
  color: inherit;
  text-decoration: none;
}
```

This removes default blue underlined browser links.

Your component CSS then decides how links should look.

---

## 6.5 Global Utility Classes

### Simple Explanation

Utility classes are small reusable classes.

Your global CSS has utilities like:

```css
.font-editorial
.font-body
.label-caps
.vertical-label
.display-xl
.headline-lg
.body-lg
.italic
.uppercase
.text-center
```

### Example: `.font-editorial`

```css
.font-editorial {
  font-family: var(--font-editorial);
}
```

Used in templates:

```html
<h1 class="font-editorial">Define the <i>Parameters</i></h1>
```

Meaning:

```text
Use the premium editorial headline font.
```

### Example: `.label-caps`

```css
.label-caps {
  font-family: var(--font-body);
  font-size: 12px;
  line-height: 1.0;
  letter-spacing: 0.12em;
  font-weight: 600;
  text-transform: uppercase;
}
```

Used for small uppercase UI labels:

```html
<span class="label-caps">Step 01 / Setup</span>
```

### Why Utilities Are Useful

They avoid repeating common styles in every CSS file.

Instead of writing:

```css
font-size: 12px;
letter-spacing: 0.12em;
text-transform: uppercase;
```

everywhere, templates can use:

```html
class="label-caps"
```

### What If We Use Too Many Utilities?

HTML can become crowded with class names.

Balance is important:

- Use utilities for common text and layout helpers.
- Use component CSS for page-specific styling.

---

## 6.6 Material Symbols Icons

### Global Icon Setup

```css
.material-symbols-outlined {
  font-family: 'Material Symbols Outlined';
  font-variation-settings: 'FILL' 0, 'wght' 300, 'GRAD' 0, 'opsz' 24;
}
```

### How Icons Work In HTML

Example:

```html
<span class="material-symbols-outlined">notifications</span>
```

This displays the notification icon.

Other examples in your project:

```html
<span class="material-symbols-outlined">workspace_premium</span>
<span class="material-symbols-outlined">arrow_forward</span>
<span class="material-symbols-outlined">timer</span>
<span class="material-symbols-outlined">refresh</span>
```

### Why Icons Are Useful

Icons make actions easier to scan.

Examples:

- Bell means notifications.
- Arrow means move forward.
- Timer means remaining time.
- Premium badge means subscription.

### What If Icon Font Does Not Load?

Users may see text like:

```text
notifications
```

instead of the icon.

---

## 6.7 Component CSS

### Simple Explanation

Each component can have its own CSS file.

Example:

```text
src/app/components/login/login.component.css
```

is connected to:

```text
LoginComponent
```

through:

```ts
styleUrl: './login.component.css'
```

### Why Component CSS Exists

Component CSS keeps page-specific styling close to the component.

Examples:

| CSS File | Job |
| --- | --- |
| `login.component.css` | Login page grid/form/image styling |
| `user-navbar.component.css` | Navbar, account block, notifications |
| `user-dashboard.component.css` | Dashboard layout, stats, tables |
| `interview-start.component.css` | Setup form, chips, segments |
| `assessment-session.component.css` | Fullscreen session layout |

### What If All Styles Are In `styles.css`?

Problems:

- Global CSS becomes huge.
- Class names may conflict.
- Harder to know which styles belong to which page.
- Deleting old styles becomes risky.

### Alternative

Alternatives:

- SCSS partials.
- Shared CSS files per feature.
- Utility-first styling.
- UI component libraries.

Your project uses global foundations plus component-specific CSS, which is a good learning structure.

---

## 6.8 `:host` In Component CSS

### Simple Explanation

`:host` targets the component's own outer element.

It means:

```text
Style this component as a whole.
```

### Project Example: User Dashboard

File:

```text
src/app/components/user-dashboard/user-dashboard.component.css
```

Code:

```css
:host {
  display: block;
  min-height: 100vh;
  background:
    linear-gradient(90deg, rgba(26, 26, 26, 0.035) 1px, transparent 1px) 0 0 / 8.333% 100%,
    var(--color-background);
  color: var(--color-primary);
}
```

### What It Does

```css
display: block;
```

Makes the component behave like a block-level page.

```css
min-height: 100vh;
```

Makes it at least full browser height.

```css
background: linear-gradient(...), var(--color-background);
```

Creates subtle vertical grid lines over the background.

```css
color: var(--color-primary);
```

Sets default text color inside this component.

### Project Example: Assessment Session

```css
:host {
  display: block;
  height: 100vh;
  width: 100vw;
  background: var(--color-background);
  color: var(--color-primary);
  overflow: hidden;
}
```

### Why This Is Different

Assessment session is a full-screen focused test environment.

It uses:

```css
height: 100vh;
width: 100vw;
overflow: hidden;
```

so the layout fills the viewport and controls scrolling inside specific areas.

### What If We Do Not Use `:host`?

You can still style child elements, but the whole component page may not get:

- Full height.
- Background.
- Global page color.
- Overflow behavior.

---

## 6.9 Layout With Flexbox

### Simple Explanation

Flexbox arranges items in one direction:

- Row.
- Column.

It is good for navbars, button rows, cards, and centered layouts.

### Project Example: Navbar

File:

```text
src/app/components/shared/user-navbar/user-navbar.component.css
```

Code:

```css
.dashboard-nav {
  position: sticky;
  top: 0;
  z-index: 20;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 2rem;
  padding: 1.25rem 2rem;
}
```

### What It Does

```css
display: flex;
```

Makes navbar children sit in a flexible row.

```css
align-items: center;
```

Vertically aligns brand, links, and account block.

```css
justify-content: space-between;
```

Pushes left, middle, and right areas apart.

```css
gap: 2rem;
```

Adds spacing between sections.

### Project Example: Account Block

```css
.account-block {
  gap: 1rem;
}
```

and:

```css
.brand-block,
.account-block,
.dashboard-links,
.plan-pill {
  display: flex;
  align-items: center;
}
```

Meaning:

```text
Brand, account controls, links, and plan pill all align their children horizontally.
```

### Project Example: Assessment Session

```css
.session-layout {
  display: flex;
  height: 100%;
  width: 100%;
}
```

This creates:

```text
Sidebar on the left + question area on the right.
```

### Why Flexbox Is Used

Flexbox is good when content should flow in a row or column and align cleanly.

### What If We Do Not Use Flexbox?

You may need older layout tricks:

- Floats.
- Inline-block.
- Manual positioning.

Those are harder to maintain.

---

## 6.10 Layout With CSS Grid

### Simple Explanation

CSS Grid is best for two-dimensional layouts:

- Rows and columns.
- Dashboards.
- Forms.
- Card grids.
- Question maps.

### Project Example: Dashboard Stats

```css
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  border-top: 1px solid rgba(26, 26, 26, 0.18);
  border-bottom: 1px solid rgba(26, 26, 26, 0.18);
}
```

### What It Does

```css
display: grid;
```

Turns the section into a grid.

```css
grid-template-columns: repeat(4, minmax(0, 1fr));
```

Creates four equal columns.

Each stat card takes one column.

### Project Example: Activity Grid

```css
.activity-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 4rem;
  padding-top: 4rem;
}
```

Meaning:

```text
Recent Interviews and Recent Assessments appear side by side on large screens.
```

### Project Example: Interview Setup Form

```css
.setup-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 4rem 3rem;
}
```

Meaning:

```text
The interview setup form uses two columns.
Some fields can span the full width.
```

Full-width fields:

```css
.field-wide {
  grid-column: 1 / -1;
}
```

Meaning:

```text
Start at first grid line and end at last grid line.
Take full row width.
```

### Project Example: Question Map

```css
.map-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 0.6rem;
}
```

Meaning:

```text
Show question number buttons in five equal columns.
```

### Why Grid Is Used

Grid makes structured layouts easier.

### What If We Do Not Use Grid?

Dashboard and form layout would be harder to align.

You might need many wrappers, widths, floats, or manual positioning.

---

## 6.11 Sticky Navbar

### Project Code

```css
.dashboard-nav {
  position: sticky;
  top: 0;
  z-index: 20;
  background: rgba(249, 248, 246, 0.92);
  backdrop-filter: blur(12px);
}
```

### Simple Explanation

`position: sticky` keeps the navbar at the top when scrolling.

```css
top: 0;
```

means it sticks to the top edge.

```css
z-index: 20;
```

keeps it above normal page content.

```css
background: rgba(...);
backdrop-filter: blur(12px);
```

creates a slightly transparent blurred background.

### What This Does In The Website

When users scroll dashboard/user pages:

- Navbar remains visible.
- Brand and navigation are easy to access.
- Background blur keeps content readable behind it.

### What If Navbar Is Not Sticky?

Users must scroll back to the top to navigate.

This is annoying on long dashboard/history/admin pages.

### Alternative

Alternative navbar positioning:

- `position: fixed`
- Normal static navbar
- Sidebar navigation
- Mobile bottom navigation

Sticky is a good balance because it stays in document flow until it reaches the top.

---

## 6.12 Responsive Design With Media Queries

### Simple Explanation

Responsive design means the layout adapts to screen size.

Media queries apply CSS only under certain screen widths.

### Project Example: Dashboard

```css
@media (max-width: 1080px) {
  .dashboard-nav {
    align-items: flex-start;
    flex-direction: column;
  }

  .stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .activity-grid {
    grid-template-columns: 1fr;
  }
}
```

### What It Does

When screen width is 1080px or less:

- Navbar stacks vertically.
- Stats grid changes from 4 columns to 2 columns.
- Activity panels change from 2 columns to 1 column.

### Project Example: Small Mobile Dashboard

```css
@media (max-width: 680px) {
  .dashboard-shell {
    padding: 3rem 1rem 4rem;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .quick-actions button {
    width: 100%;
    justify-content: center;
  }
}
```

### What It Does

On small screens:

- Page padding becomes smaller.
- Stats become one column.
- Quick action buttons become full width.

### Project Example: Interview Setup

```css
@media (max-width: 900px) {
  .setup-form,
  .split-row {
    grid-template-columns: 1fr;
  }
}
```

Meaning:

```text
The two-column interview setup form becomes one column on smaller screens.
```

### Project Example: Assessment Session

```css
@media (max-width: 1100px) {
  .options-list {
    grid-template-columns: 1fr;
  }
}
```

Meaning:

```text
Answer options change from two columns to one column.
```

### Why Responsive Design Matters

Users may open the website on:

- Laptop.
- Desktop.
- Tablet.
- Mobile.

Without responsive CSS:

- Text may overflow.
- Tables may become unreadable.
- Buttons may be too small.
- Forms may go off-screen.
- Navbars may not fit.

---

## 6.13 Form Styling

### Project Example: Interview Input

```css
.editorial-input {
  width: 100%;
  border: 0;
  border-bottom: 1px solid rgba(26, 26, 26, 0.3);
  padding: 1rem 0;
  background: transparent;
  font-family: var(--font-editorial);
  font-size: 2rem;
  outline: none;
}
```

### What It Does

```css
width: 100%;
```

Input fills available width.

```css
border: 0;
border-bottom: 1px solid ...;
```

Removes full input box and keeps only underline style.

```css
background: transparent;
```

Lets page background show through.

```css
font-family: var(--font-editorial);
font-size: 2rem;
```

Makes input feel large and editorial.

### Focus State

```css
.editorial-input:focus,
.custom-skill input:focus,
.select-wrap select:focus {
  border-color: var(--color-secondary);
}
```

Meaning:

```text
When user focuses the input/select, underline changes to gold.
```

### Why Focus Style Matters

Focus style tells the user where they are typing.

It also helps keyboard users.

### Project Example: Error Box

```css
.error-box {
  border-color: rgba(186, 26, 26, 0.35);
  color: #ba1a1a;
  background: rgba(186, 26, 26, 0.05);
}
```

Meaning:

```text
Error messages appear red and visually separate from normal content.
```

### Project Example: Info Box

```css
.info-box {
  border-color: rgba(212, 175, 55, 0.35);
  color: var(--color-secondary);
  background: rgba(212, 175, 55, 0.08);
}
```

Meaning:

```text
Informational status messages use the brand gold color.
```

---

## 6.14 Button And Interactive State Styling

### Disabled Button

```css
.start-button:disabled,
.nav-start:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
```

### Meaning

When button is disabled:

- It becomes faded.
- Cursor shows it cannot be clicked.

This connects to template binding:

```html
[disabled]="isLoading || interviewForm.invalid"
```

### Selected Button

```css
.type-card.selected {
  border: 2px solid var(--color-secondary);
  background: rgba(212, 175, 55, 0.05);
}
```

Template:

```html
[class.selected]="form.interviewType === type"
```

Meaning:

```text
Selected interview type gets gold border and light gold background.
```

### Skill Chip Selected

```css
.skill-chip.selected {
  border-color: var(--color-secondary);
  background: rgba(212, 175, 55, 0.12);
}
```

Template:

```html
[class.selected]="form.techStack.includes(skill)"
```

Meaning:

```text
Selected tech skills are visually highlighted.
```

### Assessment Option Selected

```css
.option-card.selected {
  border: 2px solid var(--color-secondary);
  background: rgba(212, 175, 55, 0.05);
  box-shadow: 0 4px 20px rgba(212, 175, 55, 0.1);
}
```

Template:

```html
[class.selected]="answers[currentQuestion.id] === 'A'"
```

Meaning:

```text
Selected answer option is highlighted.
```

### Why Interactive States Matter

They answer user questions visually:

- Can I click this?
- Did I select this?
- Is this active?
- Is the app loading?
- Is this disabled?

Without these states, users feel lost.

---

## 6.15 Animation And Hover Effects

### Global Hover Animation

File:

```text
src/styles.css
```

Code:

```css
.gold-hover-slide {
  position: relative;
  overflow: hidden;
  z-index: 1;
  transition: color 400ms cubic-bezier(0.4, 0, 0.2, 1);
}
```

```css
.gold-hover-slide::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background-color: var(--color-secondary);
  transition: left 400ms cubic-bezier(0.4, 0, 0.2, 1);
  z-index: -1;
}
```

```css
.gold-hover-slide:hover::before {
  left: 0;
}
```

### Simple Explanation

This creates a gold background slide animation on hover.

The `::before` pseudo-element starts outside the button:

```css
left: -100%;
```

On hover, it slides in:

```css
left: 0;
```

### Project Usage

Buttons use:

```html
class="btn-submit gold-hover-slide"
```

### Loading Spin Animation

Interview start CSS:

```css
.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
```

Template:

```html
<span class="material-symbols-outlined" [class.spin]="isLoading">refresh</span>
```

Meaning:

```text
When isLoading is true, refresh icon spins.
```

### Why Animation Is Useful

Animation gives feedback:

- Button hover feels interactive.
- Loading icon shows progress.
- Hover states make clickable elements discoverable.

### What If Animations Are Overused?

The app can feel distracting or slow.

Animations should support usability, not fight it.

---

## 6.16 Tables And Dashboard Layout

### Dashboard Shell

```css
.dashboard-shell {
  max-width: 1440px;
  margin: 0 auto;
  padding: 5rem 2rem 6rem;
}
```

### Meaning

```css
max-width: 1440px;
```

Keeps content from becoming too wide.

```css
margin: 0 auto;
```

Centers the content.

```css
padding: 5rem 2rem 6rem;
```

Adds breathing room around content.

### Table Styling

```css
table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}
```

```css
th {
  padding: 0 0 1.25rem;
  border-bottom: 2px solid var(--color-primary);
  font-size: 0.72rem;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}
```

```css
td {
  padding: 1.5rem 0;
  border-bottom: 1px solid var(--color-outline);
  color: rgba(26, 26, 26, 0.72);
}
```

### What It Does

Tables become:

- Full width.
- Cleanly aligned.
- Separated with borders.
- Easy to scan.

### Why Tables Are Used

Dashboard recent interviews and assessments are tabular data:

- Domain.
- Status.
- Date.
- Grade.
- Score.

Tables make comparison easier.

---

## 6.17 Full-Screen Assessment Session Layout

File:

```text
src/app/components/assessments/assessment-session/assessment-session.component.css
```

### Main Structure

```css
.session-layout {
  display: flex;
  height: 100%;
  width: 100%;
}
```

This creates a two-panel layout.

### Sidebar

```css
.sidebar {
  width: 300px;
  min-width: 300px;
  background: rgba(26, 26, 26, 0.03);
  border-right: 1px solid rgba(26, 26, 26, 0.1);
  display: flex;
  flex-direction: column;
  padding: 2rem;
}
```

### What Sidebar Does

It holds:

- Brand.
- Timer.
- Question map.
- Submit button.

Fixed width keeps it stable.

### Main Question Area

```css
.question-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 3rem 4rem;
  overflow-y: auto;
}
```

### What It Does

```css
flex: 1;
```

Takes all remaining space after sidebar.

```css
overflow-y: auto;
```

Allows question content to scroll if needed.

### Why This Layout Is Good For Assessment

The user always sees:

- Timer.
- Question map.
- Submit button.

while the main area focuses on the current question.

### What If Everything Was One Column?

The user would need to scroll to see timer or question map.

That is worse for exam/session UX.

---

## 6.18 Modal Styling

### Project Example: Warning Modal

```css
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(26, 26, 26, 0.9);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
```

### What It Does

```css
position: fixed;
inset: 0;
```

Covers the whole screen.

```css
background: rgba(26, 26, 26, 0.9);
```

Creates dark overlay.

```css
display: flex;
align-items: center;
justify-content: center;
```

Centers the modal.

```css
z-index: 1000;
```

Keeps it above everything.

### Modal Box

```css
.warning-modal {
  background: var(--color-background);
  padding: 4rem;
  text-align: center;
  max-width: 500px;
  border: 1px solid var(--color-secondary);
}
```

### Why Modal Styling Matters

A modal must clearly interrupt the current flow.

It should:

- Cover background.
- Focus attention.
- Be readable.
- Have clear action button.

---

## 6.19 CSS And Angular Bindings Working Together

CSS and Angular templates work as a team.

### Example: Selected Interview Type

Template:

```html
[class.selected]="form.interviewType === type"
```

CSS:

```css
.type-card.selected {
  border: 2px solid var(--color-secondary);
  background: rgba(212, 175, 55, 0.05);
}
```

Result:

```text
When TypeScript state says this type is selected, CSS shows selected styling.
```

### Example: Disabled Start Button

Template:

```html
[disabled]="isLoading || interviewForm.invalid"
```

CSS:

```css
.start-button:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
```

Result:

```text
Angular disables button. CSS makes disabled state visible.
```

### Example: Active Navbar Link

Template:

```html
routerLinkActive="active"
```

CSS:

```css
.dashboard-links .active {
  color: var(--color-primary);
  border-bottom: 2px solid var(--color-primary);
  padding-bottom: 0.35rem;
}
```

Result:

```text
Current page link gets active styling.
```

### Why This Matters

Angular decides state.

CSS expresses state visually.

---

## 6.20 Common CSS Mistakes

### Mistake 1: Hardcoding Repeated Colors

Bad:

```css
color: #D4AF37;
```

Better:

```css
color: var(--color-secondary);
```

### Mistake 2: Not Handling Mobile Layout

If there are no media queries:

- Grids overflow.
- Navbar becomes cramped.
- Tables become hard to read.

### Mistake 3: Missing Disabled State

If button is disabled but looks normal, user gets confused.

### Mistake 4: Overusing Inline Styles

Inline:

```html
style="color: #4caf50;"
```

Better:

```css
.text-success {
  color: #4caf50;
}
```

### Mistake 5: Too Much Global CSS

Global CSS can accidentally affect many pages.

Put page-specific styles in component CSS.

### Mistake 6: Forgetting Overflow

Full-screen layouts need careful overflow handling.

Example:

```css
.question-area {
  overflow-y: auto;
}
```

Without it, content may become inaccessible.

### Mistake 7: Weak Visual States

Selected, active, loading, disabled, error, and empty states should be visually clear.

---

## 6.21 What This Topic Is Doing In Your Project

| CSS Area | Project Usage |
| --- | --- |
| Global CSS variables | Brand colors, fonts, spacing |
| Global font imports | Editorial headings and readable body text |
| Button/link resets | Consistent base styling |
| Utility classes | Reusable text styles |
| Component CSS | Page-specific layout and appearance |
| Flexbox | Navbars, account blocks, session layout |
| Grid | Dashboard stats, activity panels, forms, question map |
| Media queries | Mobile/tablet responsiveness |
| Class states | Selected, active, premium, answered, unread |
| Disabled styles | Prevent confusing inactive buttons |
| Modal styles | Warning overlay in assessment session |
| Animation | Hover slide and loading spin |

---

## 6.22 What If We Do Not Use This Styling Structure?

Without global variables:

```text
Design becomes inconsistent and hard to update.
```

Without component CSS:

```text
Styles become mixed and risky to change.
```

Without responsive CSS:

```text
Mobile and tablet layouts break.
```

Without visual states:

```text
Users cannot easily understand what is selected, active, disabled, or loading.
```

Without layout systems:

```text
Dashboard, forms, and sessions become poorly aligned.
```

---

## 6.23 Alternatives

### SCSS

SCSS adds variables, nesting, mixins, and functions.

Angular supports SCSS if configured.

### Tailwind CSS

Tailwind uses utility classes directly in HTML.

Example:

```html
<button class="px-4 py-2 bg-black text-white">
```

Good for fast styling, but HTML can become class-heavy.

### Bootstrap

Bootstrap gives ready-made grid, buttons, forms, navbars, and modals.

Good for quick admin-style apps.

### Angular Material

Angular Material gives Angular-native UI components.

Good for dashboards, forms, tables, dialogs, and accessibility.

### Design System

A design system creates reusable components and tokens.

Useful when the app grows larger.

Your current custom CSS gives the project a distinctive editorial visual identity.

---

## 6.24 File-By-File Styling Study Checklist

When studying any CSS file, ask:

1. Is this global CSS or component CSS?
2. Which HTML file uses these class names?
3. Which colors come from CSS variables?
4. Which layout system is used: flex, grid, position, or normal flow?
5. Which elements are responsive?
6. Which media queries exist?
7. Which classes represent states like active, selected, disabled, unread, answered?
8. Which classes style buttons?
9. Which classes style forms?
10. Which classes control spacing?
11. Which classes control typography?
12. What would break visually if this CSS block was removed?

---

## 6.25 Mini Exercise

Open:

```text
src/styles.css
```

Answer:

1. What are the main project colors?
2. What are the two main fonts?
3. What does `.label-caps` do?
4. What does `.gold-hover-slide` do?
5. Why are button and link resets used?

Open:

```text
src/app/components/shared/user-navbar/user-navbar.component.css
```

Answer:

1. Why is `.dashboard-nav` sticky?
2. What makes the notification panel appear above other content?
3. How does unread notification styling work?
4. What changes at `max-width: 1080px`?

Open:

```text
src/app/components/user-dashboard/user-dashboard.component.css
```

Answer:

1. How many columns does `.stats-grid` have on desktop?
2. What happens to `.stats-grid` on mobile?
3. How are tables styled?
4. What does `.dashboard-shell` do?

Open:

```text
src/app/components/interviews/interview-start/interview-start.component.css
```

Answer:

1. What does `.setup-form` grid do?
2. What does `.field-wide` do?
3. How is selected interview type styled?
4. How is the loading spin animation created?

Open:

```text
src/app/components/assessments/assessment-session/assessment-session.component.css
```

Answer:

1. How is the sidebar width controlled?
2. Why does `.question-area` use `flex: 1`?
3. How are selected options styled?
4. How does the warning modal cover the whole screen?

---

## 6.26 Interview-Style Questions

1. What is the difference between global CSS and component CSS?
2. What are CSS variables?
3. Why does this project use `:root` variables?
4. What is `:host` in Angular component CSS?
5. What is the difference between Flexbox and CSS Grid?
6. Why is `position: sticky` used for the navbar?
7. What are media queries?
8. Why are disabled states important?
9. How does Angular class binding work with CSS?
10. Why should repeated colors use variables?
11. What does `box-sizing: border-box` do?
12. Why should internal page-specific styles usually stay in component CSS?
13. What does `z-index` do in navbar/modal/notification UI?
14. Why does the assessment session use a full-screen layout?
15. What happens if responsive CSS is missing?

---

## Topic 6 Summary

CSS is what turns your Angular components into a polished website.

Most important mental model:

```text
Global CSS defines the design system.
Component CSS defines page-specific layout and styling.
CSS variables keep colors/fonts/spacing consistent.
Flexbox arranges items in rows or columns.
Grid creates structured columns and rows.
Media queries make layouts responsive.
Angular bindings add/remove classes.
CSS turns those classes into visible states.
```

In your project:

```text
styles.css defines the global visual language.
user-navbar CSS creates sticky navigation and notification dropdowns.
user-dashboard CSS creates candidate dashboard grids and tables.
interview-start CSS creates the setup form, chips, and selected states.
assessment-session CSS creates the full-screen test layout, sidebar, options, and warning modal.
```

Once you understand CSS in this project, you can visually trace why each page looks the way it does and how Angular state becomes visual feedback.

---

## Next Topic

Recommended next lesson:

```text
Topic 7: Routing And Navigation
```

Files to study next:

```text
src/app/app.routes.ts
src/app/app.component.html
src/app/guards/auth.guard.ts
src/app/guards/admin.guard.ts
src/app/components/shared/user-navbar/user-navbar.component.html
src/app/components/interviews/interview-start/interview-start.component.ts
```
