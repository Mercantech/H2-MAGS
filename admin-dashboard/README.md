# Receptionist Dashboard

Et moderne Svelte-baseret dashboard til receptionister i hotel systemet.

## Funktioner

### 🔐 Autentificering
- Active Directory integration
- JWT token baseret autentificering
- Automatisk token refresh
- Sikker logout funktionalitet

### 📊 Dashboard
- Real-time statistikker
- Seneste bookinger oversigt
- Værelses tilgængelighed
- Hurtige handlinger

### 📅 Booking Management
- Se alle bookinger
- Opret nye bookinger
- Rediger eksisterende bookinger
- Slet bookinger
- Søg og filtrer bookinger

### 🏨 Værelser
- Oversigt over alle værelser
- Værelses tilgængelighed
- Hotel information
- Kapacitet information

### 👥 Brugere
- Bruger oversigt
- Rolle administration
- AD gruppe information
- Bruger detaljer

### 🏢 Hoteller
- Hotel oversigt
- Hotel information
- Værelser per hotel

### ⚙️ Indstillinger
- Bruger profil
- AD status
- System information
- Notifikation indstillinger

## Teknologi Stack

- **Frontend**: SvelteKit + TypeScript
- **Styling**: Tailwind CSS
- **Icons**: Lucide Svelte
- **HTTP Client**: Axios
- **State Management**: Svelte Stores
- **Date Handling**: date-fns

## Udvikling

### Forudsætninger
- Node.js 18+
- npm eller yarn
- API server kørende på port 5000

### Installation
```bash
# Installer dependencies
npm install

# Start udviklingsserver
npm run dev

# Åbn http://localhost:5173
```

### Bygning
```bash
# Byg til produktion
npm run build

# Preview produktion build
npm run preview
```

## Projekt Struktur

```
src/
├── lib/
│   ├── components/          # Genbrugelige komponenter
│   │   ├── Sidebar.svelte
│   │   ├── Header.svelte
│   │   ├── StatsCard.svelte
│   │   ├── RecentBookings.svelte
│   │   └── RoomAvailability.svelte
│   ├── services/            # API services
│   │   └── api.ts
│   ├── stores/              # State management
│   │   ├── auth.ts
│   │   ├── bookings.ts
│   │   ├── rooms.ts
│   │   ├── users.ts
│   │   └── hotels.ts
│   └── utils/               # Utility funktioner
│       ├── date.ts
│       └── validation.ts
├── routes/
│   ├── admin/               # Admin routes
│   │   ├── login/           # Login side
│   │   ├── dashboard/       # Dashboard
│   │   ├── bookings/        # Booking management
│   │   ├── rooms/           # Værelser
│   │   ├── users/           # Brugere
│   │   ├── hotels/          # Hoteller
│   │   └── settings/        # Indstillinger
│   └── +layout.svelte       # Root layout
└── app.html                 # HTML template
```

## API Integration

Dashboardet integrerer med følgende API endpoints:

### Autentificering
- `POST /api/auth/ad-login` - AD login
- `GET /api/auth/ad-me` - Hent nuværende bruger
- `GET /api/auth/ad-status` - AD status

### Admin Endpoints
- `GET /api/admin/dashboard/stats` - Dashboard statistikker
- `GET /api/admin/dashboard/recent-bookings` - Seneste bookinger
- `GET /api/admin/dashboard/room-availability` - Værelses tilgængelighed
- `GET /api/admin/bookings/by-date` - Bookinger per dato
- `GET /api/admin/rooms/available` - Tilgængelige værelser

### Standard Endpoints
- `GET /api/bookings` - Alle bookinger
- `POST /api/bookings` - Opret booking
- `PUT /api/bookings/{id}` - Opdater booking
- `DELETE /api/bookings/{id}` - Slet booking
- `GET /api/rooms` - Alle værelser
- `GET /api/users` - Alle brugere
- `GET /api/hotels` - Alle hoteller

## Sikkerhed

- JWT token autentificering
- Automatisk token refresh
- Rate limiting på API kald
- Input validering
- XSS beskyttelse

## Responsive Design

Dashboardet er fuldt responsivt og fungerer på:
- Desktop (1024px+)
- Tablet (768px - 1023px)
- Mobile (320px - 767px)

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Udvikling

### Code Style
- ESLint for code linting
- Prettier for code formatting
- TypeScript for type safety

### Testing
```bash
# Kør tests
npm run test

# Kør tests i watch mode
npm run test:watch
```

### Linting
```bash
# Kør ESLint
npm run lint

# Fix ESLint fejl
npm run lint:fix

# Format kode
npm run format
```

## Deployment

### Produktion Build
```bash
npm run build
```

### Environment Variables
```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_NAME=Receptionist Dashboard
```

## Fejlrapportering

Hvis du støder på fejl eller problemer:

1. Tjek browser console for fejl
2. Tjek API server logs
3. Verificer at API server kører på port 5000
4. Tjek at alle dependencies er installeret

## Bidrag

1. Fork projektet
2. Opret feature branch
3. Commit ændringer
4. Push til branch
5. Opret Pull Request

## Licens

Dette projekt er en del af H2-Projekt systemet.