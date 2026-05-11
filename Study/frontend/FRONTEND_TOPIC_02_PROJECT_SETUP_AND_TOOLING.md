# Topic 2: Project Setup And Tooling

This lesson explains the setup files and tools that make your Angular frontend run, build, test, and connect to the backend.

Project frontend path:

```text
Frontend/
```

Main idea:

```text
The setup files are the control room of the Angular project. They tell Angular what packages to use, how to start the app, how to build it, how to test it, and where the backend API is.
```

---

## 2.1 What Is Project Setup?

### Simple Explanation

Project setup means all the files and tools needed before writing or running application code.

If components, services, and routes are the actual app, then setup files are the machinery that lets the app work.

### Easy Scenario

Imagine you want to open your Angular website.

You type:

```bash
npm start
```

But how does the computer know what `npm start` means?

It reads:

```text
package.json
```

Then it finds:

```json
"start": "ng serve"
```

So it runs Angular's development server.

That is project setup in action.

### What Setup Is Doing In This Project

Your frontend setup is mainly controlled by these files:

```text
package.json
angular.json
tsconfig.json
tsconfig.app.json
tsconfig.spec.json
src/environments/environment.ts
```

These files answer questions like:

- Which Angular version is used?
- Which commands can we run?
- Where is the app entry file?
- Which CSS file is global?
- Which TypeScript files are compiled?
- Where is the backend API?
- How should production build behave?
- How should tests run?

### Why We Need Setup Files

Without setup files:

- Angular would not know how to build the project.
- npm would not know which commands are available.
- TypeScript would not know which rules to follow.
- The frontend would not know the backend API URL.
- Developers would run the project in different and inconsistent ways.

### Alternative Ways

Other frontend projects may use:

- `vite.config.ts` for Vite projects.
- `webpack.config.js` for custom Webpack projects.
- `next.config.js` for Next.js projects.
- `nuxt.config.ts` for Nuxt projects.
- `.env` files for environment variables.

Angular commonly uses `package.json`, `angular.json`, and `tsconfig` files.

---

## 2.2 `package.json`

File:

```text
package.json
```

### Simple Explanation

`package.json` is the project identity card and command list.

It tells npm:

- Project name.
- Project version.
- Scripts/commands.
- Packages needed to run the app.
- Packages needed only while developing.
- Package manager version.

### Your Project Code

```json
{
  "name": "frontend",
  "version": "0.0.0",
  "scripts": {
    "ng": "ng",
    "start": "ng serve",
    "build": "ng build",
    "watch": "ng build --watch --configuration development",
    "test": "ng test"
  },
  "private": true,
  "packageManager": "npm@11.3.0"
}
```

### Line-By-Line Explanation

```json
"name": "frontend"
```

This is the project name. Here, the frontend project is simply called `frontend`.

```json
"version": "0.0.0"
```

This is the version of the frontend package. It is currently an initial/default version.

```json
"scripts"
```

Scripts are shortcut commands.

Instead of typing the full Angular command every time, you can type npm commands.

```json
"ng": "ng"
```

Lets you run Angular CLI commands through npm.

```json
"start": "ng serve"
```

Starts the development server.

When you run:

```bash
npm start
```

Angular runs:

```bash
ng serve
```

This opens the frontend locally, usually at:

```text
http://localhost:4200
```

```json
"build": "ng build"
```

Builds the app for deployment.

It converts TypeScript, HTML, and CSS into optimized files inside the build output folder.

```json
"watch": "ng build --watch --configuration development"
```

Builds the app in development mode and keeps watching for file changes.

```json
"test": "ng test"
```

Runs frontend tests.

```json
"private": true
```

Means this project is not intended to be published as a public npm package.

```json
"packageManager": "npm@11.3.0"
```

Says this project expects npm version `11.3.0`.

### Dependencies

Your project dependencies:

```json
"dependencies": {
  "@angular/common": "^21.2.0",
  "@angular/compiler": "^21.2.0",
  "@angular/core": "^21.2.0",
  "@angular/forms": "^21.2.0",
  "@angular/platform-browser": "^21.2.0",
  "@angular/router": "^21.2.0",
  "rxjs": "~7.8.0",
  "tslib": "^2.3.0"
}
```

Dependencies are packages needed for the actual app to work.

### What Each Main Dependency Does

