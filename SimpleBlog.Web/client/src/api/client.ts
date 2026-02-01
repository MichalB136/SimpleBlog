const API_BASE = '/api';

interface RequestOptions extends RequestInit {
  headers?: Record<string, string>;
}

export interface ValidationError {
  title: string;
  errors?: Record<string, string[]>;
  [key: string]: any;
}

export class ApiError extends Error {
  constructor(
    message: string,
    public validationErrors?: Record<string, string[]>
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

async function apiRequest<T>(
  path: string,
  options: RequestOptions = {}
): Promise<T> {
  const token = localStorage.getItem('authToken');
  
  // Don't set Content-Type for FormData - browser will set it automatically with boundary
  const headers: Record<string, string> = {
    ...options.headers,
  };
  
  // Only set Content-Type for JSON if body is not FormData
  if (!(options.body instanceof FormData) && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json';
  }

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  const contentType = response.headers.get('content-type');
  const isJson = contentType?.includes('application/json');
  let data: any;
  if (isJson) {
    // Defensive: some endpoints may return an empty body with a JSON content-type.
    // Read as text first and parse only if non-empty to avoid "Unexpected end of JSON input".
    const text = await response.text();
    data = text && text.trim().length > 0 ? JSON.parse(text) : null;
  } else {
    data = await response.text();
  }

  if (!response.ok) {
    console.log('Error response data:', data, 'Type:', typeof data);
    
    // If data has validation errors, create an ApiError with validation errors
    if (data && typeof data === 'object' && data.errors && typeof data.errors === 'object') {
      console.log('Validation error detected, creating ApiError');
      throw new ApiError(data.title || 'An error occurred', data.errors);
    }
    
    // If data is a string that looks like JSON, try to parse it
    if (typeof data === 'string' && data.startsWith('{')) {
      try {
        const parsedData = JSON.parse(data);
        console.log('Parsed error from string:', parsedData);
        if (parsedData.errors && typeof parsedData.errors === 'object') {
          throw new ApiError(parsedData.title || 'An error occurred', parsedData.errors);
        }
        throw new Error(parsedData.title || data);
      } catch (parseErr) {
        console.log('Could not parse error string as JSON');
        throw new Error(data);
      }
    }
    
    const errorMessage = typeof data === 'string' ? data : (data?.title || 'An error occurred');
    console.log('Throwing generic error:', errorMessage);
    throw new Error(errorMessage);
  }

  return data as T;
}

export const apiClient = {
  get: <T,>(path: string) => apiRequest<T>(path, { method: 'GET' }),
  post: <T,>(path: string, body?: unknown) =>
    apiRequest<T>(path, {
      method: 'POST',
      body: body instanceof FormData ? body : (body ? JSON.stringify(body) : undefined),
    }),
  put: <T,>(path: string, body?: unknown) =>
    apiRequest<T>(path, {
      method: 'PUT',
      body: body instanceof FormData ? body : (body ? JSON.stringify(body) : undefined),
    }),
  delete: <T,>(path: string) => apiRequest<T>(path, { method: 'DELETE' }),
};
