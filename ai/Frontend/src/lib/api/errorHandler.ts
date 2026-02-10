// Centralized Error Handler for API Errors
import { ApiError, HTTP_STATUS } from './config';
import { toast } from 'sonner';

export interface ErrorHandlerOptions {
  /**
   * Custom error messages for specific error codes
   * Example: { 'DUPLICATE_ENTRY': 'This item already exists' }
   */
  customMessages?: Record<string, string>;
  
  /**
   * Custom error titles for specific error codes
   * Example: { 'DUPLICATE_ENTRY': 'Duplicate Entry' }
   */
  customTitles?: Record<string, string>;
  
  /**
   * If true, shows a toast notification automatically
   * @default true
   */
  showToast?: boolean;
  
  /**
   * If true, logs the error to console (useful for debugging)
   * @default true in development
   */
  logError?: boolean;
  
  /**
   * Default fallback message if no specific handling is found
   */
  defaultMessage?: string;
  
  /**
   * Callback function to execute after error is handled
   */
  onError?: (error: ParsedError) => void;
}

export interface ParsedError {
  /** HTTP status code */
  status: number;
  /** Main error message */
  message: string;
  /** Error code from backend (e.g., 'DUPLICATE_ENTRY') */
  code?: string;
  /** Additional details */
  details?: string;
  /** Field that caused the error */
  field?: string;
  /** Array of validation errors */
  validationErrors?: string[];
  /** When the error occurred */
  timestamp?: string;
  /** Error type classification */
  type: 'validation' | 'duplicate' | 'notFound' | 'unauthorized' | 'forbidden' | 'server' | 'network' | 'unknown';
  /** User-friendly title */
  title: string;
  /** User-friendly description */
  description: string;
}

/**
 * Parse an API error into a structured format
 */
export function parseApiError(error: any): ParsedError {
  // Handle ApiError instances
  if (error instanceof ApiError) {
    const type = classifyErrorType(error.status, error.code);
    
    return {
      status: error.status,
      message: error.message,
      code: error.code,
      details: (error as any).details,
      field: (error as any).field,
      validationErrors: error.errors,
      timestamp: (error as any).timestamp,
      type,
      title: generateErrorTitle(type, error.code),
      description: generateErrorDescription(error),
    };
  }
  
  // Handle standard Error instances
  if (error instanceof Error) {
    // Special handling for circuit breaker errors
    if (error.message.includes('Circuit breaker is open')) {
      return {
        status: 503,
        message: 'Service temporarily unavailable',
        type: 'server',
        title: 'Service Temporarily Unavailable',
        description: 'The service is experiencing issues. Please try again in a few moments.',
      };
    }
    
    return {
      status: 0,
      message: error.message,
      type: 'unknown',
      title: 'Error',
      description: error.message,
    };
  }
  
  // Handle string errors
  if (typeof error === 'string') {
    return {
      status: 0,
      message: error,
      type: 'unknown',
      title: 'Error',
      description: error,
    };
  }
  
  // Handle unknown error types
  return {
    status: 0,
    message: 'An unexpected error occurred',
    type: 'unknown',
    title: 'Unexpected Error',
    description: 'An unexpected error occurred. Please try again.',
  };
}

/**
 * Classify error type based on status code and error code
 */
function classifyErrorType(
  status: number,
  code?: string
): ParsedError['type'] {
  // Check error code first
  if (code) {
    if (code.includes('DUPLICATE') || code.includes('EXISTS')) return 'duplicate';
    if (code.includes('VALIDATION') || code.includes('INVALID')) return 'validation';
    if (code.includes('NOT_FOUND') || code.includes('MISSING')) return 'notFound';
  }
  
  // Check status code
  switch (status) {
    case HTTP_STATUS.BAD_REQUEST:
      return 'validation';
    case HTTP_STATUS.UNAUTHORIZED:
      return 'unauthorized';
    case HTTP_STATUS.FORBIDDEN:
      return 'forbidden';
    case HTTP_STATUS.NOT_FOUND:
      return 'notFound';
    case HTTP_STATUS.CONFLICT:
      return 'duplicate';
    case HTTP_STATUS.UNPROCESSABLE_ENTITY:
      return 'validation';
    case HTTP_STATUS.INTERNAL_SERVER_ERROR:
    case HTTP_STATUS.SERVICE_UNAVAILABLE:
      return 'server';
    case 0:
      return 'network';
    default:
      return 'unknown';
  }
}

