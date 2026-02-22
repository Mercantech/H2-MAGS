## H2-2025

```mermaid
erDiagram
  Users {
    Id text PK
    Email text 
    Username text 
    HashedPassword text 
    Salt text 
    LastLogin timestamp_with_time_zone 
    PasswordBackdoor text 
    CreatedAt timestamp_with_time_zone 
    UpdatedAt timestamp_with_time_zone 
  }
```
