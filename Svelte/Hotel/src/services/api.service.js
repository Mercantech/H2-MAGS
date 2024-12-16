// src/services/api.service.js
import { auth } from "../stores/auth.store";
import { authService } from "./auth.service";

const API_BASE_URL =
  import.meta.env.VITE_API_URL || "https://localhost:7207/api";

const getHeaders = () => {
  const token = localStorage.getItem("token");
  return {
    "Content-Type": "application/json",
    Authorization: token ? `Bearer ${token}` : "",
  };
};

const handleError = async (error) => {
  if (error.status === 401) {
    // Token er udløbet eller ugyldig
    try {
      await authService.refreshToken();
      // Gentag det oprindelige kald med ny token
      return true;
    } catch (refreshError) {
      // Refresh token fejlede, log brugeren ud
      auth.logout();
      window.location.href = "/login";
      return false;
    }
  }
  throw new Error(
    error.message || "An error occurred while communicating with the API"
  );
};

const handleResponse = async (response) => {
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || "API request failed");
  }
  return response.json();
};

export const apiService = {
  // Get all rooms
  async getRooms() {
    try {
      const response = await fetch(`${API_BASE_URL}/rooms`, {
        headers: getHeaders(),
      });

      if (!response.ok) {
        const shouldRetry = await handleError(response);
        if (shouldRetry) {
          return this.getRooms();
        }
      }

      return response.json();
    } catch (error) {
      return handleError(error);
    }
  },

  // Get room details
  async getRoomDetails() {
    try {
      const response = await fetch(`${API_BASE_URL}/rooms/details`);
      return handleResponse(response);
    } catch (error) {
      return handleError(error);
    }
  },

  // Get specific room
  async getRoom(id) {
    try {
      const response = await fetch(`${API_BASE_URL}/rooms/${id}`);
      return handleResponse(response);
    } catch (error) {
      return handleError(error);
    }
  },

  // Create new room
  async createRoom(roomData) {
    try {
      const response = await fetch(`${API_BASE_URL}/rooms`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(roomData),
      });
      return handleResponse(response);
    } catch (error) {
      return handleError(error);
    }
  },

  // Update room
  async updateRoom(id, roomData) {
    try {
      const response = await fetch(`${API_BASE_URL}/rooms/${id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(roomData),
      });
      return handleResponse(response);
    } catch (error) {
      return handleError(error);
    }
  },

  // Delete room
  async deleteRoom(id) {
    try {
      const response = await fetch(`${API_BASE_URL}/rooms/${id}`, {
        method: "DELETE",
      });
      return handleResponse(response);
    } catch (error) {
      return handleError(error);
    }
  },

  // Get guests
  async getGuests() {
    try {
      const response = await fetch(`${API_BASE_URL}/users/guests`, {
        headers: getHeaders(),
      });

      if (!response.ok) {
        const shouldRetry = await handleError(response);
        if (shouldRetry) {
          return this.getGuests();
        }
      }

      return response.json();
    } catch (error) {
      return handleError(error);
    }
  },
};