/**
 * Generate user-friendly error title
 */
function generateErrorTitle(type: ParsedError['type'], code?: string): string {
  const titleMap: Record<ParsedError['type'], string> = {
    validation: 'Validation Error',
    duplicate: 'Already Exists',
    notFound: 'Not Found',
    unauthorized: 'Authentication Required',
    forbidden: 'Access Denied',
    server: 'Server Error',
    network: 'Network Error',
    unknown: 'Error',
  };
  
  return titleMap[type];
}

/**
 * Generate user-friendly error description
 */
function generateErrorDescription(error: ApiError): string {
  const details = (error as any).details;
  const validationErrors = error.errors;
  const field = (error as any).field;
  
  // If we have validation errors, format them nicely
  if (validationErrors && validationErrors.length > 0) {
    if (validationErrors.length === 1) {
      return validationErrors[0];
    }
    return validationErrors.join('; ');
  }
  
  // If we have details, use them
  if (details) {
    return `${error.message}${field ? ` (Field: ${field})` : ''}. ${details}`;
  }
  
  // Otherwise, use the main message with field if available
  return `${error.message}${field ? ` (Field: ${field})` : ''}`;
}

/**
 * Handle API error with optional custom messages and automatic toast notification
 */
export function handleApiError(
  error: any,
  options: ErrorHandlerOptions = {}
): ParsedError {
  const {
    customMessages = {},
    customTitles = {},
    showToast = true,
    logError = process.env.NODE_ENV === 'development',
    defaultMessage = 'An unexpected error occurred. Please try again.',
    onError,
  } = options;
  
  // Parse the error
  const parsedError = parseApiError(error);
  
  // Apply custom messages if provided
  if (parsedError.code && customMessages[parsedError.code]) {
    parsedError.description = customMessages[parsedError.code];
  } else if (!parsedError.description) {
    parsedError.description = defaultMessage;
  }
  
  // Apply custom titles if provided
  if (parsedError.code && customTitles[parsedError.code]) {
    parsedError.title = customTitles[parsedError.code];
  }
  
  // Log error in development
  if (logError) {
    console.error('API Error:', {
      ...parsedError,
      originalError: error,
    });
  }
  
  // Show toast notification
  if (showToast) {
    // Dismiss any existing loading toasts
    toast.dismiss();
    
    // Choose appropriate toast variant based on error type
    const isWarning = parsedError.type === 'duplicate' || parsedError.type === 'validation';
    
    if (isWarning) {
      toast.warning(parsedError.title, {
        description: parsedError.description,
      });
    } else {
      toast.error(parsedError.title, {
        description: parsedError.description,
      });
    }
  }
  
  // Execute callback if provided
  if (onError) {
    onError(parsedError);
  }
  
  return parsedError;
}

/**
 * Quick helper to check if error is a specific type
 */
export function isErrorType(error: any, type: ParsedError['type']): boolean {
  const parsed = parseApiError(error);
  return parsed.type === type;
}

/**
 * Quick helper to check if error has specific code
 */
export function hasErrorCode(error: any, code: string): boolean {
  const parsed = parseApiError(error);
  return parsed.code === code;
}

/**
 * Get user-friendly error message from any error
 */
export function getErrorMessage(error: any, fallback = 'An unexpected error occurred'): string {
  const parsed = parseApiError(error);
  return parsed.description || parsed.message || fallback;
}

/**
 * Format validation errors as a list
 */
export function formatValidationErrors(errors?: string[]): string {
  if (!errors || errors.length === 0) return '';
  
  if (errors.length === 1) {
    return errors[0];
  }
  
  return errors.map((err, idx) => `${idx + 1}. ${err}`).join('\n');
}

