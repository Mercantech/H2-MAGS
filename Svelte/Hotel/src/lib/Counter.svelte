<script>
  let count = $state(0)
  const increment = () => {
    count += 1
  }
</script>

<button onclick={increment}>
  count is {count}
</button>
<div class="bg-blue-500 text-white p-4 rounded-lg">
  Dette er en test af Tailwind CSS
</div>