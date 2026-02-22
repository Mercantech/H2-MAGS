<script lang="ts">
	import { onMount } from 'svelte';
	import { bookings, loadBookings } from '$lib/stores/bookings';
	import { formatDate, getRelativeDate } from '$lib/utils/date';
	import { Calendar, User, Bed, Clock } from 'lucide-svelte';

	onMount(() => {
		loadBookings();
	});

	$: recentBookings = $bookings.slice(0, 5);
</script>

<div class="bg-white shadow rounded-lg">
	<div class="px-4 py-5 sm:p-6">
		<h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
			Seneste Bookings
		</h3>
		
		{#if $bookings.length === 0}
			<div class="text-center py-6">
				<Calendar class="mx-auto h-12 w-12 text-gray-400" />
				<h3 class="mt-2 text-sm font-medium text-gray-900">Ingen bookinger</h3>
				<p class="mt-1 text-sm text-gray-500">Der er ingen bookinger endnu.</p>
			</div>
		{:else}
			<div class="flow-root">
				<ul role="list" class="-my-5 divide-y divide-gray-200">
					{#each recentBookings as booking}
						<li class="py-4">
							<div class="flex items-center space-x-4">
								<div class="flex-shrink-0">
									<div class="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center">
										<Calendar class="h-5 w-5 text-indigo-600" />
									</div>
								</div>
								<div class="flex-1 min-w-0">
									<div class="flex items-center justify-between">
										<div>
											<p class="text-sm font-medium text-gray-900 truncate">
												{booking.user?.username || 'Ukendt bruger'}
											</p>
											<p class="text-sm text-gray-500">
												Værelse {booking.room?.number || 'N/A'}
											</p>
										</div>
										<div class="text-right">
											<p class="text-sm text-gray-900">
												{formatDate(booking.startDate)} - {formatDate(booking.endDate)}
											</p>
											<p class="text-xs text-gray-500">
												{getRelativeDate(booking.startDate)}
											</p>
										</div>
									</div>
								</div>
							</div>
						</li>
					{/each}
				</ul>
			</div>
			
			<div class="mt-6">
				<a
					href="/admin/bookings"
					class="w-full flex justify-center items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
				>
					Se alle bookinger
				</a>
			</div>
		{/if}
	</div>
</div>
