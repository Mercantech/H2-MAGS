<!-- src/lib/components/auth/AuthGuard.svelte -->
<script>
    import { onMount } from 'svelte';
    import { fade, fly } from 'svelte/transition';
    import { auth } from '../../../stores/auth.store';
    import Login from './Login.svelte';
    
    let isAuthenticated = false;
    let isLoading = true;

    onMount(async () => {
        const token = localStorage.getItem('token');
        if (!token) {
            isLoading = false;
            return;
        }
        await checkAuth();
    });

    async function checkAuth() {
        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('No token');
            }

            const tokenData = JSON.parse(atob(token.split('.')[1]));
            const expirationTime = tokenData.exp * 1000;
            
            if (Date.now() >= expirationTime) {
                throw new Error('Token expired');
            }

            isAuthenticated = true;
        } catch (error) {
            auth.logout();
            isAuthenticated = false;
        } finally {
            setTimeout(() => {
                isLoading = false;
            }, 300);
        }
    }

    auth.subscribe(state => {
        isAuthenticated = state.isAuthenticated;
    });
</script>

<div class="min-h-screen">
    {#if isLoading}
        <div 
            class="flex items-center justify-center h-screen bg-slate-50"
            in:fade={{ duration: 200 }}
            out:fade={{ duration: 200 }}
        >
            <div class="flex flex-col items-center">
                <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
                <p class="mt-4 text-slate-600">Indlæser...</p>
            </div>
        </div>
    {:else if !isAuthenticated}
        <div 
            in:fly={{ y: 20, duration: 300, delay: 300 }}
            out:fade={{ duration: 200 }}
        >
            <Login />
        </div>
    {:else}
        <div 
            in:fly={{ y: 20, duration: 300, delay: 300 }}
            out:fade={{ duration: 200 }}
        >
            <slot />
        </div>
    {/if}
</div>

<style>
    :global(body) {
        margin: 0;
        padding: 0;
        background-color: rgb(248, 250, 252);
    }
</style>