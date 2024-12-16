// src/services/auth.service.js
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7207/api';

export const authService = {
    async login(email, password) {
        try {
            console.log(API_BASE_URL);
            console.log(JSON.stringify({ email, password }));
            const response = await fetch(`${API_BASE_URL}/users/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email, password })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message);
            }

            const data = await response.json();
            localStorage.setItem('token', data.accessToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            return data;
        } catch (error) {
            throw new Error(error.message || 'Login fejlede');
        }
    },

    async register(userData) {
        try {
            const response = await fetch(`${API_BASE_URL}/users/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(userData)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message);
            }

            return response.json();
        } catch (error) {
            throw new Error(error.message || 'Registrering fejlede');
        }
    },

    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
    },

    async refreshToken() {
        const accessToken = localStorage.getItem('token');
        const refreshToken = localStorage.getItem('refreshToken');

        if (!accessToken || !refreshToken) {
            throw new Error('Ingen tokens fundet');
        }

        try {
            const response = await fetch(`${API_BASE_URL}/users/refresh-token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ accessToken, refreshToken })
            });

            const data = await response.json();
            localStorage.setItem('token', data.accessToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            return data;
        } catch (error) {
            this.logout();
            throw new Error('Session udløbet - log venligst ind igen');
        }
    }
};