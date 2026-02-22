<script lang="ts">
	import { page } from '$app/stores';
	import { user } from '$lib/stores/auth';
	import { 
		Home, 
		Calendar, 
		Bed, 
		Users, 
		Building, 
		Settings,
		BookOpen,
		LogOut
	} from 'lucide-svelte';
	import { goto } from '$app/navigation';
	import { logout } from '$lib/stores/auth';

	const navigation = [
		{ name: 'Dashboard', href: '/admin/dashboard', icon: Home },
		{ name: 'Bookings', href: '/admin/bookings', icon: Calendar },
		{ name: 'Værelser', href: '/admin/rooms', icon: Bed },
		{ name: 'Brugere', href: '/admin/users', icon: Users },
		{ name: 'Hoteller', href: '/admin/hotels', icon: Building },
		{ name: 'Indstillinger', href: '/admin/settings', icon: Settings },
		{ name: 'Om Svelte & API', href: '/admin/about', icon: BookOpen }
	];

	function handleLogout() {
		logout();
		goto('/admin/login');
	}
</script>

<div class="hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:w-64 lg:flex-col">
	<div class="flex grow flex-col gap-y-5 overflow-y-auto bg-white px-6 pb-4 shadow-lg">
		<div class="flex h-16 shrink-0 items-center">
			<h1 class="text-xl font-bold text-indigo-600">Receptionist Dashboard</h1>
		</div>
		<nav class="flex flex-1 flex-col">
			<ul role="list" class="flex flex-1 flex-col gap-y-7">
				<li>
					<ul role="list" class="-mx-2 space-y-1">
						{#each navigation as item}
							<li>
								<a
									href={item.href}
									class="group flex gap-x-3 rounded-md p-2 text-sm leading-6 font-semibold transition-colors
										{$page.url.pathname === item.href
											? 'bg-indigo-50 text-indigo-600'
											: 'text-gray-700 hover:text-indigo-600 hover:bg-gray-50'}"
								>
									<svelte:component this={item.icon} class="h-6 w-6 shrink-0" />
									{item.name}
								</a>
							</li>
						{/each}
					</ul>
				</li>
				<li class="mt-auto">
					<div class="border-t border-gray-200 pt-4">
						<div class="flex items-center gap-x-4 px-2 py-3">
							<div class="h-8 w-8 rounded-full bg-indigo-100 flex items-center justify-center">
								<span class="text-sm font-medium text-indigo-600">
									{$user?.displayName?.charAt(0) || $user?.username?.charAt(0) || 'U'}
								</span>
							</div>
							<div class="flex-1 min-w-0">
								<p class="text-sm font-semibold text-gray-900 truncate">
									{$user?.displayName || $user?.username || 'Bruger'}
								</p>
								<p class="text-xs text-gray-500 truncate">
									{$user?.role || 'Bruger'}
								</p>
							</div>
						</div>
						<button
							on:click={handleLogout}
							class="group flex w-full gap-x-3 rounded-md p-2 text-sm leading-6 font-semibold text-gray-700 hover:text-red-600 hover:bg-red-50 transition-colors"
						>
							<LogOut class="h-6 w-6 shrink-0" />
							Log ud
						</button>
					</div>
				</li>
			</ul>
		</nav>
	</div>
</div>