| Package | Purpose In Simple Words | Where Your Project Uses It |
| --- | --- | --- |
| `@angular/core` | Main Angular engine | Components, services, dependency injection |
| `@angular/common` | Common Angular features | `CommonModule`, `*ngIf`, `*ngFor`, pipes |
| `@angular/compiler` | Compiles Angular templates | Build process |
| `@angular/forms` | Form handling | Login, register, interview, assessment, profile forms |
| `@angular/platform-browser` | Runs Angular in browser | `src/main.ts` |
| `@angular/router` | Page navigation | `src/app/app.routes.ts` |
| `rxjs` | Async data streams | Services, guards, API calls |
| `tslib` | TypeScript helper functions | Generated compiled code |

### Dev Dependencies

Your project dev dependencies:

```json
"devDependencies": {
  "@angular/build": "^21.2.8",
  "@angular/cli": "^21.2.8",
  "@angular/compiler-cli": "^21.2.0",
  "jsdom": "^28.0.0",
  "prettier": "^3.8.1",
  "typescript": "~5.9.2",
  "vitest": "^4.0.8"
}
```

Dev dependencies are packages needed while developing, building, or testing. They are not the main runtime logic of the app.

| Package | Purpose |
| --- | --- |
| `@angular/build` | Angular build system |
| `@angular/cli` | Angular command-line tool |
| `@angular/compiler-cli` | Angular compiler for TypeScript builds |
| `jsdom` | Browser-like environment for tests |
| `prettier` | Code formatter |
| `typescript` | TypeScript compiler |
| `vitest` | Test runner |

### Why We Use `package.json`

We use it because:

- Everyone can install the same packages.
- Everyone can run the same commands.
- Angular CLI knows what tools are available.
- Build/test/start scripts are standardized.

### What If We Do Not Have `package.json`?

Without `package.json`:

- `npm install` would not know what to install.
- `npm start` would not exist.
- Angular packages may be missing.
- Different developers may use different package versions.
- The project may not run.

### Alternatives

The file `package.json` is standard for JavaScript projects. Alternatives are not really replacements, but other package managers can use it:

- npm
- pnpm
- yarn
- bun

---

## 2.3 `node_modules` And `package-lock.json`

### Simple Explanation

`node_modules` is the folder where installed packages live.

`package-lock.json` records the exact versions installed.

### Easy Scenario

`package.json` says:

```text
I need Angular router version around 21.2.0.
```

`package-lock.json` says:

```text
The exact installed version is this specific one, with these exact sub-dependencies.
```

`node_modules` contains the actual files.

### What This Project Has

Your project has:

```text
node_modules/
package-lock.json
```

That means dependencies are already installed locally.

### Why We Need Them

We need `node_modules` to run commands like:

```bash
npm start
npm run build
npm test
```

We need `package-lock.json` so installs stay consistent.

### What If We Delete `node_modules`?

The app will not run until you install packages again:

```bash
npm install
```

### What If We Delete `package-lock.json`?

npm can recreate it, but versions may shift slightly. That can sometimes create new bugs.

### Alternative Ways

Other package managers create different lock files:

- `yarn.lock`
- `pnpm-lock.yaml`
- `bun.lock`

---

## 2.4 Angular CLI

### Simple Explanation

Angular CLI is Angular's command-line helper.

CLI means Command Line Interface.

It gives commands like:

```bash
ng serve
ng build
ng test
ng generate component
ng generate service
```

### What Angular CLI Does In This Project

Your `package.json` uses Angular CLI commands:

```json
"start": "ng serve",
"build": "ng build",
"test": "ng test"
```

So when you run npm scripts, Angular CLI does the actual work.

### Easy Scenario

If you type:

```bash
npm run build
```

npm reads `package.json`.

Then it runs:

```bash
ng build
```

Then Angular CLI reads:

```text
angular.json
```

Then it builds your app using the settings inside `angular.json`.

### Why We Use Angular CLI

Angular CLI helps with:

- Running local development server.
- Building production files.
- Generating components/services.
- Running tests.
- Managing Angular project conventions.

### What If We Do Not Use Angular CLI?

Without Angular CLI:

- We would need to manually configure build tools.
- We would need custom TypeScript compilation.
- We would need custom dev server setup.
- Generating files would be slower.
- Project structure may become inconsistent.

### Alternatives

Alternatives:

