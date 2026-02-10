/**
 * Authentication Types and Interfaces
 * 
 * This file contains all authentication-related type definitions
 * following industry standards and clean architecture principles.
 */

export interface UserInfo {
  id: string;
  email: string;
  name: string;
  roles: string[];
  tenantId?: string;
  profilePictureUrl?: string;
}

export interface AuthConfig {
  tenantId: string;
  clientId: string;
  scope: string;
  authority: string;
  redirects: {
    success: string;
    logout: string;
  };
}

export interface AuthState {
  user: UserInfo | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  error: string | null;
}

export interface AuthActions {
  login: (loginType?: 'popup' | 'redirect') => Promise<void>;
  logout: () => Promise<void>;
  getAccessToken: () => Promise<string | null>;
  refreshToken: () => Promise<void>;
}

export interface AuthHook extends AuthState, AuthActions {}

export type UserRole = 'RecruitmentAdmin' | 'Candidate';

export interface RoleCheck {
  hasRole: (role: string) => boolean;
  hasAnyRole: (roles: string[]) => boolean;
  hasAllRoles: (roles: string[]) => boolean;
}

export interface GuardProps {
  children: React.ReactNode;
  fallbackPath?: string;
  requiredRole?: string;
  showLoading?: boolean;
}

export interface TokenPayload {
  roles?: string[];
  Roles?: string[];
  tid?: string;
  email?: string;
  name?: string;
  sub?: string;
  aud?: string;
  iss?: string;
  exp?: number;
  iat?: number;
}
