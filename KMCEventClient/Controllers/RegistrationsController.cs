using KMCEventClient.Models;
using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventClient.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly IApiClient _api;

        public RegistrationsController(IApiClient api)
        {
            _api = api;
        }

        // =========================================================
        // GET: Registrations
        // Public users can view registrations
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var regs = await _api.GetRegistrationsAsync();
            var events = await _api.GetEventsAsync();
            var participants = await _api.GetParticipantsAsync();

            var list = regs.Select(r =>
            {
                var ev = events.FirstOrDefault(e =>
                    e.EventId == r.EventId ||
                    e.Id == r.EventId);

                var p = participants.FirstOrDefault(pp =>
                    pp.ParticipantId == r.ParticipantId ||
                    pp.Id == r.ParticipantId);

                return new KMCEventClient.Models.RegistrationViewModel
                {
                    Id = r.RegistrationId,
                    EventId = r.EventId,
                    ParticipantId = r.ParticipantId,
                    EventTitle = ev?.Title ?? "Unknown Event",
                    ParticipantName = p?.FullName ?? p?.Name ?? "Unknown Participant",
                    RegistrationDate = r.RegistrationDate
                };
            }).ToList();

            return View(list);
        }

        // =========================================================
        // GET: Registrations/Details/5
        // Public users can view registration details
        // =========================================================
        public async Task<IActionResult> Details(int id)
        {
            var reg = await _api.GetRegistrationAsync(id);

            if (reg == null)
            {
                return NotFound();
            }

            return View(reg);
        }

        // =========================================================
        // GET: Registrations/Create
        // Public users can register for an event
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var events = await _api.GetEventsAsync();
            var participants = await _api.GetParticipantsAsync();

            ViewBag.Events = events;
            ViewBag.Participants = participants;

            return View(new Registration());
        }

        // =========================================================
        // POST: Registrations/Create
        // Public users can create registration
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Registration registration)
        {
            if (!ModelState.IsValid)
            {
                var events = await _api.GetEventsAsync();
                var participants = await _api.GetParticipantsAsync();

                ViewBag.Events = events;
                ViewBag.Participants = participants;

                return View(registration);
            }

            var (created, error) = await _api.CreateRegistrationAsync(registration);

            if (created == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error ?? "Could not create registration."
                );

                var events = await _api.GetEventsAsync();
                var participants = await _api.GetParticipantsAsync();

                ViewBag.Events = events;
                ViewBag.Participants = participants;

                return View(registration);
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET: Registrations/Delete/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var reg = await _api.GetRegistrationAsync(id);

            if (reg == null)
            {
                return NotFound();
            }

            return View(reg);
        }

        // =========================================================
        // POST: Registrations/Delete/5
        // =========================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _api.DeleteRegistrationAsync(id);

            if (!ok)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Could not delete registration."
                );

                var reg = await _api.GetRegistrationAsync(id);

                if (reg == null)
                {
                    return NotFound();
                }

                return View("Delete", reg);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}