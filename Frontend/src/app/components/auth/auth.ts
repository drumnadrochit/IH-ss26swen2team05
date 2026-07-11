import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { PopupComponent } from '../shared/popup/popup';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule, PopupComponent],
  templateUrl: './auth.html',
  styleUrls: ['./auth.scss'],
})
export class AuthComponent {
  private cdr = inject(ChangeDetectorRef);

  isRegisterMode = false;
  showPopup = false;

  // Login fields
  loginUsername = '';
  loginPassword = '';

  // Register fields
  registerUsername = '';
  registerPassword = '';
  registerPasswordConfirm = '';

  errorMessage = '';
  successMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  onLogin(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.loginUsername.trim() || !this.loginPassword.trim()) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    this.authService.login(this.loginUsername, this.loginPassword).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = this.extractError(err, 'Invalid username or password.');
        this.cdr.markForCheck();
      },
    });
  }

  onRegister(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (
      !this.registerUsername.trim() ||
      !this.registerPassword.trim() ||
      !this.registerPasswordConfirm.trim()
    ) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }
    if (this.registerPassword !== this.registerPasswordConfirm) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }
    if (this.registerPassword.length < 8) {
      this.errorMessage = 'Passwords must be at least 8 characters.';
      return;
    }

    this.authService.register(this.registerUsername, this.registerPassword).subscribe({
      next: () => {
        this.successMessage = 'Account created! Switching to login…';
        this.registerUsername = '';
        this.registerPassword = '';
        this.registerPasswordConfirm = '';
        this.showPopup = true;
        this.cdr.markForCheck();

        setTimeout(() => {
          this.showPopup = false;
          this.successMessage = '';
          this.isRegisterMode = false;
          this.cdr.markForCheck();
        }, 5000);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = this.extractError(err, 'Registration failed. Please try again.');
        this.cdr.markForCheck();
      },
    });
  }

  onPopupConfirm() {
    this.showPopup = false;
    this.isRegisterMode = false;
  }

  private extractError(err: HttpErrorResponse, fallback: string): string {
    if (err?.error?.error) {
      return err.error.error;
    }
    if (err?.status === 0) {
      return 'Cannot reach server. Is the backend running?';
    }
    return fallback;
  }
}
