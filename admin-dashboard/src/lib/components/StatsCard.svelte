<script lang="ts">
	export let title: string;
	export let value: string | number;
	export let icon: any;
	export let color: string = 'indigo';
	export let change: string | null = null;
	export let changeType: 'positive' | 'negative' | 'neutral' = 'neutral';

	const colorClasses = {
		indigo: 'bg-indigo-500',
		green: 'bg-green-500',
		yellow: 'bg-yellow-500',
		red: 'bg-red-500',
		blue: 'bg-blue-500',
		purple: 'bg-purple-500'
	};

	const changeClasses = {
		positive: 'text-green-600',
		negative: 'text-red-600',
		neutral: 'text-gray-600'
	};
</script>

<div class="overflow-hidden rounded-lg bg-white shadow">
	<div class="p-5">
		<div class="flex items-center">
			<div class="flex-shrink-0">
				<div class="p-3 rounded-md {colorClasses[color]}">
					<svelte:component this={icon} class="h-6 w-6 text-white" />
				</div>
			</div>
			<div class="ml-5 w-0 flex-1">
				<dl>
					<dt class="text-sm font-medium text-gray-500 truncate">
						{title}
					</dt>
					<dd class="flex items-baseline">
						<div class="text-2xl font-semibold text-gray-900">
							{value}
						</div>
						{#if change}
							<div class="ml-2 flex items-baseline text-sm font-semibold {changeClasses[changeType]}">
								{change}
							</div>
						{/if}
					</dd>
				</dl>
			</div>
		</div>
	</div>
</div>
