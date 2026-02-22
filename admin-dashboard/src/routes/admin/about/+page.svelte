<script lang="ts">
	import { BookOpen, Code, Zap, Globe, Shield, ArrowLeft } from 'lucide-svelte';
</script>

<svelte:head>
	<title>Om Svelte & API - Receptionist Dashboard</title>
</svelte:head>

<div class="space-y-8">
	<!-- Header -->
	<div class="md:flex md:items-center md:justify-between">
		<div class="flex-1 min-w-0">
			<nav class="flex" aria-label="Breadcrumb">
				<ol class="flex items-center space-x-4">
					<li>
						<div>
							<a href="/admin/dashboard" class="text-gray-400 hover:text-gray-500">
								<ArrowLeft class="h-5 w-5" />
								<span class="sr-only">Tilbage</span>
							</a>
						</div>
					</li>
					<li>
						<div class="flex items-center">
							<span class="text-gray-400">/</span>
							<span class="ml-4 text-sm font-medium text-gray-500">Dashboard</span>
						</div>
					</li>
					<li>
						<div class="flex items-center">
							<span class="text-gray-400">/</span>
							<span class="ml-4 text-sm font-medium text-gray-900">Om Svelte & API</span>
						</div>
					</li>
				</ol>
			</nav>
			<h1 class="mt-2 text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Om Svelte & API Arkitektur
			</h1>
			<p class="mt-1 text-sm text-gray-500">
				Lær om Svelte, sammenligning med Blazor og fordelene ved en god API struktur
			</p>
		</div>
	</div>

	<!-- Svelte vs Blazor Comparison -->
	<div class="bg-white shadow rounded-lg">
		<div class="px-4 py-5 sm:p-6">
			<h2 class="text-lg leading-6 font-medium text-gray-900 mb-6">
				<Code class="h-6 w-6 inline mr-2" />
				Svelte vs Blazor - Sammenligning
			</h2>
			
			<div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
				<!-- Svelte -->
				<div class="space-y-4">
					<div class="flex items-center">
						<div class="h-10 w-10 rounded-full bg-orange-100 flex items-center justify-center">
							<Zap class="h-5 w-5 text-orange-600" />
						</div>
						<h3 class="ml-3 text-lg font-medium text-gray-900">Svelte</h3>
					</div>
					
					<div class="space-y-3">
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Compile-time optimering</p>
								<p class="text-sm text-gray-500">Ingen virtual DOM, direkte DOM manipulation</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Mindre bundle størrelse</p>
								<p class="text-sm text-gray-500">Typisk 50-80% mindre end React/Vue</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Enkel syntax</p>
								<p class="text-sm text-gray-500">Mindre boilerplate kode</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Reaktive statements</p>
								<p class="text-sm text-gray-500">Automatisk opdatering når data ændres</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-yellow-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Mindre ecosystem</p>
								<p class="text-sm text-gray-500">Færre tredjeparts biblioteker</p>
							</div>
						</div>
					</div>
				</div>

				<!-- Blazor -->
				<div class="space-y-4">
					<div class="flex items-center">
						<div class="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
							<Globe class="h-5 w-5 text-blue-600" />
						</div>
						<h3 class="ml-3 text-lg font-medium text-gray-900">Blazor</h3>
					</div>
					
					<div class="space-y-3">
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">C# overalt</p>
								<p class="text-sm text-gray-500">Samme sprog på frontend og backend</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Stærk typing</p>
								<p class="text-sm text-gray-500">Compile-time fejlopdagelse</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">.NET ecosystem</p>
								<p class="text-sm text-gray-500">Fuld adgang til .NET biblioteker</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-green-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Server-side rendering</p>
								<p class="text-sm text-gray-500">Bedre SEO og initial load</p>
							</div>
						</div>
						
						<div class="flex items-start">
							<div class="flex-shrink-0">
								<div class="h-2 w-2 bg-yellow-400 rounded-full mt-2"></div>
							</div>
							<div class="ml-3">
								<p class="text-sm font-medium text-gray-900">Større bundle</p>
								<p class="text-sm text-gray-500">.NET runtime skal downloades</p>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>

	<!-- API Architecture Benefits -->
	<div class="bg-white shadow rounded-lg">
		<div class="px-4 py-5 sm:p-6">
			<h2 class="text-lg leading-6 font-medium text-gray-900 mb-6">
				<Shield class="h-6 w-6 inline mr-2" />
				Fordele ved en god API struktur
			</h2>
			
			<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center mb-3">
						<Globe class="h-6 w-6 text-indigo-600 mr-2" />
						<h3 class="text-sm font-medium text-gray-900">Multi-Framework Support</h3>
					</div>
					<p class="text-sm text-gray-600">
						Samme API kan bruges med Svelte, React, Vue, Angular, Blazor, eller hvilken som helst frontend teknologi.
					</p>
				</div>

				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center mb-3">
						<Zap class="h-6 w-6 text-green-600 mr-2" />
						<h3 class="text-sm font-medium text-gray-900">Fleksibilitet</h3>
					</div>
					<p class="text-sm text-gray-600">
						Skift frontend teknologi uden at ændre backend. Test forskellige frameworks med samme data.
					</p>
				</div>

				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center mb-3">
						<Shield class="h-6 w-6 text-blue-600 mr-2" />
						<h3 class="text-sm font-medium text-gray-900">Sikkerhed</h3>
					</div>
					<p class="text-sm text-gray-600">
						Centraliseret autentificering og autorisation. JWT tokens fungerer på tværs af alle frontend frameworks.
					</p>
				</div>

				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center mb-3">
						<Code class="h-6 w-6 text-purple-600 mr-2" />
						<h3 class="text-sm font-medium text-gray-900">Skalérbarhed</h3>
					</div>
					<p class="text-sm text-gray-600">
						Backend og frontend kan skaleres uafhængigt. Microservices arkitektur mulig.
					</p>
				</div>

				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center mb-3">
						<BookOpen class="h-6 w-6 text-orange-600 mr-2" />
						<h3 class="text-sm font-medium text-gray-900">Dokumentation</h3>
					</div>
					<p class="text-sm text-gray-600">
						Swagger/OpenAPI dokumentation gør det nemt for udviklere at forstå og bruge API'en.
					</p>
				</div>

				<div class="bg-gray-50 rounded-lg p-4">
					<div class="flex items-center mb-3">
						<Globe class="h-6 w-6 text-red-600 mr-2" />
						<h3 class="text-sm font-medium text-gray-900">Mobile Support</h3>
					</div>
					<p class="text-sm text-gray-600">
						Samme API kan bruges til mobile apps (React Native, Flutter, Xamarin) og desktop apps.
					</p>
				</div>
			</div>
		</div>
	</div>

	<!-- Technology Stack -->
	<div class="bg-white shadow rounded-lg">
		<div class="px-4 py-5 sm:p-6">
			<h2 class="text-lg leading-6 font-medium text-gray-900 mb-6">
				<BookOpen class="h-6 w-6 inline mr-2" />
				Vores Teknologi Stack
			</h2>
			
			<div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
				<!-- Backend -->
				<div>
					<h3 class="text-md font-medium text-gray-900 mb-4">Backend (.NET API)</h3>
					<div class="space-y-2">
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-blue-500 rounded-full mr-3"></div>
							ASP.NET Core Web API
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-green-500 rounded-full mr-3"></div>
							Entity Framework Core
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-purple-500 rounded-full mr-3"></div>
							PostgreSQL Database
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-orange-500 rounded-full mr-3"></div>
							JWT Authentication
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-red-500 rounded-full mr-3"></div>
							Active Directory Integration
						</div>
					</div>
				</div>

				<!-- Frontend -->
				<div>
					<h3 class="text-md font-medium text-gray-900 mb-4">Frontend (Svelte)</h3>
					<div class="space-y-2">
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-orange-500 rounded-full mr-3"></div>
							SvelteKit
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-blue-500 rounded-full mr-3"></div>
							TypeScript
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-cyan-500 rounded-full mr-3"></div>
							Tailwind CSS
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-green-500 rounded-full mr-3"></div>
							Axios HTTP Client
						</div>
						<div class="flex items-center text-sm text-gray-600">
							<div class="h-2 w-2 bg-purple-500 rounded-full mr-3"></div>
							Lucide Icons
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>

	<!-- Code Examples -->
	<div class="bg-white shadow rounded-lg">
		<div class="px-4 py-5 sm:p-6">
			<h2 class="text-lg leading-6 font-medium text-gray-900 mb-6">
				<Code class="h-6 w-6 inline mr-2" />
				Kode Eksempler
			</h2>
			
			<div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
				<!-- Svelte Example -->
				<div>
					<h3 class="text-md font-medium text-gray-900 mb-3">Svelte Komponent</h3>
					<div class="bg-gray-900 rounded-lg p-4 overflow-x-auto">
						<pre class="text-sm text-green-400"><code>{`<script lang="ts">
  import { onMount } from 'svelte';
  import { bookings } from '$lib/stores/bookings';
  
  let isLoading = true;
  
  onMount(async () => {
    await loadBookings();
    isLoading = false;
  });
</script>

<div class="space-y-4">
  {#if isLoading}
    <div class="animate-pulse">
      Loading...
    </div>
  {:else}
    {#each $bookings as booking}
      <div class="p-4 bg-white rounded-lg shadow">
        {booking.user?.username}
      </div>
    {/each}
  {/if}
</div>`}</code></pre>
					</div>
				</div>

				<!-- API Example -->
				<div>
					<h3 class="text-md font-medium text-gray-900 mb-3">API Endpoint</h3>
					<div class="bg-gray-900 rounded-lg p-4 overflow-x-auto">
						<pre class="text-sm text-blue-400"><code>{`[HttpGet("bookings")]
public async Task<IActionResult> GetBookings()
{
    var bookings = await _context.Bookings
        .Include(b => b.User)
        .Include(b => b.Room)
        .ToListAsync();
    
    return Ok(bookings);
}

[HttpPost("bookings")]
[Authorize]
public async Task<IActionResult> CreateBooking([FromBody] BookingDto dto)
{
    var booking = new Booking
    {
        UserId = dto.UserId,
        RoomId = dto.RoomId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate
    };
    
    _context.Bookings.Add(booking);
    await _context.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetBooking), 
        new { id = booking.Id }, booking);
}`}</code></pre>
					</div>
				</div>
			</div>
		</div>
	</div>

	<!-- Benefits Summary -->
	<div class="bg-gradient-to-r from-indigo-50 to-blue-50 rounded-lg p-6">
		<h2 class="text-lg font-medium text-gray-900 mb-4">
			🎯 Hvorfor denne arkitektur?
		</h2>
		<div class="grid grid-cols-1 md:grid-cols-2 gap-4">
			<div>
				<h3 class="font-medium text-gray-900 mb-2">For Udviklere</h3>
				<ul class="text-sm text-gray-600 space-y-1">
					<li>• Vælg det framework der passer bedst til projektet</li>
					<li>• Samme API kan bruges til flere frontends</li>
					<li>• Nem at teste og prototyp</li>
					<li>• Klar separation mellem frontend og backend</li>
				</ul>
			</div>
			<div>
				<h3 class="font-medium text-gray-900 mb-2">For Virksomheden</h3>
				<ul class="text-sm text-gray-600 space-y-1">
					<li>• Fleksibilitet i teknologi valg</li>
					<li>• Nem at skifte frontend teknologi</li>
					<li>• Bedre performance og brugeroplevelse</li>
					<li>• Fremtidssikret arkitektur</li>
				</ul>
			</div>
		</div>
	</div>
</div>
