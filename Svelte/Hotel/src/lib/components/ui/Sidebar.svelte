<script>
  import { Link } from "svelte-routing";
  let isOpen = true;

  const toggleSidebar = () => {
    isOpen = !isOpen;
  };

  const menuItems = [
    { icon: "🏠", label: "Dashboard", path: "/" },
    { icon: "🛏️", label: "Værelser", path: "/rooms" },
    { icon: "📅", label: "Reservationer", path: "/bookings" },
    { icon: "👥", label: "Gæster", path: "/guests" },
    { icon: "⚙️", label: "Indstillinger", path: "/settings" }
  ];
</script>

<aside class={`min-h-screen bg-gray-800 text-white transition-all duration-300 ${isOpen ? 'w-64' : 'w-20'} fixed left-0 top-0`}>
  <div class="p-4">
    <button 
      class="w-full flex items-center justify-between p-2 hover:bg-gray-700 rounded-lg"
      on:click={toggleSidebar}
    >
      {#if isOpen}
        <span class="text-xl font-bold">Hotel Admin</span>
        <span>←</span>
      {:else}
        <span>→</span>
      {/if}
    </button>
  </div>

  <nav class="mt-4">
    {#each menuItems as item}
      <Link 
        to={item.path}
        class="flex items-center p-4 hover:bg-gray-700 transition-colors"
      >
        <span class="text-xl">{item.icon}</span>
        {#if isOpen}
          <span class="ml-4">{item.label}</span>
        {/if}
      </Link>
    {/each}
  </nav>
</aside>

<div class={`${isOpen ? 'ml-64' : 'ml-20'} transition-all duration-300`}>
  <slot />
</div>

<style>
  aside {
    z-index: 50;
    box-shadow: 4px 0 6px -1px rgba(0, 0, 0, 0.1);
  }
</style>