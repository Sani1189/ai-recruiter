/**
 * Centralized Environment Configuration
 *
 * This utility provides a clean, type-safe way to access environment variables
 * with proper fallbacks for Azure Static Web Apps deployment.
 *
 * For Azure Static Web Apps:
 * - Set environment variables in Azure Portal > Configuration > Application settings
 * - For build-time variables (NEXT_PUBLIC_*), also set them in Build settings
 * - Variables are available at build time and runtime
 */

// Client-side public environment variables (embedded at build time)
// These are safe to expose in the browser
export const env = {
  // API Configuration
  // For Azure Static Web Apps: Set NEXT_PUBLIC_API_BASE_URL in Azure Portal > Configuration
  apiBaseUrl:
    process.env.NEXT_PUBLIC_API_BASE_URL ||
    (typeof window !== "undefined" && process.env.NODE_ENV === "development"
      ? window.location.origin + "/api"
      : "http://localhost:8000/api"),

  saasApiBaseUrl:
    process.env.NEXT_PUBLIC_SAAS_API_BASE_URL || "http://localhost:5127/api",

  // Azure AD Configuration (Recruiter/Admin)
  azureAd: {
    clientId:
      process.env.NEXT_PUBLIC_AZURE_AD_CLIENT_ID ||
      "f220c4ef-f682-4ce7-9077-d740c56c446d",
    tenantId:
      process.env.NEXT_PUBLIC_AZURE_AD_TENANT_ID ||
      "c3ac4169-9d99-4eb7-b72b-bfa11f13d825",
    authority:
      process.env.NEXT_PUBLIC_AZURE_AD_AUTHORITY ||
      "https://login.microsoftonline.com/c3ac4169-9d99-4eb7-b72b-bfa11f13d825",
    scope:
      process.env.NEXT_PUBLIC_AZURE_AD_SCOPE ||
      "api://5a07ecbd-3a11-4faf-93e8-01277609dccc/User.Read",
  },

  // Azure B2C Configuration (Candidate)
  b2c: {
    clientId:
      process.env.NEXT_PUBLIC_B2C_CLIENT_ID ||
      "c1c356f0-9cfb-4a10-a0ca-e1ba4a721e24",
    tenantId:
      process.env.NEXT_PUBLIC_B2C_TENANT_ID ||
      "f6e5ae67-797a-4f3e-a8f4-3f73abc7c76f",
    authority:
      process.env.NEXT_PUBLIC_B2C_AUTHORITY ||
      "https://osilionrecruitment.ciamlogin.com/f6e5ae67-797a-4f3e-a8f4-3f73abc7c76f/v2.0",
    apiScope:
      process.env.NEXT_PUBLIC_B2C_API_SCOPE ||
      "api://4246b98e-ea61-45d1-b454-755406deac59/ApiAccess",
  },

  // API Configuration Options
  api: {
    timeout: parseInt(process.env.NEXT_PUBLIC_API_TIMEOUT || "30000"),
    retryAttempts: parseInt(process.env.NEXT_PUBLIC_API_RETRY_ATTEMPTS || "3"),
    retryDelay: parseInt(process.env.NEXT_PUBLIC_API_RETRY_DELAY || "1000"),
    cacheTimeout: parseInt(
      process.env.NEXT_PUBLIC_API_CACHE_TIMEOUT || "300000",
    ),
    maxConcurrentRequests: parseInt(
      process.env.NEXT_PUBLIC_API_MAX_CONCURRENT || "10",
    ),
  },

  // Environment
  isDevelopment: process.env.NODE_ENV === "development",
  isProduction: process.env.NODE_ENV === "production",
} as const;

/**
 * Server-side only environment variables
 * These should NOT be exposed to the client
 * Only accessible in Next.js API routes and server components
 */
export const serverEnv = {
  // OpenAI Configuration (server-side only)
  openai: {
    apiKey: process.env.OPENAI_API_KEY || "",
  },

  AzureFunctions: {
    url: process.env.CV_EXTRACTION_API_BASE_URL || "",
  },
  // Other server-side variables
  vercelProjectProductionUrl: process.env.VERCEL_PROJECT_PRODUCTION_URL || "",
} as const;

/**
 * Validate required environment variables
 * Call this in your app initialization to catch missing variables early
 */
export function validateEnv() {
  const missing: string[] = [];

  if (!env.apiBaseUrl) {
    missing.push("NEXT_PUBLIC_API_BASE_URL");
  }

  if (!env.azureAd.clientId) {
    missing.push("NEXT_PUBLIC_AZURE_AD_CLIENT_ID");
  }

  if (!env.b2c.clientId) {
    missing.push("NEXT_PUBLIC_B2C_CLIENT_ID");
  }

  if (missing.length > 0 && env.isDevelopment) {
    console.warn("Missing environment variables:", missing.join(", "));
  }

  return missing.length === 0;
}
