import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

import { EvnVariablesType } from "@/types/type";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * A function to get environment variables
 * @param name Name of the environment variable
 * @returns Value of the environment variable
 * @throws Throws "Missing environment variable ${name}" if the variable is not found
 */
export function getEnv(name: keyof EvnVariablesType) {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing environment variable ${name}`);
  }
  return value;
}

/**
 * A function to get the origin URL based on the environment
 * @returns The origin URL for the application
 */
export function getOrigin() {
  // If in development, return localhost URL
  if (process.env.NODE_ENV === "development") {
    return "http://localhost:3000";
  }

  // If not in browser, return the production URL from environment variables
  if (typeof window === "undefined") {
    return "https://" + getEnv("VERCEL_PROJECT_PRODUCTION_URL");
  }

  // If in browser, return the location origin
  return window.location.origin;
}

export function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}
