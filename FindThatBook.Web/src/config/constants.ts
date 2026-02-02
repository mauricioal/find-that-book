/**
 * Centralized configuration for the application.
 * Values are pulled from environment variables.
 */
export const CONFIG = {
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5249/api',
  OPENLIBRARY_URL: import.meta.env.VITE_OPENLIBRARY_URL || 'https://openlibrary.org',
};
