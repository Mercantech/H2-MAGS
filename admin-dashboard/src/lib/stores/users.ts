import { writable } from 'svelte/store';
import { usersService } from '$lib/services/api';

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

export const users = writable<User[]>([]);
export const isLoading = writable<boolean>(false);
export const error = writable<string | null>(null);

export async function loadUsers() {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await usersService.getAll();
		users.set(response.data || response);
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af brugere');
		console.error('Error loading users:', err);
	} finally {
		isLoading.set(false);
	}
}

export async function loadUserById(id: string) {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await usersService.getById(id);
		return response.data || response;
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af bruger');
		console.error('Error loading user:', err);
		return null;
	} finally {
		isLoading.set(false);
	}
}
