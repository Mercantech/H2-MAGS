using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Ticket entity baseret på ITIL 4 principper
    /// Repræsenterer en service request eller incident i hotel systemet
    /// </summary>
    public class Ticket : Common
    {
        // ITIL 4 Core Concepts
        public string TicketNumber { get; set; } = string.Empty; // Unikt ticket nummer (f.eks. TKT-2025-001)
        public string Title { get; set; } = string.Empty; // Kort beskrivelse af ticket
        public string Description { get; set; } = string.Empty; // Detaljeret beskrivelse
        
        // Service Value Chain - Hvem er involveret
        public string RequesterId { get; set; } = string.Empty; // Den der anmoder om servicen (booking ejer)
        public User? Requester { get; set; }
        
        public string? AssigneeId { get; set; } // Den der håndterer ticket (rengøring, reception, etc.)
        public User? Assignee { get; set; }
        
        // Service Management - Hvilken service
        public string ServiceType { get; set; } = string.Empty; // Cleaning, RoomService, Maintenance, General
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed, Cancelled
        
        // ITIL 4 Dimensions
        public string Category { get; set; } = string.Empty; // Incident, Service Request, Problem, Change
        public string SubCategory { get; set; } = string.Empty; // Mere specifik kategorisering
        
        // Service Level Management (SLA)
        public DateTime? DueDate { get; set; } // SLA deadline
        public DateTime? ResolvedAt { get; set; } // Når ticket blev løst
        public DateTime? ClosedAt { get; set; } // Når ticket blev lukket
        
        // Configuration Management - Hvilken booking/room
        public string? BookingId { get; set; } // Relateret booking
        public Booking? Booking { get; set; }
        
        public string? RoomId { get; set; } // Relateret rum
        public Room? Room { get; set; }
        
        public string? HotelId { get; set; } // Relateret hotel
        public Hotel? Hotel { get; set; }
        
        // Change Control - Tracking af ændringer
        public string? Resolution { get; set; } // Hvordan ticket blev løst
        public string? WorkNotes { get; set; } // Interne noter fra håndterer
        
        // Risk Management
        public string? RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
        public string? Impact { get; set; } = "Low"; // Low, Medium, High, Critical
        
        // Navigation properties
        public List<TicketComment> Comments { get; set; } = new();
        public List<TicketAttachment> Attachments { get; set; } = new();
        public List<TicketHistory> History { get; set; } = new();
    }

    /// <summary>
    /// Ticket kommentarer for kommunikation mellem requester og assignee
    /// </summary>
    public class TicketComment : Common
    {
        public string TicketId { get; set; } = string.Empty;
        public Ticket? Ticket { get; set; }
        
        public string AuthorId { get; set; } = string.Empty;
        public User? Author { get; set; }
        
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false; // Kun synlig for staff
    }

    /// <summary>
    /// Ticket vedhæftninger (billeder, dokumenter, etc.)
    /// </summary>
    public class TicketAttachment : Common
    {
        public string TicketId { get; set; } = string.Empty;
        public Ticket? Ticket { get; set; }
        
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        
        public string UploadedById { get; set; } = string.Empty;
        public User? UploadedBy { get; set; }
    }

    /// <summary>
    /// Ticket historik for change control og auditing
    /// </summary>
    public class TicketHistory : Common
    {
        public string TicketId { get; set; } = string.Empty;
        public Ticket? Ticket { get; set; }
        
        public string FieldName { get; set; } = string.Empty; // Hvilket felt der blev ændret
        public string? OldValue { get; set; } // Gammel værdi
        public string? NewValue { get; set; } // Ny værdi
        
        public string ChangedById { get; set; } = string.Empty;
        public User? ChangedBy { get; set; }
        
        public string ChangeReason { get; set; } = string.Empty; // Hvorfor ændringen blev lavet
    }

    // DTOs for API endpoints

    /// <summary>
    /// DTO for oprettelse af nyt ticket
    /// </summary>
    public class TicketCreateDto
    {
        [Required(ErrorMessage = "Titel er påkrævet")]
        [StringLength(200, ErrorMessage = "Titel må maksimalt være 200 tegn")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Beskrivelse er påkrævet")]
        [StringLength(2000, ErrorMessage = "Beskrivelse må maksimalt være 2000 tegn")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service type er påkrævet")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori er påkrævet")]
        public string Category { get; set; } = string.Empty;

        public string? SubCategory { get; set; }

        [Required(ErrorMessage = "Prioritet er påkrævet")]
        public string Priority { get; set; } = "Medium";

        public string? BookingId { get; set; }
        public string? RoomId { get; set; }
        public string? HotelId { get; set; }
    }

    /// <summary>
    /// DTO for opdatering af ticket
    /// </summary>
    public class TicketUpdateDto
    {
        [Required(ErrorMessage = "Ticket ID er påkrævet")]
        public string Id { get; set; } = string.Empty;

        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? AssigneeId { get; set; }
        public string? Resolution { get; set; }
        public string? WorkNotes { get; set; }
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// DTO for hentning af ticket information
    /// </summary>
    public class TicketGetDto
    {
        public string Id { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        
        // Requester information
        public string RequesterId { get; set; } = string.Empty;
        public string? RequesterEmail { get; set; }
        public string? RequesterUsername { get; set; }
        
        // Assignee information
        public string? AssigneeId { get; set; }
        public string? AssigneeEmail { get; set; }
        public string? AssigneeUsername { get; set; }
        
        // Related entities
        public string? BookingId { get; set; }
        public string? RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? HotelId { get; set; }
        public string? HotelName { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        
        // Resolution
        public string? Resolution { get; set; }
        public string? WorkNotes { get; set; }
        
        // Comments count
        public int CommentsCount { get; set; }
        public int AttachmentsCount { get; set; }
    }

    /// <summary>
    /// DTO for ticket kommentarer
    /// </summary>
    public class TicketCommentDto
    {
        public string Id { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string? AuthorEmail { get; set; }
        public string? AuthorUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for oprettelse af ticket kommentar
    /// </summary>
    public class TicketCommentCreateDto
    {
        [Required(ErrorMessage = "Ticket ID er påkrævet")]
        public string TicketId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kommentar er påkrævet")]
        [StringLength(1000, ErrorMessage = "Kommentar må maksimalt være 1000 tegn")]
        public string Comment { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false;
    }

    /// <summary>
    /// DTO for ticket søgning og filtrering
    /// </summary>
    public class TicketSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? ServiceType { get; set; }
        public string? Category { get; set; }
        public string? RequesterId { get; set; }
        public string? AssigneeId { get; set; }
        public string? BookingId { get; set; }
        public string? RoomId { get; set; }
        public string? HotelId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? DueFrom { get; set; }
        public DateTime? DueTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
    }
}
