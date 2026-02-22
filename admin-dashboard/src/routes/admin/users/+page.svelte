<script lang="ts">
	import { onMount } from 'svelte';
	import { users, isLoading, loadUsers } from '$lib/stores/users';
	import { formatDate } from '$lib/utils/date';
	import { User, Search, Shield, Mail, Calendar, Eye, Edit } from 'lucide-svelte';

	let searchQuery = '';
	let selectedRole = 'all';

	onMount(() => {
		loadUsers();
	});

	$: filteredUsers = $users.filter(user => {
		const matchesSearch = !searchQuery || 
			user.username?.toLowerCase().includes(searchQuery.toLowerCase()) ||
			user.email?.toLowerCase().includes(searchQuery.toLowerCase()) ||
			user.displayName?.toLowerCase().includes(searchQuery.toLowerCase());
		
		const matchesRole = selectedRole === 'all' || user.role === selectedRole;
		
		return matchesSearch && matchesRole;
	});

	$: uniqueRoles = [...new Set($users.map(user => user.role))];

	function getRoleColor(role: string) {
		switch (role.toLowerCase()) {
			case 'admin':
				return 'bg-red-100 text-red-800';
			case 'manager':
				return 'bg-yellow-100 text-yellow-800';
			case 'receptionist':
				return 'bg-blue-100 text-blue-800';
			case 'user':
				return 'bg-green-100 text-green-800';
			default:
				return 'bg-gray-100 text-gray-800';
		}
	}
</script>

<svelte:head>
	<title>Brugere - Receptionist Dashboard</title>
</svelte:head>

<div class="space-y-6">
	<!-- Header -->
	<div class="md:flex md:items-center md:justify-between">
		<div class="flex-1 min-w-0">
			<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Brugere
			</h1>
			<p class="mt-1 text-sm text-gray-500">
				Administrer alle brugere og roller
			</p>
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
						placeholder="Søg efter navn eller email..."
						bind:value={searchQuery}
					/>
				</div>
			</div>
			
			<div>
				<label for="role" class="block text-sm font-medium text-gray-700">Rolle</label>
				<select
					id="role"
					name="role"
					class="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
					bind:value={selectedRole}
				>
					<option value="all">Alle roller</option>
					{#each uniqueRoles as role}
						<option value={role}>{role}</option>
					{/each}
				</select>
			</div>
		</div>
	</div>

	<!-- Users Table -->
	<div class="bg-white shadow overflow-hidden sm:rounded-md">
		{#if $isLoading}
			<div class="text-center py-12">
				<div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
				<p class="mt-4 text-gray-600">Indlæser brugere...</p>
			</div>
		{:else if filteredUsers.length === 0}
			<div class="text-center py-12">
				<User class="mx-auto h-12 w-12 text-gray-400" />
				<h3 class="mt-2 text-sm font-medium text-gray-900">Ingen brugere</h3>
				<p class="mt-1 text-sm text-gray-500">
					{searchQuery ? 'Ingen brugere matcher din søgning.' : 'Der er ingen brugere registreret endnu.'}
				</p>
			</div>
		{:else}
			<ul class="divide-y divide-gray-200">
				{#each filteredUsers as user}
					<li>
						<div class="px-4 py-4 flex items-center justify-between hover:bg-gray-50">
							<div class="flex items-center">
								<div class="flex-shrink-0 h-10 w-10">
									<div class="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center">
										<User class="h-5 w-5 text-indigo-600" />
									</div>
								</div>
								<div class="ml-4">
									<div class="flex items-center">
										<p class="text-sm font-medium text-indigo-600 truncate">
											{user.displayName || user.username}
										</p>
										{#if user.isADUser}
											<span class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
												AD
											</span>
										{/if}
										<span class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {getRoleColor(user.role)}">
											{user.role}
										</span>
									</div>
									<div class="mt-1 flex items-center text-sm text-gray-500">
										<Mail class="h-4 w-4 mr-1" />
										<p>{user.email}</p>
									</div>
									{#if user.adGroups && user.adGroups.length > 0}
										<div class="mt-1 text-xs text-gray-500">
											AD Grupper: {user.adGroups.join(', ')}
										</div>
									{/if}
								</div>
							</div>
							<div class="flex items-center space-x-4">
								<div class="text-right">
									<p class="text-sm text-gray-500">
										{#if user.lastLogin}
											Seneste login: {formatDate(user.lastLogin)}
										{:else}
											Aldrig logget ind
										{/if}
									</p>
								</div>
								<div class="flex space-x-2">
									<button
										class="text-indigo-600 hover:text-indigo-900"
										on:click={() => {
											// Navigate to user details
										}}
									>
										<Eye class="h-5 w-5" />
									</button>
									<button
										class="text-gray-600 hover:text-gray-900"
										on:click={() => {
											// Navigate to edit user
										}}
									>
										<Edit class="h-5 w-5" />
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
