# Snack Counter Automation

Core logic for coordinating office snack refills, volunteer assignment, budget tracking, and simulated HR/chat integrations (Framsikt assignment).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — verify with `dotnet --list-sdks` (you should see an 8.0.x line)

## Run the demo (terminal)

```powershell
cd c:\Users\user\Documents\Project
dotnet run --project src/SnackCounter.Demo/SnackCounter.Demo.csproj
```

## Run the demo (VS Code / Cursor)

1. Install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) or [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) extension.
2. Open **Run and Debug** (Ctrl+Shift+D).
3. Choose **SnackCounter Demo** and press F5 (builds via the `build` task, then runs the console demo).

## Run tests

```powershell
dotnet test
```

Or in VS Code: Terminal → Run Task → **test**.

## Demo walkthrough (10 minutes)

1. Someone reports an empty box → one active refill per office; chat notifies the channel.
2. System assigns a fair volunteer (availability + least recent refill).
3. Only that volunteer can **commit**; a second commit fails (no double-buying).
4. Purchase is recorded; monthly budget is checked; spend hits the ledger.
5. Receipt goes to the simulated HR system → approval → reimbursed; volunteer stats update.

See [docs/DESIGN.md](docs/DESIGN.md) for the full design.

## Angular frontend + API

| Piece | Path |
|-------|------|
| REST API | [`src/SnackCounter.Api`](src/SnackCounter.Api) |
| Angular UI (source) | [`client/snack-counter-ui`](client/snack-counter-ui) |

### Single port (recommended for demo)

```powershell
.\scripts\sync-ui-to-wwwroot.ps1
dotnet run --project src/SnackCounter.Api/SnackCounter.Api.csproj --urls http://localhost:5018
```

Open **http://localhost:5018** → **Sign in** with an employee → dashboard.

Includes **Bearer token auth**, **receipt upload** (image/PDF) on purchase, and the full refill workflow.

### Dev mode (Angular hot reload)

```powershell
dotnet run --project src/SnackCounter.Api/SnackCounter.Api.csproj --urls http://localhost:5018
cd client/snack-counter-ui
npm start
```

Open **http://localhost:4200** (proxies `/api`). Use `/login` first — API calls require a token.

**Cursor / VS Code:** F5 **SnackCounter API** after `sync-ui-to-wwwroot.ps1`, or Run Task **ng-serve** + API for dev mode.