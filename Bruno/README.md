# Bruno API Samling

Denne mappe indeholder Bruno API-samlingen til test og dokumentation af projektets REST API endpoints.

## Hvad er Bruno?

[Bruno](https://www.usebruno.com/) er et moderne, open-source API-testværktøj, der fungerer som et alternativ til Postman. Bruno gemmer alle API-kald som filer i dit projekt, hvilket gør det nemt at versionsstyre og dele API-tests med teamet.

## Mappestruktur

```
Bruno/
└── API/
    ├── bruno.json              # Bruno samlings-konfiguration
    ├── collection.bru          # Hovedsamlings-fil
    ├── environments/           # Miljøvariabler (tom for nu)
    ├── Hotels/                 # Hotel-relaterede API endpoints
    ├── Status/                 # Status og sundhedstjek endpoints
    ├── Users/                  # Bruger- og autentificerings endpoints
    └── WeatherForecast/        # Vejrudsigts endpoints
```

## API Endpoints

### Status Endpoints
- **Ping**: Simpelt ping-endpoint til at teste API'en
- **Health Check**: Tjekker om API'en kører korrekt
- **Database Check**: Tjekker om databasen er tilgængelig

### Users Endpoints
- **Registrer bruger**: Opretter en ny bruger i systemet
- **Login**: Autentificerer bruger og returnerer JWT token
- **Hent brugerinfo**: Henter information om den nuværende bruger
- **Hent alle brugere**: Henter alle brugere (kun for administratorer)
- **Opdater bruger**: Opdaterer en eksisterende bruger
- **Slet bruger**: Sletter en bruger fra systemet
- **Tildel rolle**: Tildeler en rolle til en bruger
- **Fjern rolle**: Fjerner en brugers rolle
- **Hent roller**: Henter alle tilgængelige roller
- **Hent brugere efter rolle**: Henter alle brugere med en specifik rolle

### Hotels Endpoints
- **Opret hotel**: Opretter et nyt hotel i systemet
- **Hent alle hoteller**: Henter alle hoteller fra systemet
- **Hent hotel**: Henter et specifikt hotel baseret på ID
- **Opdater hotel**: Opdaterer et eksisterende hotel
- **Slet hotel**: Sletter et hotel fra systemet

### WeatherForecast Endpoints
- **Hent vejrudsigt**: Henter en liste af vejrudsigter for de næste 5 dage

## Sådan bruger du Bruno

### Installation
1. Download og installer Bruno fra [usebruno.com](https://www.usebruno.com/)
2. Åbn Bruno applikationen
3. Vælg "Open Collection" og naviger til `Bruno/API` mappen

### Konfiguration
1. Sørg for at API'en kører lokalt (normalt på `https://localhost:7139`)
2. Konfigurer miljøvariabler hvis nødvendigt:
   - `baseUrl`: Base URL til API'en (f.eks. `https://localhost:7139`)
   - `apiKey`: JWT token til autentificering (hvis påkrævet)

### Brug af endpoints
1. Vælg det ønskede endpoint fra samlingen
2. Tjek at URL og parametre er korrekte
3. Klik på "Send" for at udføre API-kaldet
4. Se svaret i response-panelet

### Autentificering
Mange endpoints kræver autentificering via JWT token:
1. Brug "Login" endpointet først for at få et JWT token
2. Kopier token fra response
3. Sæt token som `Authorization` header i andre requests: `Bearer <token>`

## Tips og tricks

- **Miljøvariabler**: Brug `{{variabel}}` syntax til at referere til miljøvariabler
- **Versionsstyring**: Alle .bru filer kan versionsstyres med Git
- **Deling**: Del samlingen med teamet ved at committe filerne
- **Organisering**: Endpoints er organiseret i mapper efter funktionalitet

## Fejlfinding

- Hvis API'en ikke svarer, tjek at den kører på den korrekte port
- Hvis du får 401/403 fejl, tjek at du har et gyldigt JWT token
- Hvis endpoints mangler, tjek at API'en er opdateret til den nyeste version

## Bidrag

Når du tilføjer nye API endpoints, husk at:
1. Oprette tilsvarende .bru filer i den relevante mappe
2. Bruge beskrivende filnavne på dansk
3. Inkludere relevante headers og autentificering
4. Teste endpoints før commit
