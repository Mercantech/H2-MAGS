<!-- src/routes/Guests.svelte -->
<script>
    import { fade } from 'svelte/transition';
    import { onMount } from 'svelte';
    import { apiService } from '../services/api.service';

    let guests = [];
    let isLoading = true;
    let error = null;
    let searchTerm = '';

    onMount(async () => {
        try {
            guests = await apiService.getGuests();
        } catch (err) {
            error = err.message;
        } finally {
            isLoading = false;
        }
    });

    $: filteredGuests = guests.filter(guest => 
        guest.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        guest.email.toLowerCase().includes(searchTerm.toLowerCase())
    );

    function formatDate(date) {
        return new Date(date).toLocaleString('da-DK', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
</script>

<div class="p-8" in:fade>
    <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold">Gæster</h1>
        <div class="flex gap-4">
            <input
                type="text"
                bind:value={searchTerm}
                placeholder="Søg efter navn eller email..."
                class="px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button class="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600">
                + Tilføj Gæst
            </button>
        </div>
    </div>

    {#if isLoading}
        <div class="flex justify-center items-center h-64">
            <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
        </div>
    {:else if error}
        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
            {error}
        </div>
    {:else}
        <div class="bg-white rounded-lg shadow-sm overflow-hidden">
            <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                    <tr>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Navn
                        </th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Email
                        </th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Status
                        </th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Antal Bookinger
                        </th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Sidst Logget Ind
                        </th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Handlinger
                        </th>
                    </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                    {#each filteredGuests as guest}
                        <tr class="hover:bg-gray-50">
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm font-medium text-gray-900">{guest.name}</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm text-gray-500">{guest.email}</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full 
                                    {guest.isEmailConfirmed ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'}">
                                    {guest.isEmailConfirmed ? 'Verificeret' : 'Ikke Verificeret'}
                                </span>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {guest.totalBookings}
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {formatDate(guest.lastLogin)}
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                <button class="text-blue-600 hover:text-blue-900 mr-3">Se Bookinger</button>
                                <button class="text-red-600 hover:text-red-900">Slet</button>
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>