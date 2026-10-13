using KMCEventAPI.Data;
using KMCEventAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KMCEventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly KMCEventDbContext _context;

        public EventsController(KMCEventDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: api/Events
        // PUBLIC
        // Returns ALL public events
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _context.Events
                .ToListAsync();

            return Ok(events);
        }

        // =========================================================
        // GET: api/Events/{id}
        // PUBLIC
        // =========================================================

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return Ok(eventItem);
        }

        // =========================================================
        // GET: api/Events/search
        // PUBLIC
        // =========================================================

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Event>>> SearchEvents(
            [FromQuery] string? type,
            [FromQuery] DateTime? date,
            [FromQuery] string? location)
        {
            var query = _context.Events
                .AsQueryable();

            // Filter by event type
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(e =>
                    e.Type == type);
            }

            // Filter by date
            if (date.HasValue)
            {
                var selectedDate = date.Value.Date;

                query = query.Where(e =>
                    e.Date.Date == selectedDate);
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e =>
                    EF.Functions.Like(
                        e.Location,
                        $"%{location}%"));
            }

            var events = await query.ToListAsync();

            return Ok(events);
        }

        // =========================================================
        // POST: api/Events
        // AUTHENTICATED USERS ONLY
        // OrganizerId comes ONLY from JWT
        // =========================================================

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Event>> CreateEvent(
            [FromBody] EventCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    "Event data is required.");
            }

            // Get UserId from JWT
            var userIdClaim =
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(
                    "User ID was not found in JWT.");
            }

            if (!int.TryParse(
                    userIdClaim,
                    out int organizerId))
            {
                return Unauthorized(
                    "Invalid user ID in JWT.");
            }

            // IMPORTANT:
            // OrganizerId is taken from JWT.
            // Client cannot choose OrganizerId.
            var eventItem = new Event
            {
                OrganizerId = organizerId,

                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Date = dto.Date,
                Time = dto.Time,
                Location = dto.Location,
                Capacity = dto.Capacity
            };

            _context.Events.Add(eventItem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new
                {
                    id = eventItem.EventId
                },
                eventItem);
        }

        // =========================================================
        // GET: api/Events/my-events
        // AUTHENTICATED USERS ONLY
        //
        // IMPORTANT:
        // This returns ONLY the current user's events.
        // =========================================================

        [HttpGet("my-events")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Event>>> GetMyEvents()
        {
            // Read UserId from JWT
            var userIdClaim =
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(
                    "User ID was not found in JWT.");
            }

            if (!int.TryParse(
                    userIdClaim,
                    out int currentUserId))
            {
                return Unauthorized(
                    "Invalid user ID in JWT.");
            }

            // IMPORTANT:
            // Server-side ownership filtering.
            //
            // User 1 -> only OrganizerId == 1
            // User 2 -> only OrganizerId == 2
            //
            // Never return all events here.
            var events = await _context.Events
                .Where(e =>
                    e.OrganizerId == currentUserId)
                .ToListAsync();

            return Ok(events);
        }

        // =========================================================
        // PUT: api/Events/{id}
        // CREATOR ONLY
        // =========================================================

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEvent(
            int id,
            [FromBody] EventUpdateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    "Event data is required.");
            }

            var existingEvent =
                await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            // Get current logged-in user
            var userIdClaim =
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized();
            }

            if (!int.TryParse(
                    userIdClaim,
                    out int currentUserId))
            {
                return Unauthorized();
            }

            // IMPORTANT:
            // Only creator can update.
            if (existingEvent.OrganizerId != currentUserId)
            {
                return Forbid();
            }

            // Update event details.
            // OrganizerId is NOT changed.
            existingEvent.Title = dto.Title;
            existingEvent.Description = dto.Description;
            existingEvent.Type = dto.Type;
            existingEvent.Date = dto.Date;
            existingEvent.Time = dto.Time;
            existingEvent.Location = dto.Location;
            existingEvent.Capacity = dto.Capacity;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================================================
        // DELETE: api/Events/{id}
        // CREATOR ONLY
        // =========================================================

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(
            int id)
        {
            var eventItem =
                await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Get current logged-in user
            var userIdClaim =
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized();
            }

            if (!int.TryParse(
                    userIdClaim,
                    out int currentUserId))
            {
                return Unauthorized();
            }

            // IMPORTANT:
            // Only creator can delete.
            if (eventItem.OrganizerId != currentUserId)
            {
                return Forbid();
            }

            _context.Events.Remove(eventItem);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

