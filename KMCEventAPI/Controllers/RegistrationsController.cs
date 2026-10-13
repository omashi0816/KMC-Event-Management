using KMCEventAPI.Data;
using KMCEventAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KMCEventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly KMCEventDbContext _context;

        public RegistrationsController(KMCEventDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // GET: api/Registrations
        // Get all registrations
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Registration>>> GetRegistrations()
        {
            var registrations = await _context.Registrations
                .Include(r => r.Participant)
                .ToListAsync();

            return Ok(registrations);
        }


        // =========================================================
        // GET: api/Registrations/5
        // Get one registration
        // =========================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Registration>> GetRegistration(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Participant)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration == null)
            {
                return NotFound();
            }

            return Ok(registration);
        }


        // =========================================================
        // GET: api/Registrations/event/5
        // Get registrations for one event
        // =========================================================
        [HttpGet("event/{eventId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Registration>>> GetRegistrationsByEvent(int eventId)
        {
            // Check whether event exists
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
            {
                return NotFound("Event does not exist.");
            }


            // Get registrations with participant details
            var registrations = await _context.Registrations
                .Include(r => r.Participant)
                .Where(r => r.EventId == eventId)
                .ToListAsync();


            return Ok(registrations);
        }


        // =========================================================
        // POST: api/Registrations
        // Public users can register for events
        // =========================================================
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Registration>> CreateRegistration(
            Registration registration)
        {
            // Validate payload
            if (registration == null)
            {
                return BadRequest("Registration payload is required.");
            }


            // =====================================================
            // Check event exists
            // =====================================================

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(
                    e => e.EventId == registration.EventId
                );


            if (eventItem == null)
            {
                return BadRequest("Event does not exist.");
            }


            // =====================================================
            // Create participant if participant details supplied
            // =====================================================

            if (
                registration.ParticipantId == 0 &&
                registration.Participant != null
            )
            {
                var p = registration.Participant;


                // Validate participant
                if (
                    string.IsNullOrWhiteSpace(p.Name) ||
                    string.IsNullOrWhiteSpace(p.Email) ||
                    string.IsNullOrWhiteSpace(p.Phone)
                )
                {
                    return BadRequest(
                        "Participant Name, Email and Phone are required."
                    );
                }


                // Check existing participant by email
                var existing =
                    await _context.Participants
                        .FirstOrDefaultAsync(
                            x => x.Email == p.Email
                        );


                if (existing != null)
                {
                    // Existing participant
                    registration.ParticipantId =
                        existing.ParticipantId;
                }
                else
                {
                    // Create new participant
                    var newP = new Participant
                    {
                        Name = p.Name,
                        Email = p.Email,
                        Phone = p.Phone
                    };


                    _context.Participants.Add(newP);

                    await _context.SaveChangesAsync();


                    registration.ParticipantId =
                        newP.ParticipantId;
                }
            }


            // =====================================================
            // Check participant exists
            // =====================================================

            var participantExists =
                await _context.Participants
                    .AnyAsync(
                        p =>
                            p.ParticipantId ==
                            registration.ParticipantId
                    );


            if (!participantExists)
            {
                return BadRequest(
                    "Participant does not exist."
                );
            }


            // =====================================================
            // Prevent duplicate registration
            // =====================================================

            var alreadyRegistered =
                await _context.Registrations
                    .AnyAsync(
                        r =>
                            r.EventId ==
                            registration.EventId
                            &&
                            r.ParticipantId ==
                            registration.ParticipantId
                    );


            if (alreadyRegistered)
            {
                return Conflict(
                    "This participant is already registered for the event."
                );
            }


            // =====================================================
            // Check event capacity
            // =====================================================

            if (eventItem.Capacity > 0)
            {
                var currentCount =
                    await _context.Registrations
                        .CountAsync(
                            r =>
                                r.EventId ==
                                registration.EventId
                        );


                if (currentCount >= eventItem.Capacity)
                {
                    return BadRequest(
                        "Event capacity has been reached."
                    );
                }
            }


            // =====================================================
            // Save registration
            // =====================================================

            _context.Registrations.Add(
                registration
            );


            await _context.SaveChangesAsync();


            // =====================================================
            // Return successful response
            // =====================================================

            return CreatedAtAction(
                nameof(GetRegistration),
                new
                {
                    id = registration.RegistrationId
                },
                registration
            );
        }


        // =========================================================
        // DELETE: api/Registrations/5
        // Delete registration
        // =========================================================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRegistration(
            int id
        )
        {
            var registration =
                await _context.Registrations
                    .FindAsync(id);


            if (registration == null)
            {
                return NotFound();
            }


            _context.Registrations.Remove(
                registration
            );


            await _context.SaveChangesAsync();


            return NoContent();
        }
    }
}