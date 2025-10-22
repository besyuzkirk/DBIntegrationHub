import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

export interface User {
  id: string;
  username: string;
  email: string;
  isActive: boolean;
  lastLoginAt: string | null;
  roles: string[];
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  
  // Actions
  setAuth: (user: User, token: string) => void;
  setUser: (user: User) => void;
  logout: () => void;
  hasRole: (role: string) => boolean;
}

// Zustand store
export const useStore = create<AuthState>()(
  devtools(
    persist(
      (set, get) => ({
        user: null,
        token: null,
        isAuthenticated: false,
        
        setAuth: (user, token) =>
          set({ user, token, isAuthenticated: true }, false, 'setAuth'),
        
        setUser: (user) =>
          set({ user }, false, 'setUser'),
        
        logout: () => {
          // Clear localStorage
          localStorage.removeItem('token');
          set({ user: null, token: null, isAuthenticated: false }, false, 'logout');
        },
        
        hasRole: (role) => {
          const state = get();
          return state.user?.roles.includes(role) ?? false;
        },
      }),
      {
        name: 'auth-storage', // localStorage key
      }
    )
  )
);

