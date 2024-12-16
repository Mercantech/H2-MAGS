<script>
  import { Router, Link, Route } from "svelte-routing";
  import AuthGuard from './lib/components/auth/AuthGuard.svelte';
  import Sidebar from './lib/components/ui/Sidebar.svelte';
  import Dashboard from './routes/Dashboard.svelte';
  import Rooms from './routes/Rooms.svelte';
  import Guests from './routes/Guests.svelte';

  export let url = "";
</script>

<Router {url}>
  <AuthGuard>
    <div class="flex">
      <Sidebar />
      <main class="flex-1">
        <Route path="/" component={Dashboard} />
        <Route path="/rooms" component={Rooms} />
        <Route path="/guests" component={Guests} />
      </main>
    </div>
  </AuthGuard>
</Router>
