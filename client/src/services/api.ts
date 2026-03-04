const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000';

interface ApiResponse<T> {
  data?: T;
  error?: string;
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const token = localStorage.getItem('gymbrain_token');

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  try {
    const res = await fetch(`${API_BASE}${endpoint}`, {
      ...options,
      headers: { ...headers, ...(options.headers as Record<string, string>) },
    });

    if (!res.ok) {
      const text = await res.text();
      let errorMessage: string;
      try {
        const parsed = JSON.parse(text);
        errorMessage = parsed.detail || parsed.title || parsed.message || text;
      } catch {
        errorMessage = text || `Request failed (${res.status})`;
      }
      return { error: errorMessage };
    }

    const data = await res.json();
    return { data };
  } catch (err) {
    return { error: err instanceof Error ? err.message : 'Network error' };
  }
}

// Auth
export interface AuthResponse {
  userId: string;
  token: string;
}

export function register(email: string, password: string, tonePersona?: string) {
  return request<AuthResponse>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ email, password, tonePersona }),
  });
}

export function login(email: string, password: string) {
  return request<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });
}

// Vault
export interface VaultResponse {
  message: string;
}

export interface ILlmModel {
  provider: string;
  modelId: string;
  displayName: string;
  description: string;
  isFree: boolean;
  suitabilityRank: number;
}

export function getLlmModels() {
  return request<ILlmModel[]>('/api/auth/models');
}

export function vaultApiKey(provider: string, apiKey: string, model?: string) {
  return request<VaultResponse>('/api/auth/vault-key', {
    method: 'POST',
    body: JSON.stringify({ provider, apiKey, model }),
  });
}

// Workout
export interface WorkoutResponse {
  megaPayloadJson: string;
}

export function startWorkout(workoutFocus?: string) {
  return request<WorkoutResponse>('/api/workout/start', {
    method: 'POST',
    body: JSON.stringify({ workoutFocus }),
  });
}

export interface SaveWorkoutResponse {
  workoutSessionId: string;
}

export function saveWorkout(payloadJson: string) {
  return request<SaveWorkoutResponse>('/api/workout/save', {
    method: 'POST',
    body: JSON.stringify({ payloadJson }),
  });
}

// Nutrition
export interface NutritionResponse {
  payloadJson: string;
}

export function generateNutritionPlan(diet: string, calories: number, goal: string) {
  return request<NutritionResponse>('/api/nutrition/generate', {
    method: 'POST',
    body: JSON.stringify({ diet, calories, goal }),
  });
}

// Health
export function healthCheck() {
  return request<string>('/');
}
