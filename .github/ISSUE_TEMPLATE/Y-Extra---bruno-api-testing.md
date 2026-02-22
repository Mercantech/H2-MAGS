---
name: Uge 5 - Bruno API Testing
about: Implementér Bruno API testing og test resultat visning
title: 'Uge 5 - Bruno API Testing'
labels: ['uge-5', 'testing', 'api', 'bruno']
assignees: ''
---

## Bruno API Testing og Test Resultat Visning

- [ ] Opret Bruno test collection for jeres API
  - [ ] Test alle CRUD operationer for jeres endpoints
  - [ ] Test authentication og authorization
  - [ ] Test fejlhåndtering og edge cases
  - [ ] Lav et flow til jeres runner, som kører vigtigste User-Flows
- [ ] Konfigurer Bruno til at køre automatisk i Docker
  - [ ] Opret Bruno container i docker-compose
  - [ ] Konfigurer test resultater til at gemmes som JSON og HTML
  - [ ] Test at Bruno kører korrekt med jeres API
- [ ] Implementér test resultat visning i Blazor
  - [ ] Opret API endpoint til at hente test resultater
  - [ ] Vis test resultater i Blazor komponent
  - [ ] Implementér download af HTML rapporter
- [ ] Dokumentér jeres test strategi og resultater

---
### Eksempel på Bruno test struktur
```javascript
// Test for GET /api/users
test("Should return list of users", function() {
  expect(res.getStatus()).to.equal(200);
  expect(res.getBody()).to.be.an('array');
  expect(res.getBody().length).to.be.greaterThan(0);
});

// Test for authentication
test("Should require authentication", function() {
  expect(res.getStatus()).to.equal(401);
});
```

### Eksempel på test resultat API endpoint
```csharp
[HttpGet("overview")]
public ActionResult<TestResultsOverview> GetOverview()
{
    var files = Directory.GetFiles(_testResultsPath)
        .Where(f => f.EndsWith(".json") || f.EndsWith(".html"))
        .Select(f => new TestFileInfo
        {
            Filename = Path.GetFileName(f),
            LastModified = File.GetLastWriteTime(f),
            Size = new FileInfo(f).Length,
            Type = Path.GetExtension(f).ToLower() == ".json" ? "json" : "html"
        })
        .OrderByDescending(f => f.LastModified)
        .ToList();
    
    return Ok(new TestResultsOverview { AvailableResults = files });
}
```

### Docker Compose eksempel
```yaml
bruno-tests:
  image: node:20-alpine
  container_name: h2-bruno-tests
  working_dir: /app
  volumes:
    - ./Bruno:/app/Bruno
    - ./test-results:/app/test-results
  command: >
    sh -c "
      npm install -g @usebruno/cli &&
      sleep 30 &&
      cd Bruno/API &&
      npx @usebruno/cli run --env H2-MAGS --output /app/test-results/test-report-$$(date +%Y-%m-%d_%H-%M-%S).html --format html &&
      npx @usebruno/cli run --env H2-MAGS --output /app/test-results/test-results-$$(date +%Y-%m-%d_%H-%M-%S).json --format json
    "
  depends_on:
    - api
```

### Krav til test coverage
- [ ] Minimum 80% af jeres API endpoints skal have tests
- [ ] Alle CRUD operationer skal testes
- [ ] Authentication og authorization skal testes
- [ ] Fejlhåndtering skal testes
- [ ] Test resultater skal vises i Blazor frontend

### Bonus opgaver
- [ ] Implementér test resultat historik
- [ ] Opret test resultat sammenligning
- [ ] Implementér automatisk test kørsel ved deployment
- [ ] Opret test resultat notifikationer
