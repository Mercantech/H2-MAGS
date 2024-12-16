// src/stores/room.store.js
import { writable } from 'svelte/store';
import { apiService } from '../services/api.service';

export const rooms = writable([]);
export const roomDetails = writable([]);
export const isLoading = writable(false);
export const error = writable(null);

export const roomStore = {
    // Load all rooms
    async loadRooms() {
        isLoading.set(true);
        error.set(null);
        try {
            const data = await apiService.getRooms();
            rooms.set(data);
        } catch (err) {
            error.set(err.message);
        } finally {
            isLoading.set(false);
        }
    },

    // Load room details
    async loadRoomDetails() {
        isLoading.set(true);
        error.set(null);
        try {
            const data = await apiService.getRoomDetails();
            roomDetails.set(data);
        } catch (err) {
            error.set(err.message);
        } finally {
            isLoading.set(false);
        }
    },

    // Add new room
    async addRoom(roomData) {
        try {
            const newRoom = await apiService.createRoom(roomData);
            rooms.update(existing => [...existing, newRoom]);
            return newRoom;
        } catch (err) {
            error.set(err.message);
            throw err;
        }
    },

    // Update room
    async updateRoom(id, roomData) {
        try {
            await apiService.updateRoom(id, roomData);
            rooms.update(existing => 
                existing.map(room => 
                    room.id === id ? { ...room, ...roomData } : room
                )
            );
        } catch (err) {
            error.set(err.message);
            throw err;
        }
    },

    // Delete room
    async deleteRoom(id) {
        try {
            await apiService.deleteRoom(id);
            rooms.update(existing => 
                existing.filter(room => room.id !== id)
            );
        } catch (err) {
            error.set(err.message);
            throw err;
        }
    }
};