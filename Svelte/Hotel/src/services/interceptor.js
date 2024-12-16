// src/services/interceptor.js
import { auth } from '../stores/auth.store';
import { authService } from './auth.service';

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
    failedQueue.forEach(prom => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });
    
    failedQueue = [];
};

export const setupInterceptors = () => {
    window.fetch = new Proxy(window.fetch, {
        apply: async function(fetch, that, args) {
            let [resource, config] = args;
            
            // Tilføj token hvis den findes
            if (localStorage.getItem('token')) {
                config = config || {};
                config.headers = config.headers || {};
                config.headers.Authorization = `Bearer ${localStorage.getItem('token')}`;
            }

            try {
                const response = await fetch.apply(that, [resource, config]);
                
                if (response.status === 401) {
                    if (!isRefreshing) {
                        isRefreshing = true;
                        
                        try {
                            await authService.refreshToken();
                            isRefreshing = false;
                            processQueue(null, localStorage.getItem('token'));
                            
                            // Gentag det oprindelige kald med ny token
                            config.headers.Authorization = `Bearer ${localStorage.getItem('token')}`;
                            return fetch.apply(that, [resource, config]);
                        } catch (error) {
                            processQueue(error, null);
                            auth.logout();
                            window.location.href = '/login';
                            throw error;
                        }
                    } else {
                        // Vent på at refresh er færdig
                        return new Promise((resolve, reject) => {
                            failedQueue.push({ resolve, reject });
                        }).then(() => {
                            config.headers.Authorization = `Bearer ${localStorage.getItem('token')}`;
                            return fetch.apply(that, [resource, config]);
                        });
                    }
                }
                
                return response;
            } catch (error) {
                throw error;
            }
        }
    });
};