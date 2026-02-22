# Gmail SMTP Setup Guide

Denne guide forklarer hvordan du opsætter Gmail SMTP til at sende emails fra H2-MAGS systemet.

## 🔧 Gmail Konfiguration

### 1. Opret Gmail App Password

1. **Gå til din Google Account indstillinger**
   - Besøg: https://myaccount.google.com/
   - Vælg "Security" i venstre menu

2. **Aktiver 2-Factor Authentication**
   - Gå til "2-Step Verification"
   - Følg instruktionerne for at aktivere 2FA

3. **Opret App Password**
   - Gå til "App passwords" (kun tilgængelig efter 2FA er aktiveret)
   - Vælg "Mail" som app type
   - Vælg "Other" som enhed og navngiv det "H2-MAGS API"
   - Google vil generere et 16-cifret App Password
   - **GEM DETTE PASSWORD - du kan ikke se det igen!**

### 2. Opdater appsettings.json

Erstat placeholder værdierne i `appsettings.json`:

```json
{
  "MailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "din-email@gmail.com",
    "SmtpPassword": "dit-16-cifrede-app-password",
    "FromEmail": "din-email@gmail.com",
    "FromName": "H2-MAGS System"
  }
}
```

### 3. Test Email Konfiguration

Efter du har opdateret konfigurationen, kan du teste det:

1. **Start API'en**
2. **Log ind som administrator**
3. **Kald test endpoint:**
   ```
   POST /api/users/test-email?testEmail=din-test-email@example.com
   ```

## 📧 Email Features

### Automatiske Emails

- **Velkommen Email**: Sendes automatisk når en ny bruger registreres
- **HTML Format**: Professionelle emails med styling
- **Fejlhåndtering**: Systemet fortsætter selvom email fejler

### HTML Template

Velkommen emails indeholder:
- Brugerens navn og rolle
- Oprettelsesdato
- Link til systemet
- Professionel styling

## 🔒 Sikkerhed

### Vigtige Sikkerhedsnoter

1. **Brug App Password**: Aldrig brug din normale Gmail adgangskode
2. **Gem Secrets Sikkert**: Overvej at bruge environment variables i produktion
3. **2FA Påkrævet**: Gmail kræver 2FA for App Passwords
4. **Rate Limiting**: Gmail har begrænsninger på antal emails per dag

### Environment Variables (Anbefalet til Produktion)

I stedet for at gemme secrets i appsettings.json, kan du bruge environment variables:

```bash
# Windows
set MailSettings__SmtpUsername=din-email@gmail.com
set MailSettings__SmtpPassword=dit-app-password
set MailSettings__FromEmail=din-email@gmail.com

# Linux/Mac
export MailSettings__SmtpUsername=din-email@gmail.com
export MailSettings__SmtpPassword=dit-app-password
export MailSettings__FromEmail=din-email@gmail.com
```

## 🐛 Troubleshooting

### Almindelige Problemer

1. **"Invalid credentials"**
   - Tjek at du bruger App Password, ikke normal adgangskode
   - Verificer at 2FA er aktiveret

2. **"Connection timeout"**
   - Tjek internet forbindelse
   - Verificer SMTP server og port (smtp.gmail.com:587)

3. **"Authentication failed"**
   - Tjek at App Password er korrekt
   - Verificer at Gmail kontoen er aktiv

4. **"Less secure app access"**
   - Dette er ikke længere tilgængeligt i Gmail
   - Brug App Password i stedet

### Debug Logging

Aktivér detaljeret logging for at se SMTP fejl:

```json
{
  "Logging": {
    "LogLevel": {
      "API.Services.MailService": "Debug"
    }
  }
}
```

## 📚 Yderligere Ressourcer

- [Gmail App Passwords Guide](https://support.google.com/accounts/answer/185833)
- [Gmail SMTP Settings](https://support.google.com/mail/answer/7126229)
- [ASP.NET Core Email Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/email)

## ✅ Checklist

- [ ] 2-Factor Authentication aktiveret på Gmail
- [ ] App Password oprettet
- [ ] appsettings.json opdateret med korrekte værdier
- [ ] Test email sendt succesfuldt
- [ ] Velkommen email fungerer ved brugeroprettelse

