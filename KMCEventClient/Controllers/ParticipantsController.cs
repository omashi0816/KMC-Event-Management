
using KMCEventClient.Models;
using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventClient.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly IApiClient _api;

        public ParticipantsController(IApiClient api)
        {
            _api = api;
        }

        // Public - anyone can view participants
        public async Task<IActionResult> Index()
        {
            var participants = await _api.GetParticipantsAsync();

            return View(participants);
        }

        // Public - participant registration page
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Participant());
        }

        // Public - create participant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Participant participant)
        {
            if (!ModelState.IsValid)
            {
                return View(participant);
            }

            var created = await _api.CreateParticipantAsync(participant);

            if (created == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Could not register participant."
                );

                return View(participant);
            }

            TempData["SuccessMessage"] =
                "Participant registration completed successfully.";

            return RedirectToAction(nameof(Index));
        }

        // View participant details
        public async Task<IActionResult> Details(int id)
        {
            var participant = await _api.GetParticipantAsync(id);

            if (participant == null)
            {
                return NotFound();
            }

            return View(participant);
        }
    }
}