- Custom Vite setup.
- Custom Webpack setup.
- Nx workspace tooling.
- Manually configured TypeScript build.

For Angular apps, Angular CLI is the standard path.

---

## 2.5 `angular.json`

File:

```text
angular.json
```

### Simple Explanation

`angular.json` is Angular's project configuration file.

It tells Angular:

- This is an application.
- Source files are inside `src`.
- App starts from `src/main.ts`.
- Global styles are in `src/styles.css`.
- Assets are in `public`.
- How to build.
- How to serve.
- How to test.

### Important Project Configuration

```json
"projects": {
  "frontend": {
    "projectType": "application",
    "root": "",
    "sourceRoot": "src",
    "prefix": "app"
  }
}
```

### Line-By-Line Explanation

```json
"projectType": "application"
```

This says the project is an Angular app, not a library.

```json
"root": ""
```

The project root is the current `Frontend` folder.

```json
"sourceRoot": "src"
```

The app source code lives in:

```text
src/
```

```json
"prefix": "app"
```

Angular component selectors usually start with `app`.

Example:

```ts
selector: 'app-login'
```

### Schematics

Your `angular.json` has:

```json
"schematics": {
  "@schematics/angular:component": {
    "type": "component",
    "addTypeToClassName": false
  }
}
```

Schematics control how Angular generates new files.

### Easy Scenario

If you generate a new component:

```bash
ng generate component components/example
```

Angular CLI uses schematic settings to decide naming style and file structure.

### Build Section

Important part:

```json
"build": {
  "builder": "@angular/build:application",
  "options": {
    "browser": "src/main.ts",
    "tsConfig": "tsconfig.app.json",
    "assets": [
      {
        "glob": "**/*",
        "input": "public"
      }
    ],
    "styles": ["src/styles.css"]
  }
}
```

### Build Line-By-Line

```json
"builder": "@angular/build:application"
```

This tells Angular which builder should create the app bundle.

```json
"browser": "src/main.ts"
```

This is the browser entry point.

Your app starts from:

```text
src/main.ts
```

```json
"tsConfig": "tsconfig.app.json"
```

This tells Angular which TypeScript config to use for app builds.

```json
"assets"
```

Files from `public/` are copied into the build output.

Use this for static files like:

- Images.
- Icons.
- Manifest files.
- Public text files.

```json
"styles": ["src/styles.css"]
```

This is the global CSS file.

### Production Configuration

```json
"production": {
  "budgets": [
    {
      "type": "initial",
      "maximumWarning": "500kB",
      "maximumError": "1MB"
    },
    {
      "type": "anyComponentStyle",
      "maximumWarning": "8kB",
      "maximumError": "16kB"
    }
  ],
  "outputHashing": "all"
}
```

### What Production Settings Mean

```json
"budgets"
```

Budgets warn or fail the build if files become too large.

Example:

```text
If the initial app bundle is bigger than 500kB, Angular gives a warning.
If it is bigger than 1MB, Angular gives an error.
```

```json
"outputHashing": "all"
```

Adds hashes to output file names.

Example:

```text
main-A7S9D3.js
styles-F8K2L1.css
```

This helps browser caching. When code changes, file names change, so browser downloads the new version.

### Development Configuration

```json
"development": {
  "optimization": false,
  "extractLicenses": false,
  "sourceMap": true
}
```

Development mode keeps things easier to debug.

```json
"optimization": false
```

Does not heavily compress/optimize code.

```json
"extractLicenses": false
```

Does not extract licenses separately.

```json
"sourceMap": true
```

Allows browser dev tools to show original TypeScript source while debugging.

### Serve Section

```json
"serve": {
  "builder": "@angular/build:dev-server",
  "defaultConfiguration": "development"
}
```

This tells Angular how to run the local dev server.

When you run:

```bash
npm start
```

It uses `ng serve`, and `ng serve` uses this `serve` configuration.

### Test Section

```json
"test": {
  "builder": "@angular/build:unit-test"
}
```

This tells Angular which builder runs frontend unit tests.

### Why We Use `angular.json`

We use it because Angular needs one central place for:

- Build settings.
- Serve settings.
- Test settings.
- Project paths.
- Global assets.
- Global styles.
- Production/development behavior.

### What If `angular.json` Is Wrong?

Examples:

| Wrong Setting | What Can Break |
| --- | --- |
| Wrong `browser` path | App will not start/build |
| Wrong `styles` path | Global CSS will not load |
| Wrong `sourceRoot` | Angular may not find source files |
| Wrong `tsConfig` | TypeScript build may fail |
| Very low budgets | Production build may fail |
| Missing assets config | Public files may not copy |

### Alternatives

Alternatives:

- Vite config.
- Webpack config.
- Nx project config.
- Custom Angular builder.

But for a normal Angular CLI app, `angular.json` is the expected configuration file.

---

## 2.6 TypeScript Config Files

Files:

```text
tsconfig.json
tsconfig.app.json
tsconfig.spec.json
```

### Simple Explanation

TypeScript config files tell TypeScript how strict it should be and which files it should compile.

Angular code is written in TypeScript:

```ts
export class LoginComponent {}
```

But browsers run JavaScript, not TypeScript directly.

So Angular/TypeScript compiles TypeScript into JavaScript.

### Why Angular Uses TypeScript

TypeScript gives:

- Types.
- Interfaces.
- Compile-time checking.
- Better editor autocomplete.
- Better refactoring.
- Fewer runtime mistakes.

### `tsconfig.json`

Your base config:

```json
{
  "compileOnSave": false,
  "compilerOptions": {
    "strict": true,
    "noImplicitOverride": true,
    "noPropertyAccessFromIndexSignature": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true,
    "skipLibCheck": true,
    "isolatedModules": true,
    "experimentalDecorators": true,
    "importHelpers": true,
    "target": "ES2022",
    "module": "preserve"
  }
}
```

### Important Settings Explained

```json
"strict": true
```

Turns on strict TypeScript checking.

This helps catch mistakes early.

Example:

```ts
let score: number = '95';
```

This is wrong because `'95'` is a string, not a number. Strict mode catches it.

```json
"noImplicitReturns": true
```

Makes sure functions return something consistently.

Bad example:

```ts
function getStatus(score: number): string {
  if (score > 50) {
    return 'Pass';
  }
}
```

If score is `50` or less, this function returns nothing. TypeScript warns you.

```json
"noFallthroughCasesInSwitch": true
```

Prevents accidental switch-case fallthrough.

```json
"experimentalDecorators": true
```

Angular uses decorators like:

```ts
@Component({})
@Injectable({})
```

This setting allows decorator syntax.

```json
"target": "ES2022"
```

The output JavaScript targets modern JavaScript features.

```json
"module": "preserve"
```

Keeps module syntax for the build tool to handle.

### Angular Compiler Options

```json
"angularCompilerOptions": {
  "enableI18nLegacyMessageIdFormat": false,
  "strictInjectionParameters": true,
  "strictInputAccessModifiers": true,
  "strictTemplates": true
}
```

### What These Mean

```json
"strictTemplates": true
```

Angular checks template expressions more strictly.

Example:

If your HTML uses:

```html
{{ user.fullName }}
```

but `user` can be `null`, Angular may warn you.

This helps prevent browser runtime errors.

```json
"strictInjectionParameters": true
```

Makes dependency injection safer.

If Angular cannot understand what service to inject, it can catch that earlier.

### `tsconfig.app.json`

Your app config:

```json
{
  "extends": "./tsconfig.json",
  "compilerOptions": {
    "outDir": "./out-tsc/app",
    "types": []
  },
  "include": ["src/**/*.ts"],
  "exclude": ["src/**/*.spec.ts"]
}
```

### Line-By-Line

```json
"extends": "./tsconfig.json"
```

This file inherits the base TypeScript settings.

```json
"outDir": "./out-tsc/app"
```

Compiled output goes here during build.

```json
"include": ["src/**/*.ts"]
```

Compile TypeScript files inside `src`.

```json
"exclude": ["src/**/*.spec.ts"]
```

Do not include test files in the normal app build.

Test files are handled by test config instead.

### Why We Use TypeScript Config

We use TypeScript config because:

- The compiler needs rules.
- App files and test files are handled differently.
- Strict checks catch mistakes.
- Angular template checking improves safety.

### What If We Do Not Use Strict TypeScript?

If strict mode is off:

- More mistakes can reach the browser.
- `null` and `undefined` bugs become easier.
- API response shape mistakes may be missed.
- Refactoring becomes riskier.

### Alternatives

Alternatives:

