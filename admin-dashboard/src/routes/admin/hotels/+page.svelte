<script lang="ts">
	import { onMount } from 'svelte';
	import { hotels, isLoading, loadHotels } from '$lib/stores/hotels';
	import { Building, Search, MapPin, Phone, Eye, Edit } from 'lucide-svelte';

	let searchQuery = '';

	onMount(() => {
		loadHotels();
	});

	$: filteredHotels = $hotels.filter(hotel => {
		const matchesSearch = !searchQuery || 
			hotel.name?.toLowerCase().includes(searchQuery.toLowerCase()) ||
			hotel.address?.toLowerCase().includes(searchQuery.toLowerCase());
		
		return matchesSearch;
	});
</script>

<svelte:head>
	<title>Hoteller - Receptionist Dashboard</title>
</svelte:head>

<div class="space-y-6">
	<!-- Header -->
	<div class="md:flex md:items-center md:justify-between">
		<div class="flex-1 min-w-0">
			<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Hoteller
			</h1>
			<p class="mt-1 text-sm text-gray-500">
				Administrer alle hoteller
			</p>
		</div>
	</div>

	<!-- Search -->
	<div class="bg-white shadow rounded-lg p-6">
		<div class="max-w-md">
			<label for="search" class="block text-sm font-medium text-gray-700">Søg</label>
			<div class="mt-1 relative rounded-md shadow-sm">
				<div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
					<Search class="h-5 w-5 text-gray-400" />
				</div>
				<input
					type="text"
					name="search"
					id="search"
					class="focus:ring-indigo-500 focus:border-indigo-500 block w-full pl-10 sm:text-sm border-gray-300 rounded-md"
					placeholder="Søg efter hotel navn eller adresse..."
					bind:value={searchQuery}
				/>
			</div>
		</div>
	</div>

	<!-- Hotels Grid -->
	{#if $isLoading}
		<div class="text-center py-12">
			<div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
			<p class="mt-4 text-gray-600">Indlæser hoteller...</p>
		</div>
	{:else if filteredHotels.length === 0}
		<div class="text-center py-12">
			<Building class="mx-auto h-12 w-12 text-gray-400" />
			<h3 class="mt-2 text-sm font-medium text-gray-900">Ingen hoteller</h3>
			<p class="mt-1 text-sm text-gray-500">
				{searchQuery ? 'Ingen hoteller matcher din søgning.' : 'Der er ingen hoteller registreret endnu.'}
			</p>
		</div>
	{:else}
		<div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
			{#each filteredHotels as hotel}
				<div class="bg-white overflow-hidden shadow rounded-lg">
					<div class="p-5">
						<div class="flex items-center">
							<div class="flex-shrink-0">
								<div class="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center">
									<Building class="h-5 w-5 text-indigo-600" />
								</div>
							</div>
							<div class="ml-5 w-0 flex-1">
								<dl>
									<dt class="text-sm font-medium text-gray-500 truncate">
										Hotel
									</dt>
									<dd class="text-lg font-medium text-gray-900">
										{hotel.name}
									</dd>
								</dl>
							</div>
						</div>
					</div>
					<div class="bg-gray-50 px-5 py-3">
						<div class="text-sm space-y-2">
							{#if hotel.address}
								<div class="flex items-center">
									<MapPin class="h-4 w-4 text-gray-400 mr-2" />
									<span class="text-gray-600">{hotel.address}</span>
								</div>
							{/if}
							{#if hotel.phone}
								<div class="flex items-center">
									<Phone class="h-4 w-4 text-gray-400 mr-2" />
									<span class="text-gray-600">{hotel.phone}</span>
								</div>
							{/if}
						</div>
					</div>
					<div class="bg-white px-5 py-3">
						<div class="flex justify-end space-x-2">
							<button
								class="text-indigo-600 hover:text-indigo-900"
								on:click={() => {
									// Navigate to hotel details
								}}
							>
								<Eye class="h-4 w-4" />
							</button>
							<button
								class="text-gray-600 hover:text-gray-900"
								on:click={() => {
									// Navigate to edit hotel
								}}
							>
								<Edit class="h-4 w-4" />
							</button>
						</div>
					</div>
				</div>
			{/each}
		</div>
	{/if}
</div>
