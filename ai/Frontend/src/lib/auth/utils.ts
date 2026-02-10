/**
 * Authentication Utilities
 * 
 * Centralized utilities for authentication operations,
 * token handling, and role management.
 */

import { TokenPayload } from '@/types/auth';

/**
 * Decodes a JWT token and returns the payload
 */
export function decodeJwtToken(token: string): TokenPayload | null {
  try {
    const tokenParts = token.split('.');
    if (tokenParts.length !== 3) {
      throw new Error('Invalid token format');
    }
    
    const payload = JSON.parse(atob(tokenParts[1]));
    return payload;
  } catch (error) {
    console.error('Failed to decode JWT token:', error);
    return null;
  }
}

/**
 * Extracts roles from a token payload
 */
export function extractRolesFromToken(payload: TokenPayload): string[] {
  const roles = payload.roles || payload.Roles || [];
  return Array.isArray(roles) ? roles : [roles].filter(Boolean);
}

/**
 * Checks if a user has a specific role
 */
export function hasRole(userRoles: string[], requiredRole: string): boolean {
  return userRoles.includes(requiredRole);
}

/**
 * Checks if a user has any of the specified roles
 */
export function hasAnyRole(userRoles: string[], requiredRoles: string[]): boolean {
  return requiredRoles.some(role => userRoles.includes(role));
}

/**
 * Checks if a user has all of the specified roles
 */
export function hasAllRoles(userRoles: string[], requiredRoles: string[]): boolean {
  return requiredRoles.every(role => userRoles.includes(role));
}

/**
 * Validates if an account belongs to a specific tenant
 */
export function isAccountFromTenant(account: any, tenantId: string): boolean {
  return account.idTokenClaims?.tid === tenantId;
}

/**
 * Creates a standardized user info object from an MSAL account
 */
export function createUserInfoFromAccount(account: any, roles: string[] = []): {
  id: string;
  email: string;
  name: string;
  roles: string[];
  tenantId: string;
} {
  return {
    id: account.homeAccountId || account.localAccountId || '',
    email: account.username || account.idTokenClaims?.email || account.idTokenClaims?.preferred_username || '',
    name: account.name || account.idTokenClaims?.name || 'User',
    roles,
    tenantId: account.idTokenClaims?.tid || '',
  };
}

/**
 * Logs authentication events in a standardized format
 */
export function logAuthEvent(event: string, data: any = {}) {
  console.log(`[AUTH] ${event}:`, {
    timestamp: new Date().toISOString(),
    ...data,
  });
}

/**
 * Creates a standardized error message for authentication failures
 */
export function createAuthErrorMessage(error: any): string {
  if (error?.errorCode) {
    switch (error.errorCode) {
      case 'user_cancelled':
        return 'Sign-in was cancelled by the user';
      case 'consent_required':
        return 'User consent is required';
      case 'interaction_required':
        return 'Additional authentication is required';
      case 'token_expired':
        return 'Your session has expired. Please sign in again';
      default:
        return `Authentication failed: ${error.errorMessage || error.message || 'Unknown error'}`;
    }
  }
  
  return error?.message || 'An unexpected error occurred during authentication';
}