- JavaScript only.
- Less strict TypeScript.
- Runtime validation with libraries like zod.
- Generated types from OpenAPI.

For this project, strict TypeScript is a good choice because there are many models and API responses.

---

## 2.7 Environment File

File:

```text
src/environments/environment.ts
```

### Your Project Code

```ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5190/api'
};
```

### Simple Explanation

The environment file stores configuration values that can change between development and production.

The most important value here is:

```ts
apiUrl: 'http://localhost:5190/api'
```

This tells the frontend where the backend API starts.

### Easy Scenario

Suppose the login service needs to call the backend.

Instead of writing this everywhere:

```text
http://localhost:5190/api
```

the app uses:

```ts
environment.apiUrl
```

So if the API URL changes later, you update it in one place.

### What This Does In The Project

Example from auth service:

```ts
private apiUrl = `${environment.apiUrl}/auth`;
```

If:

```ts
environment.apiUrl = 'http://localhost:5190/api'
```

then:

```ts
this.apiUrl = 'http://localhost:5190/api/auth'
```

So login might call:

```text
http://localhost:5190/api/auth/login
```

### Why We Use Environment Config

We use it because:

- API base URL should not be repeated everywhere.
- Development and production URLs can be different.
- Code is easier to update.
- Services stay cleaner.

### What If We Hardcode API URLs In Every Service?

If API URL is hardcoded everywhere:

- Changing backend port becomes painful.
- Some services may be forgotten.
- Development and production builds become messy.
- Bugs happen when different services point to different URLs.

### What If `apiUrl` Is Wrong?

If `apiUrl` is wrong:

- Login fails.
- Register fails.
- Interviews do not load.
- Assessments do not start.
- Admin pages cannot fetch data.
- Browser console/network tab will show failed requests.

### Alternatives

Alternatives:

- `.env` files.
- Runtime config loaded from JSON.
- Server-injected config.
- Reverse proxy so frontend calls relative URLs like `/api`.
- Deployment environment variables.

---

## 2.8 Important Commands

### `npm install`

Installs packages listed in `package.json`.

Run this when:

- You cloned/copied the project.
- `node_modules` is missing.
- Dependencies changed.

Command:

```bash
npm install
```

What it uses:

```text
package.json
package-lock.json
```

What it creates/updates:

```text
node_modules/
```

### `npm start`

Starts the Angular development server.

Command:

```bash
npm start
```

Internally runs:

```bash
ng serve
```

Usually opens at:

```text
http://localhost:4200
```

Use it while developing.

### `npm run build`

Builds the frontend.

Command:

```bash
npm run build
```

Internally runs:

```bash
ng build
```

It uses production configuration by default because `angular.json` says:

```json
"defaultConfiguration": "production"
```

Use it before deployment or to check whether the project compiles.

### `npm test`

Runs tests.

Command:

```bash
npm test
```

Internally runs:

```bash
ng test
```

Use it to check component/unit tests.

### `npm run watch`

Builds in development mode and watches changes.

Command:

```bash
npm run watch
```

Internally runs:

```bash
ng build --watch --configuration development
```

Useful when another tool/server is serving the built files and you want Angular to rebuild automatically.

---

## 2.9 Development vs Production

### Development Mode

Development mode is for coding and debugging.

In your `angular.json`:

```json
"development": {
  "optimization": false,
  "extractLicenses": false,
  "sourceMap": true
}
```

Development mode gives:

- Easier debugging.
- Source maps.
- Faster rebuilds.
- Less optimization.

### Production Mode

Production mode is for real deployment.

In your `angular.json`:

```json
"production": {
  "budgets": [...],
  "outputHashing": "all"
}
```

Production mode gives:

- Smaller files.
- Optimized bundles.
- Cache-friendly file names.
- Build budget checks.

### Easy Scenario

Development mode is like working on a draft document where comments and editing marks are visible.

Production mode is like exporting a clean final PDF for users.

### What If We Deploy Development Build?

Possible problems:

- Larger files.
- Slower loading.
- More internal debugging info.
- Less optimized performance.

### What If We Develop With Production Build Only?

Possible problems:

- Harder debugging.
- Slower feedback.
- Source code harder to inspect.

---

## 2.10 How These Files Work Together

### Full Flow When Running The App

When you run:

```bash
npm start
```

Flow:

