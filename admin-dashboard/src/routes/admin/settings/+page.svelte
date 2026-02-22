<script lang="ts">
	import { onMount } from 'svelte';
	import { user } from '$lib/stores/auth';
	import { authService } from '$lib/services/api';
	import { Settings, User, Shield, Database, Bell } from 'lucide-svelte';

	let adStatus = null;
	let isLoading = false;

	onMount(async () => {
		await loadADStatus();
	});

	async function loadADStatus() {
		isLoading = true;
		try {
			const response = await authService.getADStatus();
			adStatus = response;
		} catch (error) {
			console.error('Error loading AD status:', error);
		} finally {
			isLoading = false;
		}
	}
</script>

<svelte:head>
	<title>Indstillinger - Receptionist Dashboard</title>
</svelte:head>

<div class="space-y-6">
	<!-- Header -->
	<div class="md:flex md:items-center md:justify-between">
		<div class="flex-1 min-w-0">
			<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Indstillinger
			</h1>
			<p class="mt-1 text-sm text-gray-500">
				Administrer systemindstillinger og konfiguration
			</p>
		</div>
	</div>

	<!-- Settings Sections -->
	<div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
		<!-- User Profile -->
		<div class="bg-white shadow rounded-lg">
			<div class="px-4 py-5 sm:p-6">
				<h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
					Bruger Profil
				</h3>
				<div class="space-y-4">
					<div class="flex items-center">
						<User class="h-5 w-5 text-gray-400 mr-3" />
						<div>
							<p class="text-sm font-medium text-gray-900">
								{$user?.displayName || $user?.username || 'Ukendt bruger'}
							</p>
							<p class="text-sm text-gray-500">{$user?.email}</p>
						</div>
					</div>
					<div class="flex items-center">
						<Shield class="h-5 w-5 text-gray-400 mr-3" />
						<div>
							<p class="text-sm font-medium text-gray-900">Rolle</p>
							<p class="text-sm text-gray-500">{$user?.role || 'Bruger'}</p>
						</div>
					</div>
					{#if $user?.isADUser}
						<div class="flex items-center">
							<Database class="h-5 w-5 text-gray-400 mr-3" />
							<div>
								<p class="text-sm font-medium text-gray-900">Login Metode</p>
								<p class="text-sm text-gray-500">Active Directory</p>
							</div>
						</div>
					{/if}
				</div>
			</div>
		</div>

		<!-- Active Directory Status -->
		<div class="bg-white shadow rounded-lg">
			<div class="px-4 py-5 sm:p-6">
				<h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
					Active Directory Status
				</h3>
				{#if isLoading}
					<div class="text-center py-4">
						<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600 mx-auto"></div>
						<p class="mt-2 text-sm text-gray-600">Indlæser status...</p>
					</div>
				{:else if adStatus}
					<div class="space-y-4">
						<div class="flex items-center justify-between">
							<span class="text-sm font-medium text-gray-900">Status</span>
							<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {adStatus.testConnection ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
								{adStatus.testConnection ? 'Tilsluttet' : 'Ikke tilsluttet'}
							</span>
						</div>
						<div class="flex items-center justify-between">
							<span class="text-sm font-medium text-gray-900">Server</span>
							<span class="text-sm text-gray-500">{adStatus.server}</span>
						</div>
						<div class="flex items-center justify-between">
							<span class="text-sm font-medium text-gray-900">Domain</span>
							<span class="text-sm text-gray-500">{adStatus.domain}</span>
						</div>
						<div class="flex items-center justify-between">
							<span class="text-sm font-medium text-gray-900">Test Bruger</span>
							<span class="text-sm text-gray-500">{adStatus.testUser}</span>
						</div>
					</div>
				{:else}
					<div class="text-center py-4">
						<Database class="mx-auto h-8 w-8 text-gray-400" />
						<p class="mt-2 text-sm text-gray-600">Kunne ikke hente AD status</p>
					</div>
				{/if}
			</div>
		</div>

		<!-- System Information -->
		<div class="bg-white shadow rounded-lg">
			<div class="px-4 py-5 sm:p-6">
				<h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
					System Information
				</h3>
				<div class="space-y-4">
					<div class="flex items-center justify-between">
						<span class="text-sm font-medium text-gray-900">Version</span>
						<span class="text-sm text-gray-500">1.0.0</span>
					</div>
					<div class="flex items-center justify-between">
						<span class="text-sm font-medium text-gray-900">Environment</span>
						<span class="text-sm text-gray-500">Development</span>
					</div>
					<div class="flex items-center justify-between">
						<span class="text-sm font-medium text-gray-900">Last Updated</span>
						<span class="text-sm text-gray-500">{new Date().toLocaleDateString('da-DK')}</span>
					</div>
				</div>
			</div>
		</div>

		<!-- Notifications -->
		<div class="bg-white shadow rounded-lg">
			<div class="px-4 py-5 sm:p-6">
				<h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
					Notifikationer
				</h3>
				<div class="space-y-4">
					<div class="flex items-center">
						<Bell class="h-5 w-5 text-gray-400 mr-3" />
						<div>
							<p class="text-sm font-medium text-gray-900">Email Notifikationer</p>
							<p class="text-sm text-gray-500">Modtag notifikationer via email</p>
						</div>
						<div class="ml-auto">
							<input type="checkbox" class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded" checked />
						</div>
					</div>
					<div class="flex items-center">
						<Bell class="h-5 w-5 text-gray-400 mr-3" />
						<div>
							<p class="text-sm font-medium text-gray-900">Push Notifikationer</p>
							<p class="text-sm text-gray-500">Modtag push notifikationer</p>
						</div>
						<div class="ml-auto">
							<input type="checkbox" class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded" />
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</div>
