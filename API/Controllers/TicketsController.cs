using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Services;
using DomainModels;
using System.Security.Claims;

namespace API.Controllers
{
    /// <summary>
    /// Controller for ticket management baseret på ITIL 4 principper
    /// Håndterer service requests, incidents og problem management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(TicketService ticketService, ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        /// <summary>
        /// Hent alle tickets med filtrering og paginering
        /// </summary>
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TicketGetDto>>> GetTickets([FromQuery] TicketSearchDto searchDto)
        {
            try
            {
                // For demo mode, use demo user ID if no authenticated user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "demo-user-123";
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                var tickets = await _ticketService.GetTicketsAsync(searchDto, userId, userRole);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af tickets");
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Hent specifik ticket med kommentarer og vedhæftninger
        /// </summary>
        [HttpGet("{id}")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<ActionResult<TicketGetDto>> GetTicket(string id)
        {
            try
            {
                // For demo mode, use demo user ID if no authenticated user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "demo-user-123";
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                var ticket = await _ticketService.GetTicketByIdAsync(id, userId, userRole);
                if (ticket == null)
                {
                    return NotFound("Ticket ikke fundet");
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af ticket {TicketId}", id);
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Opret nyt ticket (kun booking ejere)
        /// </summary>
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<ActionResult<TicketGetDto>> CreateTicket([FromBody] TicketCreateDto createDto)
        {
            try
            {
                // For demo mode, use demo user ID if no authenticated user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "9c93e99f-67b4-4179-b61d-8218d564faa9";
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                // Valider at brugeren har adgang til den relaterede booking
                /*if (!string.IsNullOrEmpty(createDto.BookingId))
                {
                    var hasAccess = await _ticketService.ValidateBookingAccessAsync(createDto.BookingId, userId);
                    if (!hasAccess)
                    {
                        return Forbid("Du har ikke adgang til denne booking");
                    }
                }*/

                var ticket = await _ticketService.CreateTicketAsync(createDto, userId);
                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved oprettelse af ticket");
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Opdater ticket (kun assignee eller admin)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TicketGetDto>> UpdateTicket(string id, [FromBody] TicketUpdateDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                updateDto.Id = id;
                var ticket = await _ticketService.UpdateTicketAsync(updateDto, userId, userRole);
                if (ticket == null)
                {
                    return NotFound("Ticket ikke fundet eller ingen adgang");
                }

                return Ok(ticket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved opdatering af ticket {TicketId}", id);
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Slet ticket (kun admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTicket(string id)
        {
            try
            {
                var success = await _ticketService.DeleteTicketAsync(id);
                if (!success)
                {
                    return NotFound("Ticket ikke fundet");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved sletning af ticket {TicketId}", id);
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Tildel ticket til en medarbejder
        /// </summary>
        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Admin,Reception,CleaningStaff")]
        public async Task<ActionResult<TicketGetDto>> AssignTicket(string id, [FromBody] string assigneeId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                var ticket = await _ticketService.AssignTicketAsync(id, assigneeId, userId, userRole);
                if (ticket == null)
                {
                    return NotFound("Ticket ikke fundet eller ingen adgang");
                }

                return Ok(ticket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved tildeling af ticket {TicketId}", id);
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Luk ticket (marker som løst)
        /// </summary>
        [HttpPost("{id}/close")]
        [Authorize(Roles = "Admin,Reception,CleaningStaff")]
        public async Task<ActionResult<TicketGetDto>> CloseTicket(string id, [FromBody] string resolution)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                var ticket = await _ticketService.CloseTicketAsync(id, resolution, userId, userRole);
                if (ticket == null)
                {
                    return NotFound("Ticket ikke fundet eller ingen adgang");
                }

                return Ok(ticket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved lukning af ticket {TicketId}", id);
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Hent ticket kommentarer
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<TicketCommentDto>>> GetTicketComments(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                var comments = await _ticketService.GetTicketCommentsAsync(id, userId, userRole);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af ticket kommentarer {TicketId}", id);
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Tilføj kommentar til ticket
        /// </summary>
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<TicketCommentDto>> AddTicketComment(string id, [FromBody] TicketCommentCreateDto commentDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                commentDto.TicketId = id;
                var comment = await _ticketService.AddTicketCommentAsync(commentDto, userId, userRole);
                if (comment == null)
                {
                    return NotFound("Ticket ikke fundet eller ingen adgang");
                }

                return CreatedAtAction(nameof(GetTicketComments), new { id }, comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved tilføjelse af ticket kommentar {TicketId}", id);
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Hent ticket statistikker (kun admin)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetTicketStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var statistics = await _ticketService.GetTicketStatisticsAsync(fromDate, toDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af ticket statistikker");
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Hent mine tickets (tickets hvor brugeren er requester eller assignee)
        /// </summary>
        [HttpGet("my-tickets")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TicketGetDto>>> GetMyTickets([FromQuery] TicketSearchDto searchDto)
        {
            try
            {
                // For demo mode, use demo user ID if no authenticated user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "9c93e99f-67b4-4179-b61d-8218d564faa9";
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                searchDto.RequesterId = userId; // Filtrer kun på brugerens tickets
                var tickets = await _ticketService.GetTicketsAsync(searchDto, userId, "User");
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af mine tickets");
                return StatusCode(500, "Intern server fejl");
            }
        }

        /// <summary>
        /// Hent tickets tildelt til mig (kun staff)
        /// </summary>
        [HttpGet("assigned-to-me")]
        [Authorize(Roles = "Admin,Reception,CleaningStaff")]
        public async Task<ActionResult<IEnumerable<TicketGetDto>>> GetAssignedTickets([FromQuery] TicketSearchDto searchDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bruger ikke fundet");
                }

                searchDto.AssigneeId = userId; // Filtrer kun på tickets tildelt brugeren
                var tickets = await _ticketService.GetTicketsAsync(searchDto, userId, "Staff");
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af tildelte tickets");
                return StatusCode(500, "Intern server fejl");
            }
        }
    }
}
