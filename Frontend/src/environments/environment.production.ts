// For a frontend served from the same origin as the API.
// If deployed separately, set this to the deployed HTTPS API URL.
export const environment = {
  production: true,
  apiBaseUrl: '/api'
} as const;
