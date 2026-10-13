using KMCEventClient.Models;
using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;
using KMCEventClient.Filters;

namespace KMCEventClient.Controllers
{
    public class EventsController : Controller
    {
        private readonly IApiClient _api;

        public EventsController(IApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? type, DateTime? date, string? location)
        {
            var events = await _api.SearchEventsAsync(type, date, location);

            // Determine current logged-in user id from session JWT (if present)
            int? currentUserId = null;
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var parts = token.Split('.');
                    if (parts.Length >= 2)
                    {
                        var payload = parts[1];
                        payload = payload.Replace('-', '+').Replace('_', '/');
                        switch (payload.Length % 4)
                        {
                            case 2: payload += "=="; break;
                            case 3: payload += "="; break;
                        }
                        var bytes = Convert.FromBase64String(payload);
                        var json = System.Text.Encoding.UTF8.GetString(bytes);
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("userId", out var el))
                        {
                            if (el.ValueKind == System.Text.Json.JsonValueKind.Number && el.TryGetInt32(out var n)) currentUserId = n;
                            else if (el.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(el.GetString(), out var p)) currentUserId = p;
                        }

                        // also extract role if present
                        if (doc.RootElement.TryGetProperty("role", out var roleEl))
                        {
                            ViewBag.CurrentUserRole = roleEl.GetString();
                        }
                        else
                        {
                            ViewBag.CurrentUserRole = null;
                        }
                    }
                }
            }
            catch
            {
                // ignore parse errors
            }

            ViewBag.CurrentUserId = currentUserId;

            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        [SessionAuthorize]
        public IActionResult Create()
        {
            return View(new Event { StartDate = DateTime.Now });
        }

        [HttpPost]
        [SessionAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event ev)
        {
            if (!ModelState.IsValid) return View(ev);
            var created = await _api.CreateEventAsync(ev);
            if (created == null)
            {
                ModelState.AddModelError(string.Empty, "Could not create event.");
                return View(ev);
            }
            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        [HttpPost]
        [SessionAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event ev)
        {
            if (id != ev.Id) return BadRequest();
            if (!ModelState.IsValid) return View(ev);
            var ok = await _api.UpdateEventAsync(id, ev);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Could not update event.");
                return View(ev);
            }
            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [SessionAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _api.DeleteEventAsync(id);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Could not delete event.");
                var ev = await _api.GetEventAsync(id);
                return View("Delete", ev);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
