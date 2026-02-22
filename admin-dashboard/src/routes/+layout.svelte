<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { isAuthenticated, user } from '$lib/stores/auth';
	import { page } from '$app/stores';
	import { browser } from '$app/environment';
	import '../app.css';

	onMount(() => {
		// Redirect to admin dashboard if authenticated and on root
		if (browser && $isAuthenticated && $page.url.pathname === '/') {
			goto('/admin/dashboard');
		}
	});
</script>

<main class="min-h-screen bg-gray-50">
	<slot />
</main>