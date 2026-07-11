export interface AuthRequest {
  userName: string;
  password: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface AuthResponse {
  userId: string;
  userName: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}
