import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthRequest, RefreshRequest, AuthResponse } from '../models/API/Auth';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/auth`;

  private tokenKey = 'accessToken';
  private refreshKey = 'refreshToken';
  private userKey = 'currentUser';

  private isBrowser(): boolean {
    return typeof localStorage !== 'undefined';
  }

  login(userName: string, password: string) {
    const body: AuthRequest = { userName, password };
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, body).pipe(
      tap(res => this.storeSession(res))
    );
  }

  register(userName: string, password: string) {
    const body: AuthRequest = { userName, password };
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, body).pipe(
      tap(res => this.storeSession(res))
    );
  }

  refresh() {
    const body: RefreshRequest = { refreshToken: this.getRefreshToken() ?? '' };
    return this.http.post<AuthResponse>(`${this.baseUrl}/refresh`, body).pipe(
      tap(res => this.storeSession(res))
    );
  }

  logout(): void {
    if (this.isBrowser()) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.refreshKey);
      localStorage.removeItem(this.userKey);
    }
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  getToken(): string | null {
    return this.isBrowser() ? localStorage.getItem(this.tokenKey) : null;
  }

  getRefreshToken(): string | null {
    return this.isBrowser() ? localStorage.getItem(this.refreshKey) : null;
  }

  getCurrentUser(): { userId: string; userName: string } | null {
    if (!this.isBrowser()) return null;
    const stored = localStorage.getItem(this.userKey);
    return stored ? JSON.parse(stored) : null;
  }

  private storeSession(res: AuthResponse): void {
    if (!this.isBrowser()) return;
    localStorage.setItem(this.tokenKey, res.accessToken);
    localStorage.setItem(this.refreshKey, res.refreshToken);
    localStorage.setItem(
      this.userKey,
      JSON.stringify({ userId: res.userId, userName: res.userName })
    );
  }
}
