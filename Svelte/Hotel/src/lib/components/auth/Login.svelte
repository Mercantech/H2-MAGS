<!-- src/lib/components/auth/Login.svelte -->
<script>
    import { navigate } from "svelte-routing";
    import { auth } from '../../../stores/auth.store';
    
    let email = '';
    let password = '';
    let error = '';

    async function handleSubmit() {
        try {
            await auth.login(email, password);
            navigate("/", { replace: true });
        } catch (err) {
            error = err.message;
        }
    }
</script>

<div class="min-h-screen flex items-center justify-center bg-gray-100">
    <div class="bg-white p-8 rounded-lg shadow-md w-96">
        <h2 class="text-2xl font-bold mb-6 text-center">Log ind</h2>
        
        {#if error}
            <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                {error}
            </div>
        {/if}

        <form on:submit|preventDefault={handleSubmit} class="space-y-4">
            <div>
                <label class="block text-gray-700 mb-2" for="email">Email</label>
                <input
                    type="email"
                    id="email"
                    bind:value={email}
                    class="w-full p-2 border rounded"
                    required
                />
            </div>

            <div>
                <label class="block text-gray-700 mb-2" for="password">Adgangskode</label>
                <input
                    type="password"
                    id="password"
                    bind:value={password}
                    class="w-full p-2 border rounded"
                    required
                />
            </div>

            <button
                type="submit"
                class="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600"
            >
                Log ind
            </button>
        </form>
    </div>
</div>