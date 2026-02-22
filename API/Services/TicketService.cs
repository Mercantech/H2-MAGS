using Microsoft.EntityFrameworkCore;
using API.Data;
using DomainModels;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using API.Hubs;

namespace API.Services
{
    /// <summary>
    /// Service for ticket management baseret på ITIL 4 principper
    /// Implementerer Service Value Chain, Change Control, Service Level Management og Configuration Management
    /// </summary>
    public class TicketService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<TicketService> _logger;
        private readonly IHubContext<TicketHub> _hubContext;

        public TicketService(AppDBContext context, ILogger<TicketService> logger, IHubContext<TicketHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Hent tickets med filtrering og paginering
        /// Implementerer Service Level Management for SLA tracking
        /// </summary>
        public async Task<IEnumerable<TicketGetDto>> GetTicketsAsync(TicketSearchDto searchDto, string userId, string? userRole)
        {
            var query = _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.Assignee)
                .Include(t => t.Booking)
                .Include(t => t.Room)
                .Include(t => t.Hotel)
                .AsQueryable();

            // ITIL 4 Access Control - Kun relevante tickets for brugeren
            if (userRole == "User")
            {
                query = query.Where(t => t.RequesterId == userId);
            }
            else if (userRole == "CleaningStaff")
            {
                query = query.Where(t => t.AssigneeId == userId || t.ServiceType == "Cleaning" || t.Status == "Open");
            }
            else if (userRole == "Reception")
            {
                query = query.Where(t => t.AssigneeId == userId || t.ServiceType == "RoomService" || t.Status == "Open");
            }
            // Admin kan se alle tickets

            // Filtrering baseret på søgekriterier
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                query = query.Where(t => t.Title.Contains(searchDto.SearchTerm) || 
                                       t.Description.Contains(searchDto.SearchTerm) ||
                                       t.TicketNumber.Contains(searchDto.SearchTerm));
            }

            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                query = query.Where(t => t.Status == searchDto.Status);
            }

            if (!string.IsNullOrEmpty(searchDto.Priority))
            {
                query = query.Where(t => t.Priority == searchDto.Priority);
            }

            if (!string.IsNullOrEmpty(searchDto.ServiceType))
            {
                query = query.Where(t => t.ServiceType == searchDto.ServiceType);
            }

            if (!string.IsNullOrEmpty(searchDto.Category))
            {
                query = query.Where(t => t.Category == searchDto.Category);
            }

            if (!string.IsNullOrEmpty(searchDto.RequesterId))
            {
                query = query.Where(t => t.RequesterId == searchDto.RequesterId);
            }

            if (!string.IsNullOrEmpty(searchDto.AssigneeId))
            {
                query = query.Where(t => t.AssigneeId == searchDto.AssigneeId);
            }

            if (!string.IsNullOrEmpty(searchDto.BookingId))
            {
                query = query.Where(t => t.BookingId == searchDto.BookingId);
            }

            if (!string.IsNullOrEmpty(searchDto.RoomId))
            {
                query = query.Where(t => t.RoomId == searchDto.RoomId);
            }

            if (!string.IsNullOrEmpty(searchDto.HotelId))
            {
                query = query.Where(t => t.HotelId == searchDto.HotelId);
            }

            if (searchDto.CreatedFrom.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= searchDto.CreatedFrom.Value);
            }

            if (searchDto.CreatedTo.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= searchDto.CreatedTo.Value);
            }

            if (searchDto.DueFrom.HasValue)
            {
                query = query.Where(t => t.DueDate >= searchDto.DueFrom.Value);
            }

            if (searchDto.DueTo.HasValue)
            {
                query = query.Where(t => t.DueDate <= searchDto.DueTo.Value);
            }

            // Sortering
            query = searchDto.SortBy.ToLower() switch
            {
                "priority" => searchDto.SortDirection == "asc" ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
                "status" => searchDto.SortDirection == "asc" ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status),
                "duedate" => searchDto.SortDirection == "asc" ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate),
                "title" => searchDto.SortDirection == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
                _ => searchDto.SortDirection == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
            };

            // Paginering
            var tickets = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(t => new TicketGetDto
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Description = t.Description,
                    ServiceType = t.ServiceType,
                    Priority = t.Priority,
                    Status = t.Status,
                    Category = t.Category,
                    SubCategory = t.SubCategory,
                    RiskLevel = t.RiskLevel,
                    Impact = t.Impact,
                    RequesterId = t.RequesterId,
                    RequesterEmail = t.Requester!.Email,
                    RequesterUsername = t.Requester.Username,
                    AssigneeId = t.AssigneeId,
                    AssigneeEmail = t.Assignee != null ? t.Assignee.Email : null,
                    AssigneeUsername = t.Assignee != null ? t.Assignee.Username : null,
                    BookingId = t.BookingId,
                    RoomId = t.RoomId,
                    RoomNumber = t.Room != null ? t.Room.Number : null,
                    HotelId = t.HotelId,
                    HotelName = t.Hotel != null ? t.Hotel.Name : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    DueDate = t.DueDate,
                    ResolvedAt = t.ResolvedAt,
                    ClosedAt = t.ClosedAt,
                    Resolution = t.Resolution,
                    WorkNotes = t.WorkNotes,
                    CommentsCount = t.Comments.Count,
                    AttachmentsCount = t.Attachments.Count
                })
                .ToListAsync();

            return tickets;
        }

        /// <summary>
        /// Hent specifik ticket
        /// </summary>
        public async Task<TicketGetDto?> GetTicketByIdAsync(string id, string userId, string? userRole)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.Assignee)
                .Include(t => t.Booking)
                .Include(t => t.Room)
                .Include(t => t.Hotel)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return null;

            // ITIL 4 Access Control
            if (userRole == "User" && ticket.RequesterId != userId)
                return null;

            if (userRole == "CleaningStaff" && ticket.AssigneeId != userId && ticket.ServiceType != "Cleaning")
                return null;

            if (userRole == "Reception" && ticket.AssigneeId != userId && ticket.ServiceType != "RoomService")
                return null;

            return new TicketGetDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                ServiceType = ticket.ServiceType,
                Priority = ticket.Priority,
                Status = ticket.Status,
                Category = ticket.Category,
                SubCategory = ticket.SubCategory,
                RiskLevel = ticket.RiskLevel,
                Impact = ticket.Impact,
                RequesterId = ticket.RequesterId,
                RequesterEmail = ticket.Requester!.Email,
                RequesterUsername = ticket.Requester.Username,
                AssigneeId = ticket.AssigneeId,
                AssigneeEmail = ticket.Assignee != null ? ticket.Assignee.Email : null,
                AssigneeUsername = ticket.Assignee != null ? ticket.Assignee.Username : null,
                BookingId = ticket.BookingId,
                RoomId = ticket.RoomId,
                RoomNumber = ticket.Room != null ? ticket.Room.Number : null,
                HotelId = ticket.HotelId,
                HotelName = ticket.Hotel != null ? ticket.Hotel.Name : null,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                DueDate = ticket.DueDate,
                ResolvedAt = ticket.ResolvedAt,
                ClosedAt = ticket.ClosedAt,
                Resolution = ticket.Resolution,
                WorkNotes = ticket.WorkNotes,
                CommentsCount = ticket.Comments.Count,
                AttachmentsCount = ticket.Attachments.Count
            };
        }

        /// <summary>
        /// Opret nyt ticket
        /// Implementerer Service Value Chain og Configuration Management
        /// </summary>
        public async Task<TicketGetDto> CreateTicketAsync(TicketCreateDto createDto, string userId)
        {
            // Generer unikt ticket nummer
            var ticketNumber = await GenerateTicketNumberAsync();

            // Beregn SLA baseret på prioritet (Service Level Management)
            var dueDate = CalculateSLA(createDto.Priority);

            // Valider booking adgang hvis relevant
            if (!string.IsNullOrEmpty(createDto.BookingId))
            {
                var booking = await _context.Bookings
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == createDto.BookingId);

                if (booking == null)
                    throw new ArgumentException("Booking ikke fundet");

                if (booking.UserId != userId)
                    throw new UnauthorizedAccessException("Du har ikke adgang til denne booking");
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString(),
                TicketNumber = ticketNumber,
                Title = createDto.Title,
                Description = createDto.Description,
                ServiceType = createDto.ServiceType,
                Priority = createDto.Priority,
                Status = "Open",
                Category = createDto.Category,
                SubCategory = createDto.SubCategory,
                RequesterId = userId,
                BookingId = createDto.BookingId,
                RoomId = createDto.RoomId,
                HotelId = createDto.HotelId,
                DueDate = dueDate,
                RiskLevel = CalculateRiskLevel(createDto.Priority, createDto.ServiceType),
                Impact = CalculateImpact(createDto.Priority, createDto.ServiceType),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);

            // Tilføj til historik (Change Control)
            await AddTicketHistoryAsync(ticket.Id, "Status", null, "Open", userId, "Ticket oprettet");

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket oprettet: {TicketNumber} af bruger {UserId}", ticketNumber, userId);

            // Send SignalR notifikation om nyt ticket
            await _hubContext.Clients.All.SendAsync("TicketCreated", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                ServiceType = ticket.ServiceType,
                Priority = ticket.Priority,
                RequesterId = ticket.RequesterId,
                CreatedAt = ticket.CreatedAt
            });

            return await GetTicketByIdAsync(ticket.Id, userId, "User");
        }

        /// <summary>
        /// Opdater ticket
        /// Implementerer Change Control og Service Level Management
        /// </summary>
        public async Task<TicketGetDto?> UpdateTicketAsync(TicketUpdateDto updateDto, string userId, string? userRole)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == updateDto.Id);

            if (ticket == null)
                return null;

            // ITIL 4 Access Control
            if (userRole == "User" && ticket.RequesterId != userId)
                throw new UnauthorizedAccessException("Du har ikke adgang til at opdatere dette ticket");

            if (userRole == "CleaningStaff" && ticket.AssigneeId != userId && ticket.ServiceType != "Cleaning")
                throw new UnauthorizedAccessException("Du har ikke adgang til at opdatere dette ticket");

            if (userRole == "Reception" && ticket.AssigneeId != userId && ticket.ServiceType != "RoomService")
                throw new UnauthorizedAccessException("Du har ikke adgang til at opdatere dette ticket");

            // Change Control - Track ændringer
            if (!string.IsNullOrEmpty(updateDto.Title) && updateDto.Title != ticket.Title)
            {
                await AddTicketHistoryAsync(ticket.Id, "Title", ticket.Title, updateDto.Title, userId, "Titel opdateret");
                ticket.Title = updateDto.Title;
            }

            if (!string.IsNullOrEmpty(updateDto.Description) && updateDto.Description != ticket.Description)
            {
                await AddTicketHistoryAsync(ticket.Id, "Description", ticket.Description, updateDto.Description, userId, "Beskrivelse opdateret");
                ticket.Description = updateDto.Description;
            }

            if (!string.IsNullOrEmpty(updateDto.Status) && updateDto.Status != ticket.Status)
            {
                await AddTicketHistoryAsync(ticket.Id, "Status", ticket.Status, updateDto.Status, userId, "Status opdateret");
                ticket.Status = updateDto.Status;

                if (updateDto.Status == "Resolved")
                {
                    ticket.ResolvedAt = DateTime.UtcNow;
                }
                else if (updateDto.Status == "Closed")
                {
                    ticket.ClosedAt = DateTime.UtcNow;
                }
            }

            if (!string.IsNullOrEmpty(updateDto.Priority) && updateDto.Priority != ticket.Priority)
            {
                await AddTicketHistoryAsync(ticket.Id, "Priority", ticket.Priority, updateDto.Priority, userId, "Prioritet opdateret");
                ticket.Priority = updateDto.Priority;
                ticket.DueDate = CalculateSLA(updateDto.Priority); // Opdater SLA
            }

            if (!string.IsNullOrEmpty(updateDto.AssigneeId) && updateDto.AssigneeId != ticket.AssigneeId)
            {
                await AddTicketHistoryAsync(ticket.Id, "Assignee", ticket.AssigneeId, updateDto.AssigneeId, userId, "Ticket tildelt");
                ticket.AssigneeId = updateDto.AssigneeId;
            }

            if (!string.IsNullOrEmpty(updateDto.Resolution))
            {
                await AddTicketHistoryAsync(ticket.Id, "Resolution", ticket.Resolution, updateDto.Resolution, userId, "Løsning tilføjet");
                ticket.Resolution = updateDto.Resolution;
            }

            if (!string.IsNullOrEmpty(updateDto.WorkNotes))
            {
                await AddTicketHistoryAsync(ticket.Id, "WorkNotes", ticket.WorkNotes, updateDto.WorkNotes, userId, "Arbejdsnoter opdateret");
                ticket.WorkNotes = updateDto.WorkNotes;
            }

            if (updateDto.DueDate.HasValue && updateDto.DueDate != ticket.DueDate)
            {
                await AddTicketHistoryAsync(ticket.Id, "DueDate", ticket.DueDate?.ToString(), updateDto.DueDate?.ToString(), userId, "SLA deadline opdateret");
                ticket.DueDate = updateDto.DueDate;
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket opdateret: {TicketId} af bruger {UserId}", ticket.Id, userId);

            // Send SignalR notifikation om ticket opdatering
            await _hubContext.Clients.Group($"Ticket_{ticket.Id}").SendAsync("TicketUpdated", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Status = ticket.Status,
                Priority = ticket.Priority,
                UpdatedAt = ticket.UpdatedAt,
                UpdatedBy = userId
            });

            return await GetTicketByIdAsync(ticket.Id, userId, userRole);
        }

        /// <summary>
        /// Slet ticket (kun admin)
        /// </summary>
        public async Task<bool> DeleteTicketAsync(string id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket slettet: {TicketId}", id);
            return true;
        }

        /// <summary>
        /// Tildel ticket til medarbejder
        /// </summary>
        public async Task<TicketGetDto?> AssignTicketAsync(string ticketId, string assigneeId, string userId, string? userRole)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return null;

            // Valider at assignee eksisterer
            var assignee = await _context.Users.FindAsync(assigneeId);
            if (assignee == null)
                throw new ArgumentException("Medarbejder ikke fundet");

            // ITIL 4 Access Control
            if (userRole != "Admin" && userRole != "Reception")
                throw new UnauthorizedAccessException("Du har ikke adgang til at tildele tickets");

            ticket.AssigneeId = assigneeId;
            ticket.Status = "InProgress";
            ticket.UpdatedAt = DateTime.UtcNow;

            await AddTicketHistoryAsync(ticket.Id, "Assignee", ticket.AssigneeId, assigneeId, userId, $"Ticket tildelt til {assignee.Username}");
            await AddTicketHistoryAsync(ticket.Id, "Status", "Open", "InProgress", userId, "Ticket tildelt og sat i gang");

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket tildelt: {TicketId} til {AssigneeId} af {UserId}", ticketId, assigneeId, userId);

            // Send SignalR notifikation om ticket tildeling
            await _hubContext.Clients.Group($"Ticket_{ticket.Id}").SendAsync("TicketAssigned", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                AssigneeId = assigneeId,
                AssigneeName = assignee.Username,
                AssignedBy = userId,
                AssignedAt = DateTime.UtcNow
            });

            return await GetTicketByIdAsync(ticket.Id, userId, userRole);
        }

        /// <summary>
        /// Luk ticket
        /// </summary>
        public async Task<TicketGetDto?> CloseTicketAsync(string ticketId, string resolution, string userId, string? userRole)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return null;

            // ITIL 4 Access Control
            if (userRole == "User" && ticket.RequesterId != userId)
                throw new UnauthorizedAccessException("Du har ikke adgang til at lukke dette ticket");

            if (userRole == "CleaningStaff" && ticket.AssigneeId != userId)
                throw new UnauthorizedAccessException("Du har ikke adgang til at lukke dette ticket");

            if (userRole == "Reception" && ticket.AssigneeId != userId)
                throw new UnauthorizedAccessException("Du har ikke adgang til at lukke dette ticket");

            ticket.Status = "Closed";
            ticket.Resolution = resolution;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await AddTicketHistoryAsync(ticket.Id, "Status", "Resolved", "Closed", userId, "Ticket lukket");
            await AddTicketHistoryAsync(ticket.Id, "Resolution", ticket.Resolution, resolution, userId, "Løsning tilføjet");

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket lukket: {TicketId} af {UserId}", ticketId, userId);

            // Send SignalR notifikation om ticket lukning
            await _hubContext.Clients.Group($"Ticket_{ticket.Id}").SendAsync("TicketClosed", new
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Resolution = resolution,
                ClosedBy = userId,
                ClosedAt = ticket.ClosedAt
            });

            return await GetTicketByIdAsync(ticket.Id, userId, userRole);
        }

        /// <summary>
        /// Hent ticket kommentarer
        /// </summary>
        public async Task<IEnumerable<TicketCommentDto>> GetTicketCommentsAsync(string ticketId, string userId, string? userRole)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return Enumerable.Empty<TicketCommentDto>();

            // ITIL 4 Access Control
            if (userRole == "User" && ticket.RequesterId != userId)
                return Enumerable.Empty<TicketCommentDto>();

            var comments = await _context.TicketComments
                .Include(c => c.Author)
                .Where(c => c.TicketId == ticketId)
                .Where(c => userRole == "User" ? !c.IsInternal : true) // Brugere kan ikke se interne kommentarer
                .OrderBy(c => c.CreatedAt)
                .Select(c => new TicketCommentDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    Comment = c.Comment,
                    IsInternal = c.IsInternal,
                    AuthorId = c.AuthorId,
                    AuthorEmail = c.Author!.Email,
                    AuthorUsername = c.Author.Username,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return comments;
        }

        /// <summary>
        /// Tilføj kommentar til ticket
        /// </summary>
        public async Task<TicketCommentDto?> AddTicketCommentAsync(TicketCommentCreateDto commentDto, string userId, string? userRole)
        {
            var ticket = await _context.Tickets.FindAsync(commentDto.TicketId);
            if (ticket == null)
                return null;

            // ITIL 4 Access Control
            if (userRole == "User" && ticket.RequesterId != userId)
                throw new UnauthorizedAccessException("Du har ikke adgang til at kommentere på dette ticket");

            var comment = new TicketComment
            {
                Id = Guid.NewGuid().ToString(),
                TicketId = commentDto.TicketId,
                AuthorId = userId,
                Comment = commentDto.Comment,
                IsInternal = commentDto.IsInternal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kommentar tilføjet til ticket: {TicketId} af {UserId}", commentDto.TicketId, userId);

            // Send SignalR notifikation om ny kommentar
            await _hubContext.Clients.Group($"Ticket_{commentDto.TicketId}").SendAsync("CommentAdded", new
            {
                CommentId = comment.Id,
                TicketId = comment.TicketId,
                Message = comment.Comment,
                AuthorId = comment.AuthorId,
                AuthorName = (await _context.Users.FindAsync(userId))!.Username,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt
            });

            return new TicketCommentDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                Comment = comment.Comment,
                IsInternal = comment.IsInternal,
                AuthorId = comment.AuthorId,
                AuthorEmail = (await _context.Users.FindAsync(userId))!.Email,
                AuthorUsername = (await _context.Users.FindAsync(userId))!.Username,
                CreatedAt = comment.CreatedAt
            };
        }

        /// <summary>
        /// Hent ticket statistikker
        /// </summary>
        public async Task<object> GetTicketStatisticsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Tickets.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.CreatedAt <= toDate.Value);

            var totalTickets = await query.CountAsync();
            var openTickets = await query.CountAsync(t => t.Status == "Open");
            var inProgressTickets = await query.CountAsync(t => t.Status == "InProgress");
            var resolvedTickets = await query.CountAsync(t => t.Status == "Resolved");
            var closedTickets = await query.CountAsync(t => t.Status == "Closed");

            var ticketsByPriority = await query
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            var ticketsByServiceType = await query
                .GroupBy(t => t.ServiceType)
                .Select(g => new { ServiceType = g.Key, Count = g.Count() })
                .ToListAsync();

            var ticketsByCategory = await query
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            var resolvedTicketsData = await query
                .Where(t => t.ResolvedAt.HasValue)
                .Select(t => new { Created = t.CreatedAt, Resolved = t.ResolvedAt!.Value })
                .ToListAsync();

            var avgResolutionTime = resolvedTicketsData.Any() 
                ? resolvedTicketsData.Average(t => (t.Resolved - t.Created).TotalDays)
                : 0;

            return new
            {
                TotalTickets = totalTickets,
                OpenTickets = openTickets,
                InProgressTickets = inProgressTickets,
                ResolvedTickets = resolvedTickets,
                ClosedTickets = closedTickets,
                TicketsByPriority = ticketsByPriority,
                TicketsByServiceType = ticketsByServiceType,
                TicketsByCategory = ticketsByCategory,
                AverageResolutionTimeDays = avgResolutionTime
            };
        }

        /// <summary>
        /// Valider booking adgang
        /// </summary>
        public async Task<bool> ValidateBookingAccessAsync(string bookingId, string userId)
        {
            // For demo mode, always allow access
            if (userId == "demo-user-123")
            {
                return true;
            }
            
            var booking = await _context.Bookings.FindAsync(bookingId);
            return booking != null && booking.UserId == userId;
        }

        // Private helper methods

        private async Task<string> GenerateTicketNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"TKT-{year}-";
            
            var lastTicket = await _context.Tickets
                .Where(t => t.TicketNumber.StartsWith(prefix))
                .OrderByDescending(t => t.TicketNumber)
                .FirstOrDefaultAsync();

            if (lastTicket == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastTicket.TicketNumber.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }

        private DateTime CalculateSLA(string priority)
        {
            return priority switch
            {
                "Critical" => DateTime.UtcNow.AddHours(2),
                "High" => DateTime.UtcNow.AddHours(8),
                "Medium" => DateTime.UtcNow.AddDays(1),
                "Low" => DateTime.UtcNow.AddDays(3),
                _ => DateTime.UtcNow.AddDays(1)
            };
        }

        private string CalculateRiskLevel(string priority, string serviceType)
        {
            if (priority == "Critical" || serviceType == "Maintenance")
                return "High";
            if (priority == "High")
                return "Medium";
            return "Low";
        }

        private string CalculateImpact(string priority, string serviceType)
        {
            if (priority == "Critical")
                return "Critical";
            if (priority == "High" || serviceType == "Maintenance")
                return "High";
            if (priority == "Medium")
                return "Medium";
            return "Low";
        }

        private Task AddTicketHistoryAsync(string ticketId, string fieldName, string? oldValue, string? newValue, string userId, string changeReason)
        {
            var history = new TicketHistory
            {
                Id = Guid.NewGuid().ToString(),
                TicketId = ticketId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedById = userId,
                ChangeReason = changeReason,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TicketHistories.Add(history);
            return Task.CompletedTask;
        }
    }
}
