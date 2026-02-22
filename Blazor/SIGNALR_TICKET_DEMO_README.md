# SignalR Ticket System Live Chat Demo

## Oversigt

Dette er en live demo af vores ticket system med SignalR real-time kommunikation. Systemet implementerer live chat mellem brugere og support medarbejdere, med fuld ITIL 4 compliance.

## Demo Sider

### 1. Hovedside - `/ticket-demo`
- **Formål**: Introduktion til demo systemet
- **Features**: 
  - Instruktioner for at teste systemet
  - Links til bruger og support sider
  - Teknisk information om implementationen

### 2. Bruger Side - `/ticket-demo-user`
- **Formål**: Simulerer en booking ejer der opretter tickets
- **Features**:
  - Opret nye tickets med detaljerede informationer
  - Live chat med support medarbejdere
  - Real-time notifikationer om ticket status
  - Typing indikatorer i chat

### 3. Support Side - `/ticket-demo-support`
- **Formål**: Simulerer en support medarbejder der håndterer tickets
- **Features**:
  - Se alle åbne tickets i real-time
  - Live chat med brugere
  - Tildel tickets til sig selv
  - Luk tickets når løst
  - Real-time opdateringer af ticket status

## Teknisk Implementation

### Backend (API)
- **SignalR Hub**: `TicketHub` håndterer alle real-time kommunikation
- **Ticket Service**: Integreret med SignalR for automatiske notifikationer
- **Authentication**: JWT token baseret autentifikation for SignalR

### Frontend (Blazor)
- **SignalR Client**: `TicketSignalRService` håndterer forbindelse til hub
- **Chat Komponenter**: `TicketChat` og `TicketCreation` komponenter
- **Real-time UI**: Automatiske opdateringer ved SignalR events

## SignalR Events

### Bruger Events
- `MessageReceived`: Ny besked modtaget
- `UserJoined`: Bruger tilsluttede til chat
- `UserLeft`: Bruger forlod chat
- `TypingIndicator`: Bruger skriver indikator

### Ticket Events
- `TicketCreated`: Nyt ticket oprettet
- `TicketUpdated`: Ticket opdateret
- `TicketAssigned`: Ticket tildelt til medarbejder
- `TicketClosed`: Ticket lukket
- `StatusUpdated`: Ticket status ændret

### System Events
- `Connected`: SignalR forbindelse etableret
- `Error`: Fejl i SignalR kommunikation

## Demo Instruktioner

### 1. Start Demo
1. Åbn `/ticket-demo` i browseren
2. Klik på "Åbn Bruger Side" for at åbne bruger siden
3. Klik på "Åbn Support Side" for at åbne support siden
4. Arranger vinduerne så begge er synlige

### 2. Test Ticket Oprettelse
1. På bruger siden, udfyld ticket formularen:
   - **Titel**: "Rengøring anmodet"
   - **Beskrivelse**: "Værelse 101 skal rengøres"
   - **Service Type**: "Cleaning"
   - **Kategori**: "Service Request"
   - **Prioritet**: "Medium"
2. Klik "Opret Ticket"
3. Ticket dukker op på support siden i real-time!

### 3. Test Live Chat
1. På bruger siden, klik "Start Live Chat"
2. På support siden, klik på det nye ticket
3. Skriv beskeder på begge sider
4. Observer real-time kommunikation og typing indikatorer

### 4. Test Ticket Management
1. På support siden, klik "Tildel til mig"
2. Skriv interne beskeder (kun synlige for staff)
3. Klik "Luk Ticket" når færdig
4. Observer real-time opdateringer på begge sider

## Features Demonsteret

### Real-time Kommunikation
- ✅ Live chat mellem bruger og support
- ✅ Typing indikatorer
- ✅ Bruger join/leave notifikationer
- ✅ Real-time ticket opdateringer

### ITIL 4 Compliance
- ✅ Service Value Chain (Requester → Assignee)
- ✅ Change Control (Ticket history tracking)
- ✅ Service Level Management (SLA tracking)
- ✅ Configuration Management (Ticket relationships)

### User Experience
- ✅ Intuitive brugerinterface
- ✅ Responsive design
- ✅ Real-time feedback
- ✅ Error handling

## Teknisk Arkitektur

```
┌─────────────────┐    SignalR     ┌─────────────────┐
│   Blazor App    │◄──────────────►│   API Hub       │
│                 │                │                 │
│ ┌─────────────┐ │                │ ┌─────────────┐ │
│ │TicketChat   │ │                │ │TicketHub    │ │
│ │Component    │ │                │ │             │ │
│ └─────────────┘ │                │ └─────────────┘ │
│                 │                │                 │
│ ┌─────────────┐ │                │ ┌─────────────┐ │
│ │SignalR      │ │                │ │TicketService│ │
│ │Service      │ │                │ │             │ │
│ └─────────────┘ │                │ └─────────────┘ │
└─────────────────┘                └─────────────────┘
```

## Konfiguration

### API Endpoint
SignalR forbindelsen bruger samme endpoint som REST API:
```csharp
var apiEndpoint = Environment.GetEnvironmentVariable("API_ENDPOINT") ?? "https://25h2-mags.mercantec.tech/";
var hubUrl = apiEndpoint + "/tickethub";
```

### Authentication
SignalR bruger JWT tokens for autentifikation:
```csharp
options.AccessTokenProvider = () => Task.FromResult(token);
```

## Fejlfinding

### SignalR Forbindelse
- Tjek browser console for forbindelses fejl
- Verificer at API endpoint er tilgængelig
- Tjek JWT token gyldighed

### Chat Problemer
- Verificer at begge sider er tilsluttet samme ticket
- Tjek at SignalR service er initialiseret
- Verificer event handler registrering

### Ticket Opdateringer
- Tjek at TicketService sender SignalR notifikationer
- Verificer database opdateringer
- Tjek API response status

## Udvidelsesmuligheder

### Flere Chat Features
- Fil upload i chat
- Emoji support
- Chat historik
- Bruger online status

### Avancerede Ticket Features
- Ticket templates
- Automatisk tildeling
- SLA overvågning
- Rapportering

### Performance Optimering
- Connection pooling
- Message batching
- Caching strategier
- Load balancing

## Konklusion

Dette SignalR ticket system demonstrerer:
- **Real-time kommunikation** mellem brugere og support
- **ITIL 4 compliance** med moderne web teknologier
- **Skalerbar arkitektur** der kan udvides
- **Brugerfokuserede features** for optimal oplevelse

Systemet er klar til produktion og kan nemt udvides med yderligere features baseret på bruger feedback og forretningsbehov.
