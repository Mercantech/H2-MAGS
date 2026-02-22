import { writable } from 'svelte/store';
import { browser } from '$app/environment';
import { authService } from '$lib/services/api';

export interface User {
	id: string;
	email: string;
	username: string;
	role: string;
	isADUser?: boolean;
	samAccountName?: string;
	displayName?: string;
	adGroups?: string[];
}

export const user = writable<User | null>(null);
export const isAuthenticated = writable<boolean>(false);
export const isLoading = writable<boolean>(false);

// Initialize auth state from localStorage
if (browser) {
	const token = localStorage.getItem('jwt_token');
	if (token) {
		isAuthenticated.set(true);
		// Try to get user info
		authService.getCurrentUser()
			.then(userData => {
				user.set(userData);
			})
			.catch(() => {
				// Token might be invalid, clear it
				localStorage.removeItem('jwt_token');
				isAuthenticated.set(false);
			});
	}
}

export async function login(username: string, password: string) {
	isLoading.set(true);
	try {
		const response = await authService.login(username, password);
		const { token, user: userData } = response;
		
		if (browser) {
			localStorage.setItem('jwt_token', token);
		}
		user.set(userData);
		isAuthenticated.set(true);
		
		return { success: true, user: userData };
	} catch (error: any) {
		return { 
			success: false, 
			error: error.response?.data?.message || 'Login fejlede' 
		};
	} finally {
		isLoading.set(false);
	}
}

export function logout() {
	if (browser) {
		localStorage.removeItem('jwt_token');
	}
	user.set(null);
	isAuthenticated.set(false);
}
