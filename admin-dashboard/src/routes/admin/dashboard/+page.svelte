<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { isAuthenticated } from '$lib/stores/auth';
	import { browser } from '$app/environment';
	import { adminService } from '$lib/services/api';
	import StatsCard from '$lib/components/StatsCard.svelte';
	import RecentBookings from '$lib/components/RecentBookings.svelte';
	import RoomAvailability from '$lib/components/RoomAvailability.svelte';
	import { 
		Calendar, 
		Bed, 
		Users, 
		DollarSign,
		Plus,
		TrendingUp,
		BookOpen,
		Clock
	} from 'lucide-svelte';

	let stats = {
		totalBookings: 0,
		activeBookings: 0,
		availableRooms: 0,
		totalRevenue: 0
	};
	let isLoading = true;

	onMount(async () => {
		if (browser && !$isAuthenticated) {
			goto('/admin/login');
			return;
		}

		await loadDashboardData();
	});

	async function loadDashboardData() {
		try {
			isLoading = true;
			const response = await adminService.getDashboardStats();
			stats = response;
		} catch (error) {
			console.error('Fejl ved indlæsning af dashboard data:', error);
		} finally {
			isLoading = false;
		}
	}
</script>

<svelte:head>
	<title>Dashboard - Receptionist Dashboard</title>
</svelte:head>

<div class="space-y-6">
	<!-- Header -->
	<div class="md:flex md:items-center md:justify-between">
		<div class="flex-1 min-w-0">
			<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Dashboard
			</h1>
			<p class="mt-1 text-sm text-gray-500">
				Velkommen til receptionist dashboardet
			</p>
		</div>
		<div class="mt-4 flex flex-wrap gap-3 md:mt-0 md:ml-4">
			<button
				type="button"
				class="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
				on:click={() => goto('/admin/bookings')}
			>
				<Clock class="h-4 w-4 mr-2" />
				Dagens Check-ins
			</button>
			<button
				type="button"
				class="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
				on:click={() => goto('/admin/about')}
			>
				<BookOpen class="h-4 w-4 mr-2" />
				Om Svelte & API
			</button>
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

	<!-- Stats Cards -->
	{#if isLoading}
		<div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
			{#each Array(4) as _}
				<div class="bg-white shadow rounded-lg p-6 animate-pulse">
					<div class="flex items-center">
						<div class="flex-shrink-0">
							<div class="h-10 w-10 bg-gray-200 rounded-md"></div>
						</div>
						<div class="ml-5 w-0 flex-1">
							<div class="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
							<div class="h-6 bg-gray-200 rounded w-1/2"></div>
						</div>
					</div>
				</div>
			{/each}
		</div>
	{:else}
		<div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
			<StatsCard 
				title="Total Bookings" 
				value={stats.totalBookings} 
				icon={Calendar}
				color="indigo"
			/>
			<StatsCard 
				title="Aktive Bookings" 
				value={stats.activeBookings} 
				icon={TrendingUp}
				color="green"
			/>
			<StatsCard 
				title="Ledige Værelser" 
				value={stats.availableRooms} 
				icon={Bed}
				color="blue"
			/>
			<StatsCard 
				title="Total Omsætning" 
				value={`${stats.totalRevenue} kr`} 
				icon={DollarSign}
				color="purple"
			/>
		</div>
	{/if}

	<!-- Main Content Grid -->
	<div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
		<RecentBookings />
		<RoomAvailability />
	</div>

	<!-- Quick Actions -->
	<div class="bg-white shadow rounded-lg">
		<div class="px-4 py-5 sm:p-6">
			<h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
				Hurtige Handlinger
			</h3>
			<div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
				<button
					class="relative group bg-white p-6 focus-within:ring-2 focus-within:ring-inset focus-within:ring-indigo-500 rounded-lg border border-gray-300 hover:border-gray-400"
					on:click={() => goto('/admin/bookings/create')}
				>
					<div>
						<span class="rounded-lg inline-flex p-3 bg-indigo-50 text-indigo-700 ring-4 ring-white">
							<Calendar class="h-6 w-6" />
						</span>
					</div>
					<div class="mt-8">
						<h3 class="text-lg font-medium">
							<span class="absolute inset-0" aria-hidden="true"></span>
							Ny Booking
						</h3>
						<p class="mt-2 text-sm text-gray-500">
							Opret en ny booking for en gæst
						</p>
					</div>
				</button>

				<button
					class="relative group bg-white p-6 focus-within:ring-2 focus-within:ring-inset focus-within:ring-indigo-500 rounded-lg border border-gray-300 hover:border-gray-400"
					on:click={() => goto('/admin/bookings')}
				>
					<div>
						<span class="rounded-lg inline-flex p-3 bg-green-50 text-green-700 ring-4 ring-white">
							<TrendingUp class="h-6 w-6" />
						</span>
					</div>
					<div class="mt-8">
						<h3 class="text-lg font-medium">
							<span class="absolute inset-0" aria-hidden="true"></span>
							Se Bookings
						</h3>
						<p class="mt-2 text-sm text-gray-500">
							Administrer eksisterende bookinger
						</p>
					</div>
				</button>

				<button
					class="relative group bg-white p-6 focus-within:ring-2 focus-within:ring-inset focus-within:ring-indigo-500 rounded-lg border border-gray-300 hover:border-gray-400"
					on:click={() => goto('/admin/rooms')}
				>
					<div>
						<span class="rounded-lg inline-flex p-3 bg-blue-50 text-blue-700 ring-4 ring-white">
							<Bed class="h-6 w-6" />
						</span>
					</div>
					<div class="mt-8">
						<h3 class="text-lg font-medium">
							<span class="absolute inset-0" aria-hidden="true"></span>
							Værelser
						</h3>
						<p class="mt-2 text-sm text-gray-500">
							Se og administrer værelser
						</p>
					</div>
				</button>

				<button
					class="relative group bg-white p-6 focus-within:ring-2 focus-within:ring-inset focus-within:ring-indigo-500 rounded-lg border border-gray-300 hover:border-gray-400"
					on:click={() => goto('/admin/users')}
				>
					<div>
						<span class="rounded-lg inline-flex p-3 bg-purple-50 text-purple-700 ring-4 ring-white">
							<Users class="h-6 w-6" />
						</span>
					</div>
					<div class="mt-8">
						<h3 class="text-lg font-medium">
							<span class="absolute inset-0" aria-hidden="true"></span>
							Brugere
						</h3>
						<p class="mt-2 text-sm text-gray-500">
							Administrer brugere og roller
						</p>
					</div>
				</button>
			</div>
		</div>
	</div>
</div>
