# Active Directory Integration

Denne dokumentation beskriver implementeringen af Active Directory integration i H2-Projekt API'et.

## Oversigt

API'et understøtter nu autentificering mod Active Directory (AD) via LDAP protokollen. Brugere kan logge ind med deres AD credentials og få udstedt JWT tokens baseret på deres AD gruppemedlemskaber.

## Konfiguration

### appsettings.json

```json
{
  "ActiveDirectory": {
    "Server": "10.133.71.100",
    "Domain": "mags.local",
    "ReaderUsername": "adReader",
    "ReaderPassword": "Merc1234!",
    "Port": 389,
    "UseSSL": false
  }
}
```

### NuGet Pakker

- `System.DirectoryServices.Protocols` (Version 9.0.0) - Til LDAP kommunikation

## Implementerede Komponenter

### 1. ActiveDirectoryService

**Fil:** `API/Services/ActiveDirectoryService.cs`

Hovedservice til AD integration:
- Autentificerer brugere mod AD
- Henter brugerinformation og gruppemedlemskaber
- Mapper AD grupper til applikationsroller

**Hovedmetoder:**
- `AuthenticateUserAsync(username, password)` - Autentificerer bruger mod AD
- `MapADGroupToRole(adGroups)` - Mapper AD grupper til roller

### 2. JwtService (Opdateret)

**Fil:** `API/Services/JwtService.cs`

Udvidet med AD support:
- `GenerateTokenForADUser(adUser, role)` - Genererer JWT for AD brugere
- Inkluderer AD-specifikke claims (adUser, adGroups)

### 3. AuthController

**Fil:** `API/Controllers/ADAuthController.cs`

Nye endpoints:
- `POST /api/auth/ad-login` - AD login endpoint
- `GET /api/auth/ad-me` - Hent nuværende AD bruger info
- `GET /api/auth/ad-status` - AD forbindelsesstatus (kun Admin)

## API Endpoints

### AD Login

```http
POST /api/auth/ad-login
Content-Type: application/json

{
  "username": "adReader",
  "password": "Merc1234!"
}
```

**Response:**
```json
{
  "message": "AD Login godkendt!",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "samAccountName": "adReader",
    "email": "adreader@mags.local",
    "displayName": "AD Reader",
    "firstName": "AD",
    "lastName": "Reader",
    "role": "Admin",
    "adGroups": ["Domain Admins", "IT Support"],
    "isADUser": true
  }
}
```

### Hent AD Bruger Info

```http
GET /api/auth/ad-me
Authorization: Bearer <jwt-token>
```

**Response:**
```json
{
  "samAccountName": "adReader",
  "email": "adreader@mags.local",
  "displayName": "AD Reader",
  "role": "Admin",
  "adGroups": ["Domain Admins", "IT Support"],
  "isADUser": true,
  "loginMethod": "Active Directory"
}
```

### AD Status (Admin kun)

```http
GET /api/auth/ad-status
Authorization: Bearer <admin-jwt-token>
```

**Response:**
```json
{
  "adConfigured": true,
  "server": "10.133.71.100",
  "domain": "mags.local",
  "testConnection": true,
  "testUser": "adReader",
  "timestamp": "2025-01-27T10:30:00+02:00"
}
```

## Rolle Mapping

AD grupper mappes til applikationsroller som følger:

| AD Gruppe Indhold | Applikationsrolle |
|-------------------|-------------------|
| "Admin" eller "Administrator" | Admin |
| "Manager" | Manager |
| "User" | User |
| Default | User |

## Sikkerhed

### Rate Limiting
- Implementerer samme rate limiting som traditionel login
- Progressive delays ved mislykkede forsøg
- Konto låsning ved for mange forsøg

### JWT Claims
AD brugere får følgende claims i deres JWT:
- `sub` - SAM Account Name
- `email` - Email fra AD
- `name` - Display Name fra AD
- `role` - Mapped applikationsrolle
- `adUser` - "true" for AD brugere
- `adGroups` - Komma-separeret liste af AD grupper

## Test

### Bruno Tests
Bruno test filer er oprettet i `Bruno/API/Auth/`:
- `AD-Login.bru` - Test AD login
- `AD-Me.bru` - Test hent AD bruger info
- `AD-Status.bru` - Test AD status endpoint

### Manuelt Test
1. Start API'et
2. Test AD forbindelse: `GET /api/auth/ad-status`
3. Test AD login: `POST /api/auth/ad-login`
4. Test AD bruger info: `GET /api/auth/ad-me`

## Fejlhåndtering

### Almindelige Fejl
- **401 Unauthorized** - Forkert credentials eller bruger ikke fundet i AD
- **429 Too Many Requests** - Konto låst på grund af for mange mislykkede forsøg
- **500 Internal Server Error** - AD forbindelsesfejl eller server fejl

### Logging
Alle AD operationer logges med detaljerede fejlmeddelelser:
- Login forsøg (succesfulde og mislykkede)
- AD forbindelsesfejl
- Bruger søgninger
- Rolle mapping

## Fremtidige Forbedringer

1. **Konfigurerbar Rolle Mapping** - Flyt rolle mapping til konfiguration
2. **Caching** - Cache AD brugerinformation for bedre performance
3. **SSL Support** - Understøttelse af LDAPS
4. **Gruppe Hierarki** - Understøttelse af nested grupper
5. **Bruger Sync** - Automatisk synkronisering af AD brugere til lokal database

## Troubleshooting

### Almindelige Problemer

1. **AD forbindelse fejler**
   - Tjek server IP og port
   - Verificer AD reader credentials
   - Tjek netværksforbindelse

2. **Bruger ikke fundet**
   - Verificer username format (sAMAccountName, email, eller UPN)
   - Tjek AD base DN konfiguration

3. **Rolle mapping fejler**
   - Tjek AD gruppemedlemskaber
   - Verificer rolle mapping logik

### Debug Logging
Aktivér detaljeret logging ved at sætte log level til `Debug` i appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "API.Services.ActiveDirectoryService": "Debug"
    }
  }
}
```
