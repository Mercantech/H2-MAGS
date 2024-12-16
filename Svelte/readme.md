# Svelte

Svelte er en moderne frontend-rammeværk, der gør det nemt at bygge interaktive brugergrænseflader. Her er der lavet et alternativ til BlazorWASM, som er bygget op med Svelte. Der er fokus på admin siden!

## Kom i gang

### Forudsætninger
- Node.js (version 16 eller nyere)
- npm (følger med Node.js)

### Installation af projektet

1. Opret et nyt Svelte projekt med Vite:
```bash
npm create vite@latest Hotel -- --template svelte
cd Hotel
```

2. Installer afhængigheder:
```bash
npm install
```

### Udvikling

Start udviklingsserveren:
```bash
npm run dev
```

For at få adgang fra andre enheder på netværket:
```bash
npm run dev -- --host
```

### Build til produktion

Byg projektet til produktion:
```bash
npm run build
```

### Preview af produktionsbuild

For at se hvordan produktionsbygget ser ud:
```bash
npm run preview
```

## Projektstruktur

```
mit-projekt/
├── src/
│   ├── lib/
│   ├── routes/
│   ├── App.svelte
│   └── main.js
├── public/
├── index.html
└── vite.config.js
```

## Vigtige filer
- `src/App.svelte`: Hovedkomponenten
- `src/main.js`: Indgangspunkt for applikationen
- `vite.config.js`: Konfiguration for Vite

