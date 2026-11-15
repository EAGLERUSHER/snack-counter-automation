import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import {
  ChatMessage,
  Employee,
  MonthlyReport,
  OfficeId,
  OfficeInfo,
  Refill,
} from '../../models';
import { SnackApiService } from '../../services/snack-api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(SnackApiService);
  readonly auth = inject(AuthService);

  offices = signal<OfficeInfo[]>([]);
  employees = signal<Employee[]>([]);
  refills = signal<Record<OfficeId, Refill | null>>({ Oslo: null, Bengaluru: null });
  chat = signal<ChatMessage[]>([]);
  report = signal<MonthlyReport | null>(null);
  purchaseOslo = 500;
  purchaseBlr = 1500;
  receiptOslo: File | null = null;
  receiptBlr: File | null = null;
  error = signal<string | null>(null);
  loading = signal(false);

  readonly officeIds: OfficeId[] = ['Oslo', 'Bengaluru'];

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      offices: this.api.getOffices(),
      employees: this.api.getEmployees(),
      chat: this.api.getChat(),
      report: this.api.getMonthlyReport(),
      oslo: this.api.getActiveRefill('Oslo'),
      blr: this.api.getActiveRefill('Bengaluru'),
    }).subscribe({
      next: (data) => {
        this.offices.set(data.offices);
        this.employees.set(data.employees);
        this.chat.set([...data.chat].reverse());
        this.report.set(data.report);
        this.refills.set({ Oslo: data.oslo, Bengaluru: data.blr });
        this.loading.set(false);
      },
      error: (err) => {
        if (err?.status === 401) {
          this.auth.logout();
          return;
        }
        this.error.set(err?.message ?? 'Failed to load data. Is the API running?');
        this.loading.set(false);
      },
    });
  }

  employeeName(id: string | null | undefined): string {
    if (!id) return '—';
    return this.employees().find((e) => e.id === id)?.name ?? id;
  }

  currentUserId(): string {
    return this.auth.employee()?.id ?? '';
  }

  reportEmpty(office: OfficeId): void {
    this.run(this.api.reportEmpty(office, this.currentUserId()));
  }

  commit(office: OfficeId): void {
    const refill = this.refills()[office];
    if (!refill) return;
    const who = refill.assignedVolunteerId ?? this.currentUserId();
    this.run(this.api.commit(refill.id, who));
  }

  purchase(office: OfficeId): void {
    const refill = this.refills()[office];
    if (!refill) return;
    const who = refill.committedByEmployeeId ?? refill.assignedVolunteerId ?? this.currentUserId();
    const amount = office === 'Oslo' ? this.purchaseOslo : this.purchaseBlr;
    const file = office === 'Oslo' ? this.receiptOslo : this.receiptBlr;
    if (!file) {
      this.error.set('Attach a receipt image or PDF before recording purchase.');
      return;
    }
    this.run(this.api.purchase(refill.id, who, amount, file));
  }

  onReceipt(office: OfficeId, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    if (office === 'Oslo') this.receiptOslo = file;
    else this.receiptBlr = file;
  }

  submitHr(office: OfficeId): void {
    const refill = this.refills()[office];
    if (!refill) return;
    this.run(this.api.submitHr(refill.id));
  }

  approveHr(office: OfficeId): void {
    const refill = this.refills()[office];
    if (!refill) return;
    this.run(this.api.approveHr(refill.id));
  }

  reassign(office: OfficeId): void {
    const refill = this.refills()[office];
    if (!refill) return;
    this.run(this.api.reassignStale(refill.id));
  }

  advanceTime(): void {
    this.run(this.api.advanceHours(5));
  }

  budgetPct(office: OfficeInfo): number {
    return Math.min(100, Math.round((office.spentThisMonth / office.monthlyBudget) * 100));
  }

  private run<T>(obs: import('rxjs').Observable<T>): void {
    this.error.set(null);
    obs.subscribe({
      next: () => this.refresh(),
      error: (err) => {
        const body = err?.error;
        this.error.set(body?.message ?? err?.message ?? 'Request failed');
        this.refresh();
      },
    });
  }
}