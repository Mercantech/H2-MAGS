import { writable } from 'svelte/store';
import { hotelsService } from '$lib/services/api';

export interface Hotel {
	id: string;
	name: string;
	address?: string;
	phone?: string;
	email?: string;
}

export const hotels = writable<Hotel[]>([]);
export const isLoading = writable<boolean>(false);
export const error = writable<string | null>(null);

export async function loadHotels() {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await hotelsService.getAll();
		hotels.set(response.data || response);
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af hoteller');
		console.error('Error loading hotels:', err);
	} finally {
		isLoading.set(false);
	}
}

export async function loadHotelById(id: string) {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await hotelsService.getById(id);
		return response.data || response;
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af hotel');
		console.error('Error loading hotel:', err);
		return null;
	} finally {
		isLoading.set(false);
	}
}
