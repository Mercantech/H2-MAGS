import { writable } from 'svelte/store';
import { roomsService } from '$lib/services/api';

export interface Room {
	id: string;
	number: string;
	capacity: number;
	hotelId: string;
	hotel?: {
		id: string;
		name: string;
		address: string;
	};
}

export const rooms = writable<Room[]>([]);
export const availableRooms = writable<Room[]>([]);
export const isLoading = writable<boolean>(false);
export const error = writable<string | null>(null);

export async function loadRooms() {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await roomsService.getAll();
		rooms.set(response.data || response);
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af værelser');
		console.error('Error loading rooms:', err);
	} finally {
		isLoading.set(false);
	}
}

export async function loadAvailableRooms(startDate: string, endDate: string) {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await roomsService.getAvailable(startDate, endDate);
		availableRooms.set(response.data || response);
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af ledige værelser');
		console.error('Error loading available rooms:', err);
	} finally {
		isLoading.set(false);
	}
}
