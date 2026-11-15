import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { Employee } from '../models';

const TOKEN_KEY = 'snack_counter_token';
const USER_KEY = 'snack_counter_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly employee = signal<Employee | null>(this.readStoredUser());

  get token(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  get isLoggedIn(): boolean {
    return !!this.token;
  }

  loadEmployeesForLogin() {
    return this.http.get<Employee[]>('/api/employees');
  }

  login(employeeId: string) {
    return this.http
      .post<{ token: string; employee: Employee }>('/api/auth/login', { employeeId })
      .pipe(
        tap((res) => {
          sessionStorage.setItem(TOKEN_KEY, res.token);
          sessionStorage.setItem(USER_KEY, JSON.stringify(res.employee));
          this.employee.set(res.employee);
        }),
      );
  }

  logout(): void {
    const token = this.token;
    if (token) {
      this.http
        .post('/api/auth/logout', null, {
          headers: { Authorization: `Bearer ${token}` },
        })
        .subscribe({ error: () => undefined });
    }
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    this.employee.set(null);
    this.router.navigateByUrl('/login');
  }

  private readStoredUser(): Employee | null {
    const raw = sessionStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as Employee;
    } catch {
      return null;
    }
  }
}