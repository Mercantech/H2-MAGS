# Ticket System Bruno Test Suite

## Oversigt

Denne Bruno test suite dækker det komplette ticket system med fokus på bruger booking workflow og ITIL 4 compliance. Testene simulerer reelle scenarier hvor brugere opretter tickets relateret til deres bookinger.

## Test Struktur

### 1. Ticket Oprettelse med Booking (Tests 1-15)
- **Test 1**: Opret ticket med booking reference
- **Test 11**: Opret generelt ticket uden booking
- **Test 13**: Opret room service ticket
- **Test 14**: Opret maintenance ticket
- **Test 15**: Opret generel support ticket

### 2. Ticket Management (Tests 2-10)
- **Test 2**: Hent alle tickets med filtrering
- **Test 3**: Hent specifik ticket detaljer
- **Test 4**: Tildel ticket til support medarbejder
- **Test 5**: Tilføj kommentar til ticket
- **Test 6**: Hent ticket kommentarer
- **Test 7**: Opdater ticket status
- **Test 8**: Luk ticket med løsning
- **Test 9**: Hent ticket statistikker
- **Test 10**: Hent ticket historik

### 3. Avancerede Features (Tests 12, 16-20)
- **Test 12**: Søg efter tickets
- **Test 16**: Opdater ticket med work notes
- **Test 17**: Hent bruger tickets
- **Test 18**: Hent tildelte tickets
- **Test 19**: Hent ticket attachments
- **Test 20**: Hent ticket dashboard

## Test Scenarier

### Scenario 1: Rengøring Anmodning
1. **Opret ticket** med booking reference for rengøring
2. **Hent ticket** detaljer
3. **Tildel ticket** til support medarbejder
4. **Tilføj kommentar** fra support
5. **Opdater status** med work notes
6. **Luk ticket** med løsning

### Scenario 2: Room Service Bestilling
1. **Opret ticket** for room service med booking reference
2. **Hent alle tickets** for at se det nye ticket
3. **Tildel ticket** til support medarbejder
4. **Tilføj kommentar** med bekræftelse
5. **Luk ticket** når service er leveret

### Scenario 3: Vedligeholdelse Problem
1. **Opret ticket** for vandlækage med critical priority
2. **Hent ticket** detaljer
3. **Tildel ticket** til support medarbejder
4. **Opdater status** med work notes
5. **Luk ticket** når problem er løst

### Scenario 4: Generel Support
1. **Opret ticket** for generel information uden booking
2. **Hent bruger tickets** for at se alle brugerens tickets
3. **Søg efter tickets** med forskellige kriterier
4. **Hent dashboard** data for oversigt

## Test Data

### Variabler
```javascript
{
  "baseUrl": "{{baseUrl}}/api/tickets",
  "authToken": "{{authToken}}",
  "userId": "{{userId}}",
  "supportUserId": "{{supportUserId}}",
  "hotelId": "{{hotelId}}",
  "roomId": "{{roomId}}",
  "bookingId": "{{bookingId}}",
  "ticketId": "{{ticketId}}"
}
```

### Test Tickets
1. **Rengøring Ticket**: Service type "Cleaning", Medium priority
2. **Room Service Ticket**: Service type "RoomService", Medium priority
3. **Maintenance Ticket**: Service type "Maintenance", Critical priority
4. **General Ticket**: Service type "General", Low priority

## ITIL 4 Compliance

### Service Value Chain
- **Requester**: Bruger der opretter ticket
- **Assignee**: Support medarbejder der håndterer ticket
- **Service**: Cleaning, RoomService, Maintenance, General

### Change Control
- Alle ændringer trackes i ticket historik
- Automatisk generering af ticket numre
- Audit trail for alle operationer

### Service Level Management
- **Critical**: 2 timer
- **High**: 8 timer
- **Medium**: 1 dag
- **Low**: 3 dage

### Configuration Management
- Ticket knytter til booking, room, hotel
- Bruger information tracking
- Service type kategorisering

## Test Features

### Real-time Kommunikation
- SignalR integration testing
- Live chat simulation
- Real-time notifikationer

### Bruger Adgangskontrol
- JWT token authentication
- Role-based access control
- User-specific ticket filtering

### Performance Testing
- Response time validation
- Load testing simulation
- Error handling verification

### Data Validation
- Required field validation
- Data type validation
- Business rule validation

## Kørsel af Tests

### 1. Forberedelse
```bash
# Sæt environment variabler
export API_ENDPOINT="https://25h2-mags.mercantec.tech"
export AUTH_TOKEN="your-jwt-token"
export USER_ID="user-id"
export SUPPORT_USER_ID="support-user-id"
export HOTEL_ID="hotel-id"
export ROOM_ID="room-id"
export BOOKING_ID="booking-id"
```

### 2. Kør alle tests
```bash
bruno test
```

### 3. Kør specifik test
```bash
bruno test "01-Create-Ticket-With-Booking"
```

### 4. Kør test suite med rapport
```bash
bruno test --report
```

## Test Resultater

### Success Criteria
- ✅ Alle tests returnerer 200/201 status
- ✅ Response time under 2 sekunder
- ✅ Data validation passerer
- ✅ Business rules respekteres
- ✅ ITIL 4 compliance verificeret

### Fejl Handling
- ❌ Authentication fejl
- ❌ Validation fejl
- ❌ Business rule fejl
- ❌ Performance fejl
- ❌ Data consistency fejl

## Udvidelsesmuligheder

### Flere Test Scenarier
- Multi-user testing
- Concurrent ticket creation
- Bulk operations
- Error scenarios

### Performance Testing
- Load testing
- Stress testing
- Endurance testing
- Scalability testing

### Integration Testing
- Database integration
- SignalR integration
- Email service integration
- File upload testing

## Konklusion

Denne test suite giver omfattende dækning af ticket systemet og sikrer:
- **Funktionalitet**: Alle features fungerer korrekt
- **Performance**: Systemet håndterer load effektivt
- **Security**: Adgangskontrol fungerer korrekt
- **Compliance**: ITIL 4 principper følges
- **User Experience**: Brugere kan oprette og håndtere tickets effektivt

Testene er designet til at være realistiske og dække de mest almindelige brugsscenarier i et hotel booking system.
