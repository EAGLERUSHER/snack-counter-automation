import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Employee } from '../../models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  employees = signal<Employee[]>([]);
  employeeId = signal('oslo-1');
  error = signal<string | null>(null);
  loading = signal(false);

  ngOnInit(): void {
    if (this.auth.isLoggedIn) {
      this.router.navigateByUrl('/');
      return;
    }
    this.auth.loadEmployeesForLogin().subscribe({
      next: (list) => this.employees.set(list),
      error: () => this.error.set('Could not load employees. Start the API first.'),
    });
  }

  signIn(): void {
    this.loading.set(true);
    this.error.set(null);
    this.auth.login(this.employeeId()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/');
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message ?? 'Login failed');
      },
    });
  }
}