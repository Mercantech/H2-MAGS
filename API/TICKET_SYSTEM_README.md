# Hotel Ticket System - ITIL 4 Implementation

## Oversigt

Dette ticket system er designet til at håndtere service requests og incidents i hotel booking platformen, baseret på ITIL 4 principperne. Systemet implementerer Service Value Chain, Change Control, Service Level Management og Configuration Management.

## ITIL 4 Principper Implementeret

### 1. Service Value Chain
- **Requester**: Den der anmoder om servicen (booking ejer)
- **Assignee**: Den der håndterer ticket (rengøring, reception, etc.)
- **Service**: Hvilken service der anmodes (Cleaning, RoomService, Maintenance, General)

### 2. Service Level Management (SLA)
- **Critical**: 2 timer
- **High**: 8 timer  
- **Medium**: 1 dag
- **Low**: 3 dage

### 3. Change Control
- Alle ændringer trackes i TicketHistory
- Automatisk generering af ticket numre (TKT-2025-001)
- Audit trail for alle operationer

### 4. Configuration Management
- Knytter tickets til specifikke bookings, rum og hoteller
- Validerer at kun booking ejere kan oprette tickets for deres bookinger

## Ticket Typer

### Service Types
- **Cleaning**: Rengøringsservice
- **RoomService**: Room service (mad, drikke, etc.)
- **Maintenance**: Vedligeholdelse
- **General**: Generelle henvendelser

### Categories
- **Incident**: Når noget er gået galt
- **Service Request**: Anmodning om service
- **Problem**: Strukturelle problemer
- **Change**: Ændringer i systemet

### Priorities
- **Critical**: Kritiske problemer (2 timer SLA)
- **High**: Høj prioritet (8 timer SLA)
- **Medium**: Medium prioritet (1 dag SLA)
- **Low**: Lav prioritet (3 dage SLA)

### Status
- **Open**: Nyt ticket
- **InProgress**: I gang med at håndtere
- **Resolved**: Løst, men ikke lukket
- **Closed**: Lukket og færdig
- **Cancelled**: Annulleret

## API Endpoints

### Tickets Controller (`/api/tickets`)

#### GET `/api/tickets`
Hent alle tickets med filtrering og paginering
- **Query Parameters**: 
  - `searchTerm`, `status`, `priority`, `serviceType`, `category`
  - `requesterId`, `assigneeId`, `bookingId`, `roomId`, `hotelId`
  - `createdFrom`, `createdTo`, `dueFrom`, `dueTo`
  - `page`, `pageSize`, `sortBy`, `sortDirection`

#### GET `/api/tickets/{id}`
Hent specifik ticket med kommentarer og vedhæftninger

#### POST `/api/tickets`
Opret nyt ticket (kun booking ejere)
- **Body**: `TicketCreateDto`

#### PUT `/api/tickets/{id}`
Opdater ticket (kun assignee eller admin)
- **Body**: `TicketUpdateDto`

#### DELETE `/api/tickets/{id}`
Slet ticket (kun admin)

#### POST `/api/tickets/{id}/assign`
Tildel ticket til medarbejder (kun admin/reception)
- **Body**: `assigneeId` (string)

#### POST `/api/tickets/{id}/close`
Luk ticket (marker som løst)
- **Body**: `resolution` (string)

#### GET `/api/tickets/{id}/comments`
Hent ticket kommentarer

#### POST `/api/tickets/{id}/comments`
Tilføj kommentar til ticket
- **Body**: `TicketCommentCreateDto`

#### GET `/api/tickets/statistics`
Hent ticket statistikker (kun admin)
- **Query Parameters**: `fromDate`, `toDate`

#### GET `/api/tickets/my-tickets`
Hent mine tickets (tickets hvor brugeren er requester)

#### GET `/api/tickets/assigned-to-me`
Hent tickets tildelt til mig (kun staff)

## Roller og Adgang

### User (Booking ejer)
- Kan oprette tickets for deres egne bookinger
- Kan se deres egne tickets
- Kan kommentere på deres tickets
- Kan lukke deres egne tickets

### CleaningStaff
- Kan se tickets med service type "Cleaning"
- Kan tildele tickets til sig selv
- Kan opdatere tickets de er tildelt
- Kan lukke tickets de er tildelt

### Reception
- Kan se tickets med service type "RoomService"
- Kan tildele tickets til medarbejdere
- Kan opdatere alle tickets
- Kan lukke alle tickets

### Admin
- Fuld adgang til alle tickets
- Kan slette tickets
- Kan se statistikker
- Kan tildele tickets til alle

## Database Schema

### Tickets Tabel
```sql
- Id (Primary Key)
- TicketNumber (Unique)
- Title
- Description
- ServiceType
- Priority
- Status
- Category
- SubCategory
- RequesterId (FK til Users)
- AssigneeId (FK til Users)
- BookingId (FK til Bookings)
- RoomId (FK til Rooms)
- HotelId (FK til Hotels)
- DueDate (SLA deadline)
- ResolvedAt
- ClosedAt
- Resolution
- WorkNotes
- RiskLevel
- Impact
- CreatedAt
- UpdatedAt
```

### TicketComments Tabel
```sql
- Id (Primary Key)
- TicketId (FK til Tickets)
- AuthorId (FK til Users)
- Comment
- IsInternal (kun synlig for staff)
- CreatedAt
- UpdatedAt
```

### TicketAttachments Tabel
```sql
- Id (Primary Key)
- TicketId (FK til Tickets)
- FileName
- FilePath
- ContentType
- FileSize
- UploadedById (FK til Users)
- CreatedAt
- UpdatedAt
```

### TicketHistories Tabel
```sql
- Id (Primary Key)
- TicketId (FK til Tickets)
- FieldName
- OldValue
- NewValue
- ChangedById (FK til Users)
- ChangeReason
- CreatedAt
- UpdatedAt
```

## Eksempel på Brug

### 1. Opret Ticket (Booking ejer)
```http
POST /api/tickets
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Rengøring ønsket",
  "description": "Vil gerne have rummet rengjort i morgen",
  "serviceType": "Cleaning",
  "category": "Service Request",
  "priority": "Medium",
  "bookingId": "booking-id-here"
}
```

### 2. Tildel Ticket (Reception)
```http
POST /api/tickets/ticket-id/assign
Authorization: Bearer <token>
Content-Type: application/json

"assignee-id-here"
```

### 3. Luk Ticket (Cleaning Staff)
```http
POST /api/tickets/ticket-id/close
Authorization: Bearer <token>
Content-Type: application/json

"Rummet er blevet rengjort som ønsket"
```

## Sikkerhed

- Alle endpoints kræver JWT authentication
- Role-based access control implementeret
- Booking ejere kan kun oprette tickets for deres egne bookinger
- Staff kan kun se tickets relevant for deres rolle
- Alle ændringer logges for auditing

## Monitoring og Statistikker

Systemet tilbyder omfattende statistikker:
- Total antal tickets
- Tickets per status
- Tickets per prioritet
- Tickets per service type
- Gennemsnitlig løsningstid
- Performance metrics

## Integration med Eksisterende System

Ticket systemet integrerer perfekt med:
- **Booking System**: Tickets kan knyttes til specifikke bookinger
- **Room Management**: Tickets kan knyttes til specifikke rum
- **Hotel Management**: Tickets kan knyttes til specifikke hoteller
- **User Management**: Tickets bruger eksisterende brugerroller
- **Email System**: Kan udvides til at sende notifikationer

## Fremtidige Forbedringer

- Email notifikationer ved status ændringer
- File upload til ticket attachments
- Mobile app integration
- Advanced reporting dashboard
- SLA breach alerts
- Automated ticket routing baseret på regler
