<script lang="ts">
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { isAuthenticated } from '$lib/stores/auth';
	import { browser } from '$app/environment';

	onMount(() => {
		if (browser) {
			if ($isAuthenticated) {
				goto('/admin/dashboard');
			} else {
				goto('/admin/login');
			}
		}
	});
</script>

<div class="min-h-screen flex items-center justify-center">
	<div class="text-center">
		<div class="animate-spin rounded-full h-32 w-32 border-b-2 border-indigo-600 mx-auto"></div>
		<p class="mt-4 text-gray-600">Omdirigerer...</p>
	</div>
</div>
