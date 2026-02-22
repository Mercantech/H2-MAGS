using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Hubs
{
    /// <summary>
    /// SignalR Hub for live ticket chat kommunikation
    /// Implementerer real-time messaging mellem brugere og support medarbejdere
    /// </summary>
    public class TicketHub : Hub
    {
        private readonly ILogger<TicketHub> _logger;

        public TicketHub(ILogger<TicketHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Tilslut til en specifik ticket chat
        /// </summary>
        /// <param name="ticketId">ID for ticket at tilslutte til</param>
        public async Task JoinTicketGroup(string ticketId)
        {
            // For demo mode, use connection ID as user ID
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Demo User";

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "Bruger ikke fundet");
                return;
            }

            // Valider adgang til ticket
            if (!await ValidateTicketAccess(ticketId, userId, userRole))
            {
                await Clients.Caller.SendAsync("Error", "Du har ikke adgang til denne ticket");
                return;
            }

            // Tilslut til ticket gruppe
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
            
            _logger.LogInformation("Bruger {Username} ({UserId}) tilsluttede til ticket {TicketId}", username, userId, ticketId);
            
            // Notificer andre i gruppen om ny bruger
            await Clients.Group($"Ticket_{ticketId}").SendAsync("UserJoined", new
            {
                Username = username,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });

            // Send bekræftelse til brugeren
            await Clients.Caller.SendAsync("JoinedTicket", new
            {
                TicketId = ticketId,
                Message = $"Du er nu tilsluttet til ticket {ticketId}"
            });
        }

        /// <summary>
        /// Forlad en ticket chat
        /// </summary>
        /// <param name="ticketId">ID for ticket at forlade</param>
        public async Task LeaveTicketGroup(string ticketId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Demo User";

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
            
            _logger.LogInformation("Bruger {Username} ({UserId}) forlod ticket {TicketId}", username, userId, ticketId);
            
            // Notificer andre i gruppen om bruger forlod
            await Clients.Group($"Ticket_{ticketId}").SendAsync("UserLeft", new
            {
                Username = username,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Send besked til ticket chat
        /// </summary>
        /// <param name="ticketId">ID for ticket</param>
        /// <param name="message">Besked at sende</param>
        /// <param name="isInternal">Om beskeden er intern (kun for staff)</param>
        public async Task SendMessage(string ticketId, string message, bool isInternal = false)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Demo User";
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "Bruger ikke fundet");
                return;
            }

            // Valider adgang til ticket
            if (!await ValidateTicketAccess(ticketId, userId, userRole))
            {
                await Clients.Caller.SendAsync("Error", "Du har ikke adgang til denne ticket");
                return;
            }

            // Valider at brugeren kan sende interne beskeder
            if (isInternal && !IsStaff(userRole))
            {
                await Clients.Caller.SendAsync("Error", "Du har ikke adgang til at sende interne beskeder");
                return;
            }

            var messageData = new
            {
                Id = Guid.NewGuid().ToString(),
                TicketId = ticketId,
                Message = message,
                AuthorId = userId,
                AuthorName = username,
                IsInternal = isInternal,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Besked sendt til ticket {TicketId} af {Username}: {Message}", ticketId, username, message);

            // Send besked til alle i ticket gruppen
            await Clients.Group($"Ticket_{ticketId}").SendAsync("MessageReceived", messageData);
        }

        /// <summary>
        /// Send typing indikator
        /// </summary>
        /// <param name="ticketId">ID for ticket</param>
        /// <param name="isTyping">Om brugeren skriver</param>
        public async Task SendTypingIndicator(string ticketId, bool isTyping)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Demo User";

            if (string.IsNullOrEmpty(userId))
                return;

            // Send typing indikator til andre i gruppen (ikke til sig selv)
            await Clients.OthersInGroup($"Ticket_{ticketId}").SendAsync("TypingIndicator", new
            {
                UserId = userId,
                Username = username,
                IsTyping = isTyping,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Send ticket status opdatering
        /// </summary>
        /// <param name="ticketId">ID for ticket</param>
        /// <param name="status">Ny status</param>
        /// <param name="message">Besked om ændringen</param>
        public async Task SendStatusUpdate(string ticketId, string status, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Demo User";

            if (string.IsNullOrEmpty(userId))
                return;

            var statusUpdate = new
            {
                TicketId = ticketId,
                Status = status,
                Message = message,
                UpdatedBy = username,
                UpdatedById = userId,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Status opdatering sendt for ticket {TicketId}: {Status} - {Message}", ticketId, status, message);

            // Send status opdatering til alle i ticket gruppen
            await Clients.Group($"Ticket_{ticketId}").SendAsync("StatusUpdated", statusUpdate);
        }

        /// <summary>
        /// Send ticket tildeling notifikation
        /// </summary>
        /// <param name="ticketId">ID for ticket</param>
        /// <param name="assigneeId">ID for tildelt medarbejder</param>
        /// <param name="assigneeName">Navn på tildelt medarbejder</param>
        public async Task SendAssignmentNotification(string ticketId, string assigneeId, string assigneeName)
        {
            var assignmentNotification = new
            {
                TicketId = ticketId,
                AssigneeId = assigneeId,
                AssigneeName = assigneeName,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Tildeling notifikation sendt for ticket {TicketId} til {AssigneeName}", ticketId, assigneeName);

            // Send tildeling notifikation til alle i ticket gruppen
            await Clients.Group($"Ticket_{ticketId}").SendAsync("TicketAssigned", assignmentNotification);
        }

        /// <summary>
        /// Send ticket lukning notifikation
        /// </summary>
        /// <param name="ticketId">ID for ticket</param>
        /// <param name="resolution">Løsning</param>
        /// <param name="closedBy">Hvem lukkede ticket</param>
        public async Task SendTicketClosedNotification(string ticketId, string resolution, string closedBy)
        {
            var closeNotification = new
            {
                TicketId = ticketId,
                Resolution = resolution,
                ClosedBy = closedBy,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Ticket lukning notifikation sendt for ticket {TicketId}", ticketId);

            // Send lukning notifikation til alle i ticket gruppen
            await Clients.Group($"Ticket_{ticketId}").SendAsync("TicketClosed", closeNotification);
        }

        /// <summary>
        /// Håndter når en klient tilslutter
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Demo User";
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            _logger.LogInformation("Bruger {Username} ({UserId}) med rolle {Role} tilsluttede SignalR", username, userId, userRole);

            // Send velkomst besked
            await Clients.Caller.SendAsync("Connected", new
            {
                Message = "Du er nu tilsluttet til ticket chat systemet",
                UserId = userId,
                Username = username,
                Role = userRole,
                Timestamp = DateTime.UtcNow
            });

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Håndter når en klient afbryder forbindelsen
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Demo User";

            _logger.LogInformation("Bruger {Username} ({UserId}) afbrød SignalR forbindelse", username, userId);

            if (exception != null)
            {
                _logger.LogError(exception, "Fejl ved afbrydelse af SignalR forbindelse for bruger {Username}", username);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Private helper methods

        /// <summary>
        /// Valider om brugeren har adgang til ticket
        /// </summary>
        private async Task<bool> ValidateTicketAccess(string ticketId, string userId, string? userRole)
        {
            // Dette skulle normalt tjekke mod database
            // For nu returnerer vi true for alle autentificerede brugere
            // I en rigtig implementation skulle vi tjekke:
            // - Er brugeren requester af ticket?
            // - Er brugeren assignee af ticket?
            // - Er brugeren staff med adgang til ticket type?
            // - Er brugeren admin?
            
            return !string.IsNullOrEmpty(userId);
        }

        /// <summary>
        /// Tjek om brugeren er staff
        /// </summary>
        private bool IsStaff(string? userRole)
        {
            return userRole == "Admin" || userRole == "Reception" || userRole == "CleaningStaff";
        }
    }
}
