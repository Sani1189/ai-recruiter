"use client";

import { useMsal } from "@azure/msal-react";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { toast } from "sonner";

import { TEMP } from "@/constants/temp";
import { authService } from "@/lib/api/services/auth.service";
import { candidateLoginRequest } from "@/lib/candidateAuthConfig";
import { recruiterLoginRequest } from "@/lib/recruiterAuthConfig";
import { getRecruiterMsalInstance } from "@/lib/UnifiedAuthProvider";
import { useAuthStore } from "@/stores/useAuthStore";

export type UserType = "candidate" | "recruiter" | null;

interface UserInfo {
  id: string;
  email: string | null;
  name: string | null;
  roles?: string[];
  tenantId?: string;
}

interface UnifiedAuthHook {
  user: UserInfo | null;
  userType: UserType;
  isLoading: boolean;
  isAuthenticated: boolean;
  error: string | null;
  login: (
    userType: "candidate" | "recruiter",
    loginType?: "popup" | "redirect",
    redirectUrl?: string,
  ) => Promise<void>;
  logout: () => Promise<void>;
  logoutWithPopup: () => Promise<void>;
  getAccessToken: () => Promise<string | null>;
  isAuthenticationValid: () => Promise<boolean>;
}

export function useUnifiedAuth(): UnifiedAuthHook {
  const { instance, accounts, inProgress } = useMsal();
  const router = useRouter();
  const {
    user,
    userType,
    loginWithAzure,
    logout: logoutFromStore,
  } = useAuthStore();

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  /**
   * Extract user info from MSAL account
   */
  const extractUserInfo = useCallback(
    (account: any, roles: string[] = []): UserInfo => {
      const email =
        account.username ||
        account.idTokenClaims?.email ||
        account.idTokenClaims?.preferred_username ||
        "";
      const name = account.name || account.idTokenClaims?.name || "User";
      const id = account.homeAccountId || account.localAccountId;

      return {
        id,
        email,
        name,
        roles,
        tenantId: TEMP.tenantId,
      };
    },
    [],
  );

  /**
   * Extract roles from access token
   */
  const extractRolesFromToken = useCallback((accessToken: string): string[] => {
    try {
      // Decode JWT token payload
      const payload = JSON.parse(atob(accessToken.split(".")[1]));

      // Extract roles from various possible claim names
      const roles =
        payload.roles ||
        payload.Roles ||
        payload.role ||
        payload.Role ||
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] ||
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/groups"
        ] ||
        payload.groups ||
        payload.Groups ||
        [];

      // Ensure it's an array
      return Array.isArray(roles) ? roles : [];
    } catch (error) {
      return [];
    }
  }, []);

  /**
   * Determine user type based on roles
   */
  const determineUserType = useCallback((roles: string[]): UserType => {
    // Check for various possible admin role names
    const adminRoles = [
      "RecruitmentAdmin",
      "recruitmentadmin",
      "Recruitment Admin",
      "Admin",
      "admin",
      "Administrator",
      "administrator",
    ];

    const hasAdminRole = roles.some((role) =>
      adminRoles.some((adminRole) =>
        role.toLowerCase().includes(adminRole.toLowerCase()),
      ),
    );

    if (hasAdminRole) {
      return "recruiter";
    }
    // No roles or no admin role = candidate
    return "candidate";
  }, []);

  /**
   * Initialize authentication on mount
   */
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        setIsLoading(true);
        setError(null);

        // Handle redirect promise (when user returns from Azure login redirect)
        // Check both candidate and recruiter instances for redirect responses
        const candidateResponse = await instance.handleRedirectPromise();
        const recruiterInstance = getRecruiterMsalInstance();
        const recruiterResponse =
          await recruiterInstance.handleRedirectPromise();

        const response = candidateResponse || recruiterResponse;
        let account = response?.account;
        const isRedirectResponse = !!response; // Track if this is a redirect response

        // If no redirect account, check existing accounts from both instances
        if (!account) {
          if (accounts.length > 0) {
            account = accounts[0];
          } else {
            const recruiterAccounts = await recruiterInstance.getAllAccounts();
            if (recruiterAccounts.length > 0) {
              account = recruiterAccounts[0];
            }
          }
        }

        if (account) {
          // Extract basic user info first
          const userInfo = extractUserInfo(account);

          // Try to get roles from access token
          let roles: string[] = [];
          let accessToken: string | null = null;
          let msalInstance = instance;
          let loginRequest = candidateLoginRequest;

          try {
            // Try candidate token first (since we're using candidate instance for initialization)
            const candidateTokenResponse = await instance.acquireTokenSilent({
              ...candidateLoginRequest,
              account,
            });

            if (candidateTokenResponse.accessToken) {
              roles = extractRolesFromToken(candidateTokenResponse.accessToken);
              accessToken = candidateTokenResponse.accessToken;
            }
          } catch {
            // If candidate token fails, try recruiter token
            try {
              msalInstance = getRecruiterMsalInstance();
              loginRequest = recruiterLoginRequest;
              const recruiterTokenResponse =
                await msalInstance.acquireTokenSilent({
                  ...recruiterLoginRequest,
                  account,
                });

              if (recruiterTokenResponse.accessToken) {
                roles = extractRolesFromToken(
                  recruiterTokenResponse.accessToken,
                );
                accessToken = recruiterTokenResponse.accessToken;
              }
            } catch {
              // If both fail, treat as candidate (no roles)
              roles = [];
            }
          }

          const determinedUserType = determineUserType(roles);
          const finalUserInfo = { ...userInfo, roles };

          // Update auth store
          await loginWithAzure(finalUserInfo, determinedUserType);

          // If this was a redirect response, call /me API to create/ensure user exists
          if (isRedirectResponse && accessToken) {
            try {
              await authService.getMe(accessToken);
              toast.success(
                `Welcome, ${determinedUserType === "recruiter" ? "Recruiter Admin" : "Candidate"}!`,
                {
                  description: "You have successfully signed in.",
                },
              );

              // Don't redirect here - let the page handle redirect
              // Pages (sign-in, etc.) will read from sessionStorage and redirect appropriately
            } catch (registerError) {
              // Log but don't fail login if registration fails
              console.warn(
                "Failed to create user profile after redirect login:",
                registerError,
              );
            }
          }
        }
      } catch (error) {
        setError(
          error instanceof Error ? error.message : "Authentication failed",
        );
      } finally {
        setIsLoading(false);
      }
    };

    if (inProgress === "none") {
      initializeAuth();
    }
  }, [
    instance,
    accounts,
    inProgress,
    extractUserInfo,
    determineUserType,
    loginWithAzure,
  ]);

  /**
   * Login for specific user type
   */
  const login = useCallback(
    async (
      targetUserType: "candidate" | "recruiter",
      loginType: "popup" | "redirect" = "popup",
      redirectUrl?: string,
    ) => {
      try {
        setIsLoading(true);
        setError(null);

        const loginRequest =
          targetUserType === "recruiter"
            ? recruiterLoginRequest
            : candidateLoginRequest;
        // Use appropriate MSAL instance based on user type
        const msalInstance =
          targetUserType === "recruiter"
            ? getRecruiterMsalInstance()
            : instance;

        if (loginType === "popup") {
          const response = await msalInstance.loginPopup(loginRequest);
          if (response.account) {
            const userInfo = extractUserInfo(response.account);

            // Get access token to extract roles
            let roles: string[] = [];
            try {
              const tokenResponse = await msalInstance.acquireTokenSilent({
                ...loginRequest,
                account: response.account,
              });

              if (tokenResponse.accessToken) {
                roles = extractRolesFromToken(tokenResponse.accessToken);
              }
            } catch (error) {
              console.error(
                "Failed to get access token for role extraction:",
                error,
              );
              roles = [];
            }

            const finalUserInfo = { ...userInfo, roles };
            const determinedUserType = determineUserType(roles);

            // Check if user tried to sign in as recruiter but doesn't have the role
            if (
              targetUserType === "recruiter" &&
              determinedUserType !== "recruiter"
            ) {
              // Logout immediately
              await logoutFromStore();

              toast.error("Access Denied", {
                description:
                  "You are not authorized as a recruiter/admin. Please sign in as a candidate instead.",
              });

              // Redirect to candidate sign-in
              router.push("/sign-in");
              return;
            }

            await loginWithAzure(finalUserInfo, determinedUserType);
            // Call /me endpoint after successful login to create user profile if not exists
            // This works for both recruiters and candidates
            try {
              const tokenResponse = await msalInstance.acquireTokenSilent({
                ...loginRequest,
                account: response.account,
              });

              if (tokenResponse.accessToken) {
                await authService.getMe(tokenResponse.accessToken);
              }
            } catch (registerError) {
              // Log but don't fail login if registration fails
              console.warn(
                "Failed to create user profile after login:",
                registerError,
              );
            }
            toast.success(
              `Welcome, ${determinedUserType === "recruiter" ? "Recruiter Admin" : "Candidate"}!`,
              {
                description: "You have successfully signed in.",
              },
            );
          }
        } else {
          // Don't store redirect URL here - pages handle their own redirect URLs
          // Just initiate the Azure redirect login
          await msalInstance.loginRedirect(loginRequest);
          // Note: Page will navigate away, redirect handling will complete login
        }
      } catch (error) {
        setError(error instanceof Error ? error.message : "Login failed");

        toast.error("Sign in failed", {
          description:
            error instanceof Error ? error.message : "Please try again.",
        });
      } finally {
        setIsLoading(false);
      }
    },
    [
      instance,
      extractUserInfo,
      determineUserType,
      loginWithAzure,
      logoutFromStore,
      router,
    ],
  );

  /**
   * Logout current user with popup (original method)
   */
  const logoutWithPopup = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Use the appropriate MSAL instance based on user type
      const msalInstance =
        userType === "recruiter" ? getRecruiterMsalInstance() : instance;

      // Logout from Azure first
      try {
        await msalInstance.logoutPopup({
          postLogoutRedirectUri: window.location.origin,
          mainWindowRedirectUri: window.location.origin,
        });
      } catch (popupError) {
        console.warn("Popup logout failed, trying redirect:", popupError);
        // Fallback to redirect if popup fails
        await msalInstance.logoutRedirect({
          postLogoutRedirectUri: window.location.origin,
        });
      }

      // Clear auth store after successful Azure logout
      await logoutFromStore();

      toast.success("Signed out successfully");
      router.push("/");
    } catch (error) {
      console.error("Logout failed:", error);

      // Even if Azure logout fails, clear local state
      await logoutFromStore();
      router.push("/");
    } finally {
      setIsLoading(false);
    }
  }, [instance, userType, logoutFromStore, router]);

  /**
   * Get access token for current user
   */
  const getAccessToken = useCallback(async (): Promise<string | null> => {
    try {
      if (!user || !userType) {
        console.warn("getAccessToken called but no user authenticated");
        return null;
      }

      // Use appropriate login request and MSAL instance based on user type
      const loginRequest =
        userType === "recruiter"
          ? recruiterLoginRequest
          : candidateLoginRequest;
      const msalInstance =
        userType === "recruiter" ? getRecruiterMsalInstance() : instance;

      // Get accounts from the appropriate MSAL instance only
      const msalAccounts = await msalInstance.getAllAccounts();

      if (msalAccounts.length === 0) {
        throw new Error(`No accounts found in ${userType} MSAL instance`);
      }

      const account = msalAccounts[0];

      const response = await msalInstance.acquireTokenSilent({
        ...loginRequest,
        account,
      });

      return response.accessToken;
    } catch (error: any) {
      console.error("Failed to get access token:", error);

      // Handle specific MSAL errors
      if (
        error.errorCode === "interaction_required" ||
        error.errorCode === "consent_required" ||
        error.errorCode === "login_required"
      ) {
        // Clear the auth store since tokens are invalid
        await logoutFromStore();

        throw new Error("Authentication expired. Please log in again.");
      }

      // For other errors, re-throw with more context
      if (error.message?.includes("No accounts found")) {
        await logoutFromStore();
        throw new Error(
          "No valid authentication session found. Please log in again.",
        );
      }

      throw error;
    }
  }, [instance, user, userType, logoutFromStore]);

  /**
   * Check if user authentication is still valid
   */
  const isAuthenticationValid = useCallback(async (): Promise<boolean> => {
    try {
      if (!user) {
        return false;
      }

      // Try to get a token silently - if this succeeds, auth is valid
      await getAccessToken();
      return true;
    } catch (error: any) {
      return false;
    }
  }, [getAccessToken, user, userType]);

  /**
   * Logout current user (standard method)
   */
  const logout = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      console.log("Logout initiated for user type:", userType);

      const savedTheme =
        typeof window !== "undefined" ? localStorage.getItem("theme") : null;

      await logoutFromStore();

      if (typeof window !== "undefined") {
        localStorage.removeItem("auth-storage");
        sessionStorage.clear();

        // Restore theme after clearing
        if (savedTheme) {
          localStorage.setItem("theme", savedTheme);
        }
      }

      // Use the appropriate MSAL instance based on user type
      const msalInstance =
        userType === "recruiter" ? getRecruiterMsalInstance() : instance;

      // Simply call logout which will clear all accounts and cache automatically
      try {
        await msalInstance.logoutPopup({
          postLogoutRedirectUri: window.location.origin,
          mainWindowRedirectUri: window.location.origin,
        });
      } catch (popupError) {
        console.warn("Popup logout failed, trying redirect:", popupError);
        try {
          await msalInstance.logoutRedirect({
            postLogoutRedirectUri: window.location.origin,
          });
        } catch (redirectError) {
          console.warn(
            "Redirect logout also failed, clearing local state anyway:",
            redirectError,
          );
        }
      }

      toast.success("Signed out successfully");

      setTimeout(() => {
        router.push("/");
      }, 100);
    } catch (error) {
      console.error("Logout failed:", error);

      await logoutFromStore();
      if (typeof window !== "undefined") {
        const savedTheme = localStorage.getItem("theme");
        localStorage.removeItem("auth-storage");
        sessionStorage.clear();

        if (savedTheme) {
          localStorage.setItem("theme", savedTheme);
        }
      }

      setTimeout(() => {
        router.push("/");
      }, 100);
    } finally {
      setIsLoading(false);
    }
  }, [instance, userType, logoutFromStore, router]);

  const mappedUser: UserInfo | null = user
    ? {
        id: user.id,
        email: user.email ?? "",
        name: user.name ?? "User",
        roles: (user as any).roles ?? undefined,
        tenantId: (user as any).tenantId ?? undefined,
      }
    : null;

  return {
    user: mappedUser,
    userType,
    isLoading,
    isAuthenticated: !!user,
    error,
    login,
    logout,
    logoutWithPopup,
    getAccessToken,
    isAuthenticationValid,
  };
}
