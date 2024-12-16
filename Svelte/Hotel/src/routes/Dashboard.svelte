<!-- src/routes/Dashboard.svelte -->
<script>
    import { fade } from 'svelte/transition';
</script>

<div class="p-8" in:fade>
    <h1 class="text-2xl font-bold mb-6">Dashboard</h1>
    
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <!-- Stats Cards -->
        <div class="bg-white p-6 rounded-lg shadow-sm">
            <h3 class="text-gray-500 text-sm">Totale Bookinger</h3>
            <p class="text-3xl font-bold">128</p>
            <span class="text-green-500 text-sm">+12% siden sidste måned</span>
        </div>

        <div class="bg-white p-6 rounded-lg shadow-sm">
            <h3 class="text-gray-500 text-sm">Ledige Værelser</h3>
            <p class="text-3xl font-bold">24</p>
            <span class="text-red-500 text-sm">-3 siden i går</span>
        </div>

        <div class="bg-white p-6 rounded-lg shadow-sm">
            <h3 class="text-gray-500 text-sm">Dagens Check-ins</h3>
            <p class="text-3xl font-bold">8</p>
            <span class="text-gray-500 text-sm">Forventet ankomst</span>
        </div>

        <div class="bg-white p-6 rounded-lg shadow-sm">
            <h3 class="text-gray-500 text-sm">Omsætning (Denne Måned)</h3>
            <p class="text-3xl font-bold">92.458 kr</p>
            <span class="text-green-500 text-sm">+8% siden sidste måned</span>
        </div>
    </div>

    <!-- Recent Activity -->
    <div class="mt-8 bg-white rounded-lg shadow-sm">
        <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Seneste Aktivitet</h2>
            <div class="space-y-4">
                <div class="flex items-center justify-between border-b pb-4">
                    <div>
                        <p class="font-medium">Ny booking - Værelse 203</p>
                        <p class="text-sm text-gray-500">John Doe - 3 nætter</p>
                    </div>
                    <span class="text-sm text-gray-500">For 2 timer siden</span>
                </div>
                <div class="flex items-center justify-between border-b pb-4">
                    <div>
                        <p class="font-medium">Check-ud - Værelse 105</p>
                        <p class="text-sm text-gray-500">Jane Smith</p>
                    </div>
                    <span class="text-sm text-gray-500">For 4 timer siden</span>
                </div>
                <div class="flex items-center justify-between">
                    <div>
                        <p class="font-medium">Rengøring Færdig - Værelse 308</p>
                        <p class="text-sm text-gray-500">Klar til check-ind</p>
                    </div>
                    <span class="text-sm text-gray-500">For 5 timer siden</span>
                </div>
            </div>
        </div>
    </div>
</div>