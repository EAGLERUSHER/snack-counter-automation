import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ChatMessage,
  Employee,
  MonthlyReport,
  OfficeId,
  OfficeInfo,
  Refill,
} from '../models';

@Injectable({ providedIn: 'root' })
export class SnackApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api';

  getOffices(): Observable<OfficeInfo[]> {
    return this.http.get<OfficeInfo[]>(`${this.base}/offices`);
  }

  getActiveRefill(office: OfficeId): Observable<Refill | null> {
    return this.http.get<Refill | null>(`${this.base}/offices/${office}/active-refill`);
  }

  getEmployees(): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${this.base}/employees`);
  }

  getChat(): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.base}/chat`);
  }

  getMonthlyReport(): Observable<MonthlyReport> {
    return this.http.get<MonthlyReport>(`${this.base}/reports/monthly`);
  }

  reportEmpty(officeId: OfficeId, reporterEmployeeId: string): Observable<Refill> {
    return this.http.post<Refill>(`${this.base}/refills/report-empty`, {
      officeId,
      reporterEmployeeId,
    });
  }

  commit(refillId: string, employeeId: string): Observable<Refill> {
    return this.http.post<Refill>(`${this.base}/refills/${refillId}/commit`, { employeeId });
  }

  purchase(
    refillId: string,
    employeeId: string,
    amount: number,
    receiptFile: File,
  ): Observable<Refill> {
    const form = new FormData();
    form.append('amount', String(amount));
    form.append('employeeId', employeeId);
    form.append('receipt', receiptFile, receiptFile.name);
    return this.http.post<Refill>(`${this.base}/refills/${refillId}/purchase`, form);
  }

  submitHr(refillId: string): Observable<Refill> {
    return this.http.post<Refill>(`${this.base}/refills/${refillId}/submit-hr`, {});
  }

  approveHr(refillId: string): Observable<Refill> {
    return this.http.post<Refill>(`${this.base}/refills/${refillId}/approve-hr`, {});
  }

  reassignStale(refillId: string): Observable<Refill | null> {
    return this.http.post<Refill | null>(`${this.base}/refills/${refillId}/reassign-stale`, {});
  }

  advanceHours(hours: number): Observable<string> {
    return this.http.post(`${this.base}/demo/advance-hours?hours=${hours}`, null, {
      responseType: 'text',
    });
  }
}