<script lang="ts">
	import { onMount } from 'svelte';
	import { rooms, loadRooms } from '$lib/stores/rooms';
	import { Bed, Users, CheckCircle, XCircle } from 'lucide-svelte';

	onMount(() => {
		loadRooms();
	});

	$: availableRooms = $rooms.filter(room => room.capacity > 0);
	$: totalCapacity = $rooms.reduce((sum, room) => sum + room.capacity, 0);
</script>

<div class="bg-white shadow rounded-lg">
	<div class="px-4 py-5 sm:p-6">
		<h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
			Værelses Tilgængelighed
		</h3>
		
		{#if $rooms.length === 0}
			<div class="text-center py-6">
				<Bed class="mx-auto h-12 w-12 text-gray-400" />
				<h3 class="mt-2 text-sm font-medium text-gray-900">Ingen værelser</h3>
				<p class="mt-1 text-sm text-gray-500">Der er ingen værelser registreret endnu.</p>
			</div>
		{:else}
			<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center">
						<Bed class="h-8 w-8 text-indigo-600" />
						<div class="ml-3">
							<p class="text-sm font-medium text-gray-500">Total Værelser</p>
							<p class="text-2xl font-semibold text-gray-900">{$rooms.length}</p>
						</div>
					</div>
				</div>
				
				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center">
						<Users class="h-8 w-8 text-green-600" />
						<div class="ml-3">
							<p class="text-sm font-medium text-gray-500">Total Kapacitet</p>
							<p class="text-2xl font-semibold text-gray-900">{totalCapacity}</p>
						</div>
					</div>
				</div>
			</div>

			<div class="mt-6">
				<h4 class="text-sm font-medium text-gray-900 mb-3">Værelser</h4>
				<div class="space-y-2">
					{#each $rooms.slice(0, 5) as room}
						<div class="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
							<div class="flex items-center">
								<Bed class="h-5 w-5 text-gray-400 mr-3" />
								<div>
									<p class="text-sm font-medium text-gray-900">
										Værelse {room.number}
									</p>
									<p class="text-xs text-gray-500">
										{room.hotel?.name || 'Ukendt hotel'}
									</p>
								</div>
							</div>
							<div class="flex items-center">
								<Users class="h-4 w-4 text-gray-400 mr-1" />
								<span class="text-sm text-gray-600">{room.capacity}</span>
								<CheckCircle class="h-4 w-4 text-green-500 ml-2" />
							</div>
						</div>
					{/each}
				</div>
			</div>
			
			<div class="mt-6">
				<a
					href="/admin/rooms"
					class="w-full flex justify-center items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
				>
					Se alle værelser
				</a>
			</div>
		{/if}
	</div>
</div>