```text
package.json
    -> script "start"
        -> ng serve
            -> Angular CLI
                -> angular.json serve config
                    -> angular.json build config
                        -> src/main.ts
                            -> Angular app starts
```

### Full Flow When Building The App

When you run:

```bash
npm run build
```

Flow:

```text
package.json
    -> script "build"
        -> ng build
            -> Angular CLI
                -> angular.json build config
                    -> tsconfig.app.json
                        -> tsconfig.json
                            -> compile src/**/*.ts
                                -> output build files
```

### Full Flow When Calling Backend

When the app calls backend:

```text
Component
    -> Service
        -> environment.apiUrl
            -> HttpClient
                -> API Gateway/backend
```

---

## 2.11 What This Topic Is Doing In Your Project

Your setup files make these real project features possible:

| Feature | Setup Support |
| --- | --- |
| Login/register pages | Angular forms and HTTP packages from `package.json` |
| Protected routes | Angular router package and route config |
| API calls | Angular HTTP client package and `environment.apiUrl` |
| Interview/assessment screens | Angular component system and TypeScript compile |
| Admin pages | Router, guards, services, TypeScript models |
| Global styling | `src/styles.css` configured in `angular.json` |
| Development server | `npm start` and `ng serve` |
| Production build | `npm run build` and Angular production config |
| Tests | `npm test`, Vitest/jsdom, Angular test builder |

---

## 2.12 What If We Do Not Use These Tools?

### Without npm

No easy package installation or scripts.

### Without Angular CLI

No simple `ng serve`, `ng build`, or `ng test`.

### Without `angular.json`

Angular does not know how this app should be built or served.

### Without TypeScript Config

TypeScript has no project-specific compile rules.

### Without Environment Config

Backend URLs may be repeated and become hard to change.

### Without Lock File

Dependency versions may vary between machines.

---

## 2.13 Common Mistakes

### Mistake 1: Running Commands From Wrong Folder

Bad:

```bash
npm start
```

from:

```text
Final - Copy/
```

Better:

```bash
cd Frontend
npm start
```

because `package.json` is inside `Frontend`.

### Mistake 2: Backend Is Not Running

Frontend may open fine, but login/API calls fail.

Reason:

```text
Angular app is running, but http://localhost:5190/api is not responding.
```

### Mistake 3: Wrong API Port

If backend runs on a different port, update:

```text
src/environments/environment.ts
```

### Mistake 4: Missing `node_modules`

Error may look like:

```text
ng is not recognized
Cannot find module
```

Fix:

```bash
npm install
```

### Mistake 5: Ignoring TypeScript Errors

Strict TypeScript errors usually point to real problems.

Fix them early instead of working around them with `any`.

---

## 2.14 Mini Exercise

Open:

```text
package.json
```

Answer:

1. What command starts the Angular app?
2. What command builds the Angular app?
3. Which package gives routing?
4. Which package gives forms?
5. Which package gives Observables?

Open:

```text
angular.json
```

Answer:

1. What is the app entry file?
2. What is the global stylesheet?
3. What folder contains public assets?
4. What is the default build configuration?
5. What happens if initial bundle size goes above `1MB`?

Open:

```text
src/environments/environment.ts
```

Answer:

1. What is the backend API base URL?
2. What would you change if the backend API moved to another port?

---

## 2.15 Interview-Style Questions

1. What is the purpose of `package.json`?
2. What is the difference between dependencies and devDependencies?
3. What does `npm start` do in this project?
4. What does Angular CLI do?
5. What is the purpose of `angular.json`?
6. What is the app entry point in this project?
7. Why does Angular use TypeScript?
8. What does `strict: true` mean?
9. Why should API URLs be stored in environment files?
10. What is the difference between development and production build?

---

## Topic 2 Summary

The setup and tooling files are what let your Angular project run correctly.

Most important mental model:

```text
package.json gives commands and packages.
angular.json tells Angular how to build, serve, and test.
tsconfig files tell TypeScript how to compile.
environment.ts tells services where the backend API is.
node_modules contains installed packages.
package-lock.json locks exact dependency versions.
```

If the app does not start, build, test, or connect to backend, these setup files are the first places to check.

---

## Next Topic

Recommended next lesson:

```text
Topic 3: Angular App Startup
```

Files to study next:

```text
src/index.html
src/main.ts
src/app/app.config.ts
src/app/app.component.ts
src/app/app.component.html
src/app/app.routes.ts
```
