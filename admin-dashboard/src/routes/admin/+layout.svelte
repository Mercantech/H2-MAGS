<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { isAuthenticated, user } from '$lib/stores/auth';
	import { page } from '$app/stores';
	import { browser } from '$app/environment';
	import Sidebar from '$lib/components/Sidebar.svelte';
	import Header from '$lib/components/Header.svelte';

	onMount(() => {
		if (!$isAuthenticated && $page.url.pathname !== '/admin/login') {
			goto('/admin/login');
		}
	});
</script>

{#if $isAuthenticated}
	<div class="min-h-screen bg-gray-50">
		<Sidebar />
		<div class="lg:pl-64">
			<Header />
			<main class="py-6">
				<div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
					<slot />
				</div>
			</main>
		</div>
	</div>
{:else}
	<slot />
{/if}
