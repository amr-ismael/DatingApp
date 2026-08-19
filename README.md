# DatingApp

A full-stack SPA built while following a structured **ASP.NET Core + Angular** course, as a hands-on way to learn how a .NET backend and an Angular front end fit together end-to-end.

## Stack

- **Backend:** ASP.NET Core Web API, Entity Framework Core, SQLite, JWT authentication
- **Frontend:** Angular, TypeScript, Bootstrap

## What's implemented

- User registration and login (`AuthController`, `AuthService`) with JWT issuance and storage
- Password hashing (HMAC-SHA512)
- Angular route guard (`AuthGuard`) protecting authenticated routes
- Nav bar with reactive logged-in/logged-out state
- EF Core data model and migrations, seeded with sample users

## What's not finished

This was a learning project, worked on in 2022 and set aside partway through the course. Two pieces are scaffolded but not built out:

- **Member browsing** (`member-list` component) — routed and guarded, but the component is an empty placeholder with no data fetching wired up yet.
- **Messaging** (`messages` component) — same story: scaffolded, not implemented.

The parts that are finished (auth, JWT, guards, backend integration) work end-to-end; the browsing/messaging features are exactly where the course left off.

## Running locally

**API** (`DatingApp.API/`):
```bash
dotnet restore
dotnet run
```
Requires an `appsettings.json` with a JWT `TokenKey` (not committed — see `Startup.cs` for the expected config shape).

**SPA** (`DatingApp-SPA/`):
```bash
npm install
npm start
```
Note: this project uses Angular 9 / Webpack 4, which needs Node's legacy OpenSSL provider to build on modern Node.js. That's already baked into the npm scripts via `cross-env NODE_OPTIONS=--openssl-legacy-provider`, so `npm start` / `npm run build` work out of the box.
