---
name: Uge 6 - Email Service Testing
about: Test og valider Email Service funktionalitet med Bruno
title: 'Uge 6 - Email Service Testing'
labels: ['uge-6', 'testing', 'email', 'bruno', 'smtp']
assignees: ''
---

## Email Service Testing og Validering

- [ ] Opret Bruno test collection for Email Service
  - [ ] Test velkommen email ved brugeroprettelse
  - [ ] Test booking bekræftelse email ved booking oprettelse
  - [ ] Test email konfiguration og SMTP forbindelse
  - [ ] Test fejlhåndtering ved email sending
  - [ ] Test email template generation
- [ ] Konfigurer email test miljø
  - [ ] Opret test Gmail konto til testing
  - [ ] Konfigurer test email adresser
  - [ ] Sæt op test data for email templates
  - [ ] Valider email levering og modtagelse
- [ ] Implementér email test resultat visning
  - [ ] Opret API endpoint til email test status
  - [ ] Vis email test resultater i Blazor komponent
  - [ ] Implementér email test historik
- [ ] Dokumentér email test strategi og resultater

---

### Eksempel på Email Service test struktur
```javascript
// Test for velkommen email ved brugeroprettelse
test("Should send welcome email on user registration", function() {
  expect(res.getStatus()).to.equal(200);
  expect(res.getBody().emailSent).to.equal(true);
  expect(res.getBody().message).to.include("Bruger oprettet");
});

// Test for booking bekræftelse email
test("Should send booking confirmation email", function() {
  expect(res.getStatus()).to.equal(201);
  expect(res.getBody().emailSent).to.equal(true);
  expect(res.getBody().bookingId).to.exist;
});

// Test for email konfiguration
test("Should test SMTP connection", function() {
  expect(res.getStatus()).to.equal(200);
  expect(res.getBody().smtpWorking).to.equal(true);
  expect(res.getBody().emailSent).to.equal(true);
});
```

### Eksempel på email test API endpoints
```csharp
[HttpGet("email-status")]
public ActionResult<EmailServiceStatus> GetEmailServiceStatus()
{
    var status = new EmailServiceStatus
    {
        SmtpConfigured = !string.IsNullOrEmpty(_configuration["MailSettings:SmtpUsername"]),
        LastTestRun = _lastEmailTest,
        TestResults = _emailTestResults,
        IsHealthy = _emailService.TestSmtpConnectionAsync().Result
    };
    
    return Ok(status);
}

[HttpPost("test-email-template")]
public async Task<ActionResult> TestEmailTemplate([FromBody] EmailTemplateTestRequest request)
{
    var htmlContent = _mailService.CreateTestEmailTemplate(
        request.TemplateType, 
        request.TestData
    );
    
    return Ok(new { htmlContent, templateType = request.TemplateType });
}
```

### Email test data eksempler
```json
{
  "welcomeEmailTest": {
    "email": "test@example.com",
    "username": "TestUser",
    "role": "User"
  },
  "bookingConfirmationTest": {
    "email": "test@example.com",
    "username": "TestUser",
    "roomNumber": "101",
    "hotelName": "Test Hotel",
    "startDate": "2025-02-01",
    "endDate": "2025-02-03",
    "numberOfGuests": 2,
    "totalPrice": 1500.00,
    "bookingId": "test-booking-123"
  }
}
```

### Docker Compose email test eksempel
```yaml
email-tests:
  image: node:20-alpine
  container_name: h2-email-tests
  working_dir: /app
  volumes:
    - ./Bruno:/app/Bruno
    - ./test-results:/app/test-results
  environment:
    - TEST_EMAIL=test@example.com
    - SMTP_USERNAME=${SMTP_USERNAME}
    - SMTP_PASSWORD=${SMTP_PASSWORD}
  command: >
    sh -c "
      npm install -g @usebruno/cli &&
      sleep 30 &&
      cd Bruno/Email-Service &&
      npx @usebruno/cli run --env H2-MAGS-Email --output /app/test-results/email-test-report-$$(date +%Y-%m-%d_%H-%M-%S).html --format html
    "
  depends_on:
    - api
```

### Krav til email test coverage
- [ ] Test alle email typer (velkommen, booking bekræftelse)
- [ ] Test SMTP konfiguration og forbindelse
- [ ] Test email template generation
- [ ] Test fejlhåndtering ved email fejl
- [ ] Test email levering og modtagelse
- [ ] Valider HTML email formatting
- [ ] Test email personalisering

### Email test scenarier
- [ ] **Succesfuld email sending** - Email sendes og modtages korrekt
- [ ] **SMTP konfiguration fejl** - Håndtering af manglende credentials
- [ ] **Email template fejl** - Håndtering af template generation fejl
- [ ] **Network fejl** - Håndtering af SMTP forbindelse fejl
- [ ] **Ugyldig email adresse** - Håndtering af invalid email format
- [ ] **Rate limiting** - Test af Gmail rate limits
- [ ] **Concurrent email sending** - Test af samtidig email sending

### Email test validering
- [ ] **Email indhold** - Verificer at email indeholder korrekt information
- [ ] **HTML formatting** - Valider at HTML emails vises korrekt
- [ ] **Email headers** - Tjek subject, from, to felter
- [ ] **Personalization** - Verificer at brugerdata indsættes korrekt
- [ ] **Email delivery** - Bekræft at emails faktisk leveres
- [ ] **Error logging** - Verificer at fejl logges korrekt

### Bonus opgaver
- [ ] Implementér email test automation i CI/CD
- [ ] Opret email template preview i Blazor
- [ ] Implementér email test data generation
- [ ] Opret email performance testing
- [ ] Implementér email test notifikationer
- [ ] Opret email test dashboard med metrics

### Test resultat eksempler
```json
{
  "emailTestResults": {
    "totalTests": 15,
    "passedTests": 14,
    "failedTests": 1,
    "successRate": "93.3%",
    "testDuration": "2.5s",
    "emailTypes": {
      "welcomeEmail": {
        "tested": true,
        "passed": true,
        "deliveryTime": "1.2s"
      },
      "bookingConfirmation": {
        "tested": true,
        "passed": true,
        "deliveryTime": "1.8s"
      }
    },
    "smtpStatus": {
      "configured": true,
      "connectionTest": "passed",
      "lastTest": "2025-01-15T10:30:00Z"
    }
  }
}
```

### Email test best practices
- [ ] Brug test email adresser (ikke produktion emails)
- [ ] Test med forskellige email providers
- [ ] Valider email indhold i forskellige email klienter
- [ ] Test email templates på forskellige enheder
- [ ] Dokumenter email test procedurer
- [ ] Opret email test data cleanup scripts
