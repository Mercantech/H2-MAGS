# Kør hele collection og gem som JSON
npx @usebruno/cli run --env H2-MAGS --output results.json --format json

# Kør hele collection og gem som HTML rapport
npx @usebruno/cli run --env H2-MAGS --output report.html --format html

# Kør kun specifikke mappe og gem resultater
npx @usebruno/cli run Status --env H2-MAGS --output status-results.json --format json

# Kør med timestamp i filnavn
npx @usebruno/cli run --env H2-MAGS --output "test-$(Get-Date -Format 'yyyy-MM-dd-HHmm').json" --format json