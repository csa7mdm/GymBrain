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
    'ngrok-skip-browser-warning': 'true',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  try {
    const res = await fetch(`${API_BASE}${endpoint}`, {
      ...options,
      headers: { ...headers, ...(options.headers as Record<string, string>) },
    });

    if (!res.ok) {
      if (res.status === 401 && !endpoint.includes('/auth/login')) {
        Object.keys(localStorage).forEach(key => {
          if (key.startsWith('gymbrain_')) {
            localStorage.removeItem(key);
          }
        });
        // Force reload to trigger AuthContext reset
        window.location.href = '/';
        return { error: 'Session expired. Please sign in again.' };
      }
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

// Substitute (Machine Taken — Phase 2)
export interface SubstituteOption {
  exerciseId: string;
  name: string;
  equipment: string;
  reason: string;
}

export interface SubstituteResponse {
  originalExerciseId: string;
  originalExerciseName: string;
  substitutes: SubstituteOption[];
  message?: string;
}

export function getSubstitute(exerciseId: string) {
  return request<SubstituteResponse>('/api/workout/substitute', {
    method: 'POST',
    body: JSON.stringify({ exerciseId }),
  });
}

// Events / Telemetry (Phase 6 — fire-and-forget, never blocks user)
export function trackEvent(eventName: string, metadata?: object): void {
  const token = localStorage.getItem('gymbrain_token');
  if (!token) return;
  fetch(`${API_BASE}/api/events`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify({ eventName, metadata }),
  }).catch(() => { }); // Silent fail — telemetry must never break the app
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

// Profile
export interface ProfileData {
  goal: string;
  equipmentJson: string;
  injuries: string;
  daysPerWeek: number;
  dietaryPreference: string;
  dailyCalories: number;
}

export function getProfile() {
  return request<ProfileData>('/api/profile');
}

export function saveProfile(profile: ProfileData) {
  return request<void>('/api/profile/save', {
    method: 'POST',
    body: JSON.stringify(profile),
  });
}
