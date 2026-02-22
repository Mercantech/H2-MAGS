<script lang="ts">
	import { onMount } from 'svelte';
	import { rooms, isLoading, loadRooms } from '$lib/stores/rooms';
	import { formatDate } from '$lib/utils/date';
	import { Bed, Search, Building, Users, Plus, Eye, Edit } from 'lucide-svelte';

	let searchQuery = '';
	let selectedHotel = 'all';

	onMount(() => {
		loadRooms();
	});

	$: filteredRooms = $rooms.filter(room => {
		const matchesSearch = !searchQuery || 
			room.number?.toLowerCase().includes(searchQuery.toLowerCase()) ||
			room.hotel?.name?.toLowerCase().includes(searchQuery.toLowerCase());
		
		const matchesHotel = selectedHotel === 'all' || room.hotelId === selectedHotel;
		
		return matchesSearch && matchesHotel;
	});

	$: uniqueHotels = [...new Set($rooms.map(room => ({ id: room.hotelId, name: room.hotel?.name })))];
</script>

<svelte:head>
	<title>Værelser - Receptionist Dashboard</title>
</svelte:head>

<div class="space-y-6">
	<!-- Header -->
	<div class="md:flex md:items-center md:justify-between">
		<div class="flex-1 min-w-0">
			<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Værelser
			</h1>
			<p class="mt-1 text-sm text-gray-500">
				Administrer alle værelser
			</p>
		</div>
		<div class="mt-4 flex md:mt-0 md:ml-4">
			<button
				type="button"
				class="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
			>
				<Plus class="h-4 w-4 mr-2" />
				Tilføj Værelse
			</button>
		</div>
	</div>

	<!-- Search and Filters -->
	<div class="bg-white shadow rounded-lg p-6">
		<div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
			<div>
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
						placeholder="Søg efter værelsesnummer eller hotel..."
						bind:value={searchQuery}
					/>
				</div>
			</div>
			
			<div>
				<label for="hotel" class="block text-sm font-medium text-gray-700">Hotel</label>
				<select
					id="hotel"
					name="hotel"
					class="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
					bind:value={selectedHotel}
				>
					<option value="all">Alle hoteller</option>
					{#each uniqueHotels as hotel}
						<option value={hotel.id}>{hotel.name}</option>
					{/each}
				</select>
			</div>
		</div>
	</div>

	<!-- Rooms Grid -->
	{#if $isLoading}
		<div class="text-center py-12">
			<div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
			<p class="mt-4 text-gray-600">Indlæser værelser...</p>
		</div>
	{:else if filteredRooms.length === 0}
		<div class="text-center py-12">
			<Bed class="mx-auto h-12 w-12 text-gray-400" />
			<h3 class="mt-2 text-sm font-medium text-gray-900">Ingen værelser</h3>
			<p class="mt-1 text-sm text-gray-500">
				{searchQuery ? 'Ingen værelser matcher din søgning.' : 'Der er ingen værelser registreret endnu.'}
			</p>
		</div>
	{:else}
		<div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
			{#each filteredRooms as room}
				<div class="bg-white overflow-hidden shadow rounded-lg">
					<div class="p-5">
						<div class="flex items-center">
							<div class="flex-shrink-0">
								<div class="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center">
									<Bed class="h-5 w-5 text-indigo-600" />
								</div>
							</div>
							<div class="ml-5 w-0 flex-1">
								<dl>
									<dt class="text-sm font-medium text-gray-500 truncate">
										Værelse
									</dt>
									<dd class="text-lg font-medium text-gray-900">
										{room.number}
									</dd>
								</dl>
							</div>
						</div>
					</div>
					<div class="bg-gray-50 px-5 py-3">
						<div class="text-sm">
							<div class="flex items-center justify-between">
								<div class="flex items-center">
									<Building class="h-4 w-4 text-gray-400 mr-2" />
									<span class="text-gray-600">{room.hotel?.name || 'Ukendt hotel'}</span>
								</div>
								<div class="flex items-center">
									<Users class="h-4 w-4 text-gray-400 mr-1" />
									<span class="text-gray-600">{room.capacity}</span>
								</div>
							</div>
						</div>
					</div>
					<div class="bg-white px-5 py-3">
						<div class="flex justify-end space-x-2">
							<button
								class="text-indigo-600 hover:text-indigo-900"
								on:click={() => {
									// Navigate to room details
								}}
							>
								<Eye class="h-4 w-4" />
							</button>
							<button
								class="text-gray-600 hover:text-gray-900"
								on:click={() => {
									// Navigate to edit room
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
