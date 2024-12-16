<!-- src/routes/Rooms.svelte -->
<script>
    import { fade } from 'svelte/transition';

    let rooms = [
        { id: 1, number: "101", type: "Standard", status: "Ledig", price: 899, lastCleaned: "2024-01-20" },
        { id: 2, number: "102", type: "Deluxe", status: "Optaget", price: 1299, lastCleaned: "2024-01-19" },
        { id: 3, number: "103", type: "Suite", status: "Under Rengøring", price: 1899, lastCleaned: "2024-01-20" },
        { id: 4, number: "201", type: "Standard", status: "Ledig", price: 899, lastCleaned: "2024-01-20" },
        { id: 5, number: "202", type: "Deluxe", status: "Vedligeholdelse", price: 1299, lastCleaned: "2024-01-18" },
    ];

    function getStatusColor(status) {
        switch (status) {
            case "Ledig": return "bg-green-100 text-green-800";
            case "Optaget": return "bg-red-100 text-red-800";
            case "Under Rengøring": return "bg-yellow-100 text-yellow-800";
            case "Vedligeholdelse": return "bg-gray-100 text-gray-800";
            default: return "bg-gray-100 text-gray-800";
        }
    }
</script>

<div class="p-8" in:fade>
    <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold">Værelser</h1>
        <button class="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600">
            + Tilføj Værelse
        </button>
    </div>

    <div class="bg-white rounded-lg shadow-sm overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
                <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Nummer
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Type
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Status
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Pris/nat
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Sidst Rengjort
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Handlinger
                    </th>
                </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
                {#each rooms as room}
                    <tr>
                        <td class="px-6 py-4 whitespace-nowrap">
                            <div class="text-sm font-medium text-gray-900">{room.number}</div>
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap">
                            <div class="text-sm text-gray-900">{room.type}</div>
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap">
                            <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full {getStatusColor(room.status)}">
                                {room.status}
                            </span>
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                            {room.price} kr
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {room.lastCleaned}
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                            <button class="text-blue-600 hover:text-blue-900 mr-3">Rediger</button>
                            <button class="text-red-600 hover:text-red-900">Slet</button>
                        </td>
                    </tr>
                {/each}
            </tbody>
        </table>
    </div>
</div>