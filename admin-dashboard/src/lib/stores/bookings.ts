import { writable } from 'svelte/store';
import { bookingsService } from '$lib/services/api';

export interface Booking {
	id: string;
	userId: string;
	roomId: string;
	startDate: string;
	endDate: string;
	createdAt: string;
	updatedAt: string;
	user?: {
		id: string;
		email: string;
		username: string;
	};
	room?: {
		id: string;
		number: string;
		capacity: number;
		hotelId: string;
		hotel?: {
			id: string;
			name: string;
		};
	};
}

export const bookings = writable<Booking[]>([]);
export const isLoading = writable<boolean>(false);
export const error = writable<string | null>(null);

export async function loadBookings() {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await bookingsService.getAll();
		bookings.set(response.data || response);
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af bookinger');
		console.error('Error loading bookings:', err);
	} finally {
		isLoading.set(false);
	}
}

export async function loadBookingsByDateRange(startDate: string, endDate: string) {
	isLoading.set(true);
	error.set(null);
	try {
		const response = await bookingsService.getByDateRange(startDate, endDate);
		bookings.set(response.data || response);
	} catch (err: any) {
		error.set(err.response?.data?.message || 'Fejl ved indlæsning af bookinger');
		console.error('Error loading bookings by date range:', err);
	} finally {
		isLoading.set(false);
	}
}

export async function createBooking(bookingData: Partial<Booking>) {
	try {
		const response = await bookingsService.create(bookingData);
		// Reload bookings to get updated list
		await loadBookings();
		return { success: true, booking: response };
	} catch (err: any) {
		return { 
			success: false, 
			error: err.response?.data?.message || 'Fejl ved oprettelse af booking' 
		};
	}
}

export async function updateBooking(id: string, bookingData: Partial<Booking>) {
	try {
		const response = await bookingsService.update(id, bookingData);
		// Reload bookings to get updated list
		await loadBookings();
		return { success: true, booking: response };
	} catch (err: any) {
		return { 
			success: false, 
			error: err.response?.data?.message || 'Fejl ved opdatering af booking' 
		};
	}
}

export async function deleteBooking(id: string) {
	try {
		await bookingsService.delete(id);
		// Reload bookings to get updated list
		await loadBookings();
		return { success: true };
	} catch (err: any) {
		return { 
			success: false, 
			error: err.response?.data?.message || 'Fejl ved sletning af booking' 
		};
	}
}
