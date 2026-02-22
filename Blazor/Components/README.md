# Components

Denne mappe indeholder genanvendelige UI-komponenter, som kan bruges på tværs af flere sider i Blazor-applikationen.

## Formål

- Adskille UI-logik fra side-logik
- Gøre komponenter genanvendelige
- Forbedre vedligeholdelse og testbarhed
- Holde koden DRY (Don't Repeat Yourself)

## Eksempler på komponenter

- **StatusCard**: Viser status for forskellige systemer
- **WeatherTable**: Viser vejrdata i tabelformat
- **Counter**: Interaktiv tæller-komponent
- **RoomAvailabilitySearch**: Avanceret søgekomponent til at finde ledige hotelrum

## RoomAvailabilitySearch

En omfattende komponent til at søge efter ledige hotelrum med følgende funktioner:

### Funktionalitet:
- Hotel-valg dropdown
- Dato-valg for check-in/check-out
- Antal gæster
- Avancerede filtre (rumtype, pris, faciliteter)
- Søgeresultater med rum-kort
- Responsivt design med Bootstrap

### Brug:
```razor
<RoomAvailabilitySearch />
```

Komponenten bruger `APIService.SearchRoomAvailabilityAsync()` til at kalde API'et og viser resultater i et responsivt card-layout.
