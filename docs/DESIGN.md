# Snack Counter Automation — Design

## Happy path: empty → reimbursed

1. An employee reports an empty box via the internal chat integration (`ReportEmpty`). At most **one active refill** exists per office.
2. **VolunteerSelector** picks an eligible volunteer (in-office, not on holiday, no other open commitment) using fairness (oldest `LastRefillAt`, then lowest refill count).
3. The orchestrator notifies the volunteer and channel; assignment expires after **4 hours** (configurable).
4. The assigned volunteer **commits** (`CommitToRefill`). Only one commit succeeds; others receive `CONFLICT`.
5. Volunteer buys snacks, enters amount + receipt (`RecordPurchase`). Spend is checked against the **monthly office budget** and appended to the ledger immediately (spend visibility, even before HR pays out).
6. System submits a reimbursement claim to the **HR API** with amount, currency, description, receipt image, and employee ID (`SubmitToHr`).
7. When HR approves (simulated), status becomes **Reimbursed**; volunteer stats update and the commitment is cleared.

```text
ReportedEmpty → Assigned → Committed → Purchased → HrPending → Reimbursed
                    ↘ (timeout) → Reassigned → Assigned …
```

## Components and data

| Part | Responsibility | Data kept |
|------|----------------|-----------|
| `RefillOrchestrator` | Lifecycle, chat notifications, concurrency rules | — (coordinates) |
| `RefillRequest` / `IRefillRepository` | Active refill per office | Status, assignee, committer, amounts, HR claim id, timestamps |
| `VolunteerSelector` / `IVolunteerRegistry` | Who should buy | Employees, home/visit office, location status, fairness fields, open commitment |
| `BudgetService` / `IBudgetLedger` | Monthly caps and spend | Append-only spend events (office, amount, currency, refill id, time) |
| `MonthlyReportService` | Management report | Aggregates ledger; optional FX into one reporting currency |
| `IChatNotifier` (simulated) | Nudges and transparency | In-memory message log |
| `IHrReimbursementClient` (simulated) | Claims and approval | Claim records |

Office budgets: **Oslo 6 000 NOK**, **Bengaluru 15 000 INR** (per brief).

## Preventing today’s problems

- **Box empty for days:** Channel pings on report; assignee DM; **SLA reassignment** (`ReassignIfStale`) if no commit or no purchase in time.
- **Two people buying:** Single active refill; **atomic commit** under a lock; purchase only by committed volunteer.
- **Same volunteers every time:** Fairness sort on last refill date and quarterly count; timeouts rotate assignee.
- **No spend visibility:** Ledger on purchase; `MonthlyReportService` for per-office and company totals.

## Trade-offs

- **In-memory stores** — fast to build and test; production would use a database and idempotent HR webhooks.
- **FX (1 NOK = 8 INR in tests)** — gives a single headline total for management; real payroll uses employee reimbursement currency separately.
- **Budget enforced at purchase** — hard block when monthly cap would be exceeded; typical purchase range only triggers a chat warning.
- **No UI / OCR** — chat and HR are ports with simulators.

## Web UI (Angular)

The Angular app talks to `SnackCounter.Api`: login issues an in-memory Bearer token (simulated company SSO); mutating API routes require it. Purchase uses **multipart upload** of a receipt file into the existing HR claim flow. For demos, `scripts/sync-ui-to-wwwroot.ps1` copies the production build into API `wwwroot` so UI and API share **http://localhost:5018**.

## Deliberately not built

- Production chat bot UI, receipt OCR, dispute workflows, manager escalation policies, per-snack consumption analytics, real OAuth/SSO, and real HR/chat credentials.