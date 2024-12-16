// src/stores/auth.store.js
import { writable } from 'svelte/store';
import { authService } from '../services/auth.service';

function createAuthStore() {
    const { subscribe, set, update } = writable({
        isAuthenticated: !!localStorage.getItem('token'),
        user: null,
        loading: false,
        error: null
    });

    return {
        subscribe,
        login: async (email, password) => {
            update(state => ({ ...state, loading: true, error: null }));
            try {
                const response = await authService.login(email, password);
                update(state => ({
                    ...state,
                    isAuthenticated: true,
                    user: response.user,
                    loading: false
                }));
            } catch (error) {
                update(state => ({
                    ...state,
                    error: error.message,
                    loading: false
                }));
                throw error;
            }
        },
        register: async (userData) => {
            update(state => ({ ...state, loading: true, error: null }));
            try {
                await authService.register(userData);
                update(state => ({ ...state, loading: false }));
            } catch (error) {
                update(state => ({
                    ...state,
                    error: error.message,
                    loading: false
                }));
                throw error;
            }
        },
        logout: () => {
            authService.logout();
            set({
                isAuthenticated: false,
                user: null,
                loading: false,
                error: null
            });
        }
    };
}

export const auth = createAuthStore();