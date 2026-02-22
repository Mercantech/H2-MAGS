<script lang="ts">
	import { user } from '$lib/stores/auth';
	import { Bell, Search } from 'lucide-svelte';

	let searchQuery = '';
	let showNotifications = false;
</script>

<header class="bg-white shadow-sm border-b border-gray-200">
	<div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
		<div class="flex h-16 justify-between items-center">
			<!-- Search -->
			<div class="flex flex-1 items-center">
				<div class="w-full max-w-lg lg:max-w-xs">
					<label for="search" class="sr-only">Søg</label>
					<div class="relative">
						<div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
							<Search class="h-5 w-5 text-gray-400" />
						</div>
						<input
							id="search"
							name="search"
							class="block w-full rounded-md border-0 bg-white py-1.5 pl-10 pr-3 text-gray-900 ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
							placeholder="Søg bookinger, værelser..."
							type="search"
							bind:value={searchQuery}
						/>
					</div>
				</div>
			</div>

			<!-- Right side -->
			<div class="flex items-center gap-x-4 lg:gap-x-6">
				<!-- Notifications -->
				<button
					type="button"
					class="-m-2.5 p-2.5 text-gray-400 hover:text-gray-500"
					on:click={() => showNotifications = !showNotifications}
				>
					<span class="sr-only">Vis notifikationer</span>
					<Bell class="h-6 w-6" />
				</button>

		<!-- Separator -->
		<div class="hidden lg:block lg:h-6 lg:w-px lg:bg-gray-200"></div>

				<!-- Profile dropdown -->
				<div class="relative">
					<div class="flex items-center gap-x-4">
						<div class="hidden lg:block">
							<p class="text-sm font-semibold leading-6 text-gray-900">
								{$user?.displayName || $user?.username || 'Bruger'}
							</p>
							<p class="text-xs leading-5 text-gray-500">
								{$user?.role || 'Bruger'}
								{#if $user?.isADUser}
									<span class="ml-1 text-indigo-600">(AD)</span>
								{/if}
							</p>
						</div>
						<div class="h-8 w-8 rounded-full bg-indigo-100 flex items-center justify-center">
							<span class="text-sm font-medium text-indigo-600">
								{$user?.displayName?.charAt(0) || $user?.username?.charAt(0) || 'U'}
							</span>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>

	<!-- Notifications dropdown -->
	{#if showNotifications}
		<div class="absolute right-0 z-10 mt-2 w-80 origin-top-right rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none">
			<div class="px-4 py-2 text-sm text-gray-700">
				<p class="font-medium">Notifikationer</p>
			</div>
			<div class="px-4 py-2 text-sm text-gray-500">
				<p>Ingen nye notifikationer</p>
			</div>
		</div>
	{/if}
</header>
