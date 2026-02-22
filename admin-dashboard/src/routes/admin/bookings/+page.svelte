<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { bookings, isLoading, loadBookings } from '$lib/stores/bookings';
	import { formatDate, getRelativeDate } from '$lib/utils/date';
	import { Calendar, Plus, Search, Filter, Eye, Edit, Trash2 } from 'lucide-svelte';
	import DailyCheckIns from '$lib/components/DailyCheckIns.svelte';

	let searchQuery = '';
	let selectedStatus = 'all';

	onMount(() => {
		loadBookings();
	});

	$: filteredBookings = $bookings.filter(booking => {
		const matchesSearch = !searchQuery || 
			booking.user?.username?.toLowerCase().includes(searchQuery.toLowerCase()) ||
			booking.room?.number?.toLowerCase().includes(searchQuery.toLowerCase());
		
		return matchesSearch;
	});
</script>

<svelte:head>
	<title>Bookings - Receptionist Dashboard</title>
</svelte:head>

<div class="space-y-6">
	<!-- Header -->
	<div class="md:flex md:items-center md:justify-between">
		<div class="flex-1 min-w-0">
			<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Bookings
			</h1>
			<p class="mt-1 text-sm text-gray-500">
				Administrer alle bookinger
			</p>
		</div>
		<div class="mt-4 flex md:mt-0 md:ml-4">
			<button
				type="button"
				class="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
				on:click={() => goto('/admin/bookings/create')}
			>
				<Plus class="h-4 w-4 mr-2" />
				Ny Booking
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
						placeholder="Søg efter bruger eller værelse..."
						bind:value={searchQuery}
					/>
				</div>
			</div>
			
			<div>
				<label for="status" class="block text-sm font-medium text-gray-700">Status</label>
				<select
					id="status"
					name="status"
					class="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
					bind:value={selectedStatus}
				>
					<option value="all">Alle</option>
					<option value="active">Aktive</option>
					<option value="completed">Afsluttede</option>
					<option value="cancelled">Annullerede</option>
				</select>
			</div>
		</div>
	</div>

	<!-- Daily Check-ins & Check-outs -->
	<DailyCheckIns />

	<!-- Bookings Table -->
	<div class="bg-white shadow overflow-hidden sm:rounded-md">
		{#if $isLoading}
			<div class="text-center py-12">
				<div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
				<p class="mt-4 text-gray-600">Indlæser bookinger...</p>
			</div>
		{:else if filteredBookings.length === 0}
			<div class="text-center py-12">
				<Calendar class="mx-auto h-12 w-12 text-gray-400" />
				<h3 class="mt-2 text-sm font-medium text-gray-900">Ingen bookinger</h3>
				<p class="mt-1 text-sm text-gray-500">
					{searchQuery ? 'Ingen bookinger matcher din søgning.' : 'Der er ingen bookinger endnu.'}
				</p>
				{#if !searchQuery}
					<div class="mt-6">
						<button
							type="button"
							class="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700"
							on:click={() => goto('/admin/bookings/create')}
						>
							<Plus class="h-4 w-4 mr-2" />
							Opret første booking
						</button>
					</div>
				{/if}
			</div>
		{:else}
			<ul class="divide-y divide-gray-200">
				{#each filteredBookings as booking}
					<li>
						<div class="px-4 py-4 flex items-center justify-between hover:bg-gray-50">
							<div class="flex items-center">
								<div class="flex-shrink-0 h-10 w-10">
									<div class="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center">
										<Calendar class="h-5 w-5 text-indigo-600" />
									</div>
								</div>
								<div class="ml-4">
									<div class="flex items-center">
										<p class="text-sm font-medium text-indigo-600 truncate">
											{booking.user?.username || 'Ukendt bruger'}
										</p>
										<span class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
											Aktiv
										</span>
									</div>
									<div class="mt-1 flex items-center text-sm text-gray-500">
										<p>Værelse {booking.room?.number || 'N/A'}</p>
										<span class="mx-2">•</span>
										<p>{booking.room?.hotel?.name || 'Ukendt hotel'}</p>
									</div>
								</div>
							</div>
							<div class="flex items-center space-x-4">
								<div class="text-right">
									<p class="text-sm font-medium text-gray-900">
										{formatDate(booking.startDate)} - {formatDate(booking.endDate)}
									</p>
									<p class="text-sm text-gray-500">
										{getRelativeDate(booking.startDate)}
									</p>
								</div>
								<div class="flex space-x-2">
									<button
										class="text-indigo-600 hover:text-indigo-900"
										on:click={() => goto(`/admin/bookings/${booking.id}`)}
									>
										<Eye class="h-5 w-5" />
									</button>
									<button
										class="text-gray-600 hover:text-gray-900"
										on:click={() => goto(`/admin/bookings/${booking.id}/edit`)}
									>
										<Edit class="h-5 w-5" />
									</button>
									<button
										class="text-red-600 hover:text-red-900"
										on:click={() => {
											if (confirm('Er du sikker på at du vil slette denne booking?')) {
												// Handle delete
											}
										}}
									>
										<Trash2 class="h-5 w-5" />
									</button>
								</div>
							</div>
						</div>
					</li>
				{/each}
			</ul>
		{/if}
	</div>
</div>
