import axios from 'axios';

const API_BASE_URL = 'http://localhost:8071/api'; // https://25h2-mags.mercantec.tech/api

export const api = axios.create({
	baseURL: API_BASE_URL,
	headers: {
		'Content-Type': 'application/json'
	}
});

// Request interceptor til at tilføje JWT token
api.interceptors.request.use((config) => {
	if (typeof window !== 'undefined') {
		const token = localStorage.getItem('jwt_token');
		if (token) {
			config.headers.Authorization = `Bearer ${token}`;
		}
	}
	return config;
});

// Response interceptor til fejlhåndtering
api.interceptors.response.use(
	(response) => response,
	(error) => {
		if (error.response?.status === 401) {
			// Token udløbet eller ugyldig
			if (typeof window !== 'undefined') {
				localStorage.removeItem('jwt_token');
				window.location.href = '/admin/login';
			}
		}
		return Promise.reject(error);
	}
);

// Auth service
export const authService = {
	async login(username: string, password: string) {
		const response = await api.post('/Auth/ad-login', { username, password });
		return response.data;
	},

	async getCurrentUser() {
		const response = await api.get('/Auth/ad-me');
		return response.data;
	},

	async getADStatus() {
		const response = await api.get('/Auth/ad-status');
		return response.data;
	}
};

// Bookings service
export const bookingsService = {
	async getAll() {
		const response = await api.get('/bookings');
		return response.data;
	},

	async getById(id: string) {
		const response = await api.get(`/bookings/${id}`);
		return response.data;
	},

	async create(booking: any) {
		const response = await api.post('/bookings', booking);
		return response.data;
	},

	async update(id: string, booking: any) {
		const response = await api.put(`/bookings/${id}`, booking);
		return response.data;
	},

	async delete(id: string) {
		const response = await api.delete(`/bookings/${id}`);
		return response.data;
	},

	async getByDateRange(startDate: string, endDate: string) {
		const response = await api.get(`/bookings/date-range?start=${startDate}&end=${endDate}`);
		return response.data;
	}
};

// Rooms service
export const roomsService = {
	async getAll() {
		const response = await api.get('/rooms');
		return response.data;
	},

	async getById(id: string) {
		const response = await api.get(`/rooms/${id}`);
		return response.data;
	},

	async getAvailable(startDate: string, endDate: string) {
		const response = await api.get(`/rooms/available?start=${startDate}&end=${endDate}`);
		return response.data;
	}
};

// Hotels service
export const hotelsService = {
	async getAll() {
		const response = await api.get('/hotels');
		return response.data;
	},

	async getById(id: string) {
		const response = await api.get(`/hotels/${id}`);
		return response.data;
	}
};

// Users service
export const usersService = {
	async getAll() {
		const response = await api.get('/users');
		return response.data;
	},

	async getById(id: string) {
		const response = await api.get(`/users/${id}`);
		return response.data;
	}
};

// Admin service
export const adminService = {
	async getDashboardStats() {
		const response = await api.get('/admin/dashboard/stats');
		return response.data;
	},

	async getRecentBookings(limit: number = 5) {
		const response = await api.get(`/admin/dashboard/recent-bookings?limit=${limit}`);
		return response.data;
	},

	async getRoomAvailability() {
		const response = await api.get('/admin/dashboard/room-availability');
		return response.data;
	},

	async getBookingsByDate(date: string) {
		const response = await api.get(`/admin/bookings/by-date?date=${date}`);
		return response.data;
	},

	async getAvailableRooms(startDate: string, endDate: string) {
		const response = await api.get(`/admin/rooms/available?startDate=${startDate}&endDate=${endDate}`);
		return response.data;
	},

	async getDailyCheckIns() {
		const response = await api.get('/admin/daily-checkins');
		return response.data;
	}
};
