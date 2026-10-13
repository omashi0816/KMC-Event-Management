using KMCEventAPI.Data;
using KMCEventAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KMCEventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantsController : ControllerBase
    {
        private readonly KMCEventDbContext _context;

        public ParticipantsController(KMCEventDbContext context)
        {
            _context = context;
        }

        // GET: api/Participants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Participant>>> GetParticipants()
        {
            return await _context.Participants.ToListAsync();
        }
        // GET: api/Participants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Participant>> GetParticipant(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null)
            {
                return NotFound();
            }
            return participant;
        }

        // POST: api/Participants
        // Allow anonymous so public users can create a participant record before registering
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Participant>> CreateParticipant([FromBody] Participant participant)
        {
            // Basic validation
            if (participant == null)
            {
                return BadRequest("Participant payload is required.");
            }

            if (string.IsNullOrWhiteSpace(participant.Name) ||
                string.IsNullOrWhiteSpace(participant.Email) ||
                string.IsNullOrWhiteSpace(participant.Phone))
            {
                return BadRequest("Name, Email and Phone are required for a participant.");
            }

            // Prevent duplicate participant by email
            var exists = await _context.Participants
                .AnyAsync(p => p.Email == participant.Email);

            if (exists)
            {
                return Conflict("A participant with the same email already exists.");
            }

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParticipant), new { id = participant.ParticipantId }, participant);
        }

        // PUT: api/Participants/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateParticipant(int id, [FromBody] Participant participant)
        {
            if (participant == null)
            {
                return BadRequest("Participant payload is required.");
            }

            if (id != participant.ParticipantId && participant.ParticipantId != 0)
            {
                return BadRequest("Participant ID mismatch.");
            }

            if (string.IsNullOrWhiteSpace(participant.Name) ||
                string.IsNullOrWhiteSpace(participant.Email) ||
                string.IsNullOrWhiteSpace(participant.Phone))
            {
                return BadRequest("Name, Email and Phone are required for a participant.");
            }

            var existing = await _context.Participants.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Prevent duplicate email (exclude current participant)
            var emailConflict = await _context.Participants
                .AnyAsync(p => p.Email == participant.Email && p.ParticipantId != id);
            if (emailConflict)
            {
                return Conflict("A participant with the same email already exists.");
            }

            // Update fields
            existing.Name = participant.Name;
            existing.Email = participant.Email;
            existing.Phone = participant.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Participants/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var existing = await _context.Participants.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            _context.Participants.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}