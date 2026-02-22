# Docker Active Directory Setup Guide

## Problem
Din C# Web API kan ikke oprette forbindelse til Active Directory serveren når den kører i Docker container, selvom den virker lokalt.

## Løsning
Jeg har implementeret flere forbedringer for at løse dette problem:

### 1. Docker Compose Konfiguration
- Fjernet `network_mode: host` fra API service
- Tilføjet `dns` konfiguration med Google DNS servere
- Tilføjet `extra_hosts` mapping for AD server
- Forbedret netværkskonfiguration med bridge driver

### 2. Active Directory Service Forbedringer
- Tilføjet fallback forbindelsesmetode (uden SSL)
- Forbedret fejlhåndtering med læsbare fejlmeddelelser
- Tilføjet bedre debugging og logging
- Håndtering af Docker miljø specifikke problemer

### 3. Konfiguration
Opret en `.env` fil med følgende indhold:

```env
# Build Information
BUILD_DATE=2025-01-27T10:00:00Z
VCS_REF=main

# API Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8045
DOTNET_USE_POLLING_FILE_WATCHER=1
DOTNET_RUNNING_IN_CONTAINER=true

# JWT Configuration
JWT_SECRET_KEY=MyVerySecureSecretKeyThatIsAtLeast32CharactersLong123456789
JWT_ISSUER=H2-2025-API
JWT_AUDIENCE=H2-2025-Client
JWT_EXPIRY_MINUTES=60

# Active Directory Configuration
ACTIVEDIRECTORY_SERVER=10.133.71.100
ACTIVEDIRECTORY_DOMAIN=mags.local
ACTIVEDIRECTORY_READERUSERNAME=adReader
ACTIVEDIRECTORY_READERPASSWORD=Merc1234!
ACTIVEDIRECTORY_PORT=389
ACTIVEDIRECTORY_USESSL=false
ACTIVEDIRECTORY_CONNECTIONTIMEOUT=30
ACTIVEDIRECTORY_MAXRETRIES=3
ACTIVEDIRECTORY_RETRYDELAYMS=1000

# Admin Dashboard Configuration
NODE_ENV=production
```

## Test
1. Genopbyg og start containeren:
   ```bash
   docker-compose down
   docker-compose up --build
   ```

2. Test AD forbindelsen:
   ```bash
   curl http://localhost:8751/api/Auth/ad-status
   ```

## Debugging
Hvis problemet fortsætter, tjek følgende:

1. **Netværksadgang**: Sikr at Docker containeren kan nå AD serveren
   ```bash
   docker exec -it h2-api-mags25 ping 10.133.71.100
   ```

2. **DNS Opløsning**: Tjek om DNS virker i containeren
   ```bash
   docker exec -it h2-api-mags25 nslookup 10.133.71.100
   ```

3. **Port Adgang**: Test om port 389 er tilgængelig
   ```bash
   docker exec -it h2-api-mags25 telnet 10.133.71.100 389
   ```

4. **Logs**: Tjek container logs for detaljerede fejlmeddelelser
   ```bash
   docker logs h2-api-mags25
   ```

## Fejlkoder
- **Error 81**: "Server Down" - AD serveren er ikke tilgængelig
- **Error 91**: "Connect Error" - Netværksforbindelse fejler
- **Error 85**: "Timeout" - Forbindelse timeout

## Yderligere Løsninger
Hvis problemet fortsætter, overvej:

1. **Firewall**: Sikr at port 389 er åben på AD serveren
2. **Docker Network**: Brug `--network host` hvis nødvendigt
3. **SSL/TLS**: Prøv at deaktivere SSL hvis det ikke er nødvendigt
4. **Alternative Ports**: Prøv port 636 med SSL hvis port 389 fejler
