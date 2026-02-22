<script lang="ts">
	import { goto } from '$app/navigation';
	import { login, isLoading } from '$lib/stores/auth';
	import { LogIn, Building } from 'lucide-svelte';

	let username = '';
	let password = '';
	let error = '';

	async function handleLogin() {
		error = '';
		
		if (!username || !password) {
			error = 'Brugernavn og adgangskode er påkrævet';
			return;
		}

		const result = await login(username, password);
		
		if (result.success) {
			goto('/admin/dashboard');
		} else {
			error = result.error || 'Login fejlede';
		}
	}

	function handleKeyPress(event: KeyboardEvent) {
		if (event.key === 'Enter') {
			handleLogin();
		}
	}
</script>

<svelte:head>
	<title>Login - Receptionist Dashboard</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
	<div class="max-w-md w-full space-y-8">
		<div>
			<div class="mx-auto h-12 w-12 flex items-center justify-center rounded-full bg-indigo-100">
				<Building class="h-6 w-6 text-indigo-600" />
			</div>
			<h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
				Receptionist Login
			</h2>
			<p class="mt-2 text-center text-sm text-gray-600">
				Log ind med dine Active Directory credentials
			</p>
		</div>
		
		<form class="mt-8 space-y-6" on:submit|preventDefault={handleLogin}>
			<div class="rounded-md shadow-sm -space-y-px">
				<div>
					<label for="username" class="sr-only">Brugernavn</label>
					<input
						id="username"
						name="username"
						type="text"
						required
						class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
						placeholder="Brugernavn (sAMAccountName, email eller UPN)"
						bind:value={username}
						on:keypress={handleKeyPress}
						disabled={$isLoading}
					/>
				</div>
				<div>
					<label for="password" class="sr-only">Adgangskode</label>
					<input
						id="password"
						name="password"
						type="password"
						required
						class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
						placeholder="Adgangskode"
						bind:value={password}
						on:keypress={handleKeyPress}
						disabled={$isLoading}
					/>
				</div>
			</div>

			{#if error}
				<div class="rounded-md bg-red-50 p-4">
					<div class="flex">
						<div class="flex-shrink-0">
							<svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
								<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z" clip-rule="evenodd" />
							</svg>
						</div>
						<div class="ml-3">
							<h3 class="text-sm font-medium text-red-800">
								Login fejlede
							</h3>
							<div class="mt-2 text-sm text-red-700">
								<p>{error}</p>
							</div>
						</div>
					</div>
				</div>
			{/if}

			<div>
				<button
					type="submit"
					disabled={$isLoading}
					class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
				>
					{#if $isLoading}
						<svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
							<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
							<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
						</svg>
						Logger ind...
					{:else}
						<LogIn class="h-5 w-5 mr-2" />
						Log ind
					{/if}
				</button>
			</div>

			<div class="text-center">
				<p class="text-xs text-gray-500">
					Bruger Active Directory til autentificering
				</p>
			</div>
		</form>
	</div>
</div>
