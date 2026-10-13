
using KMCEventClient.Services;
using KMCEventClient.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace KMCEventClient.Controllers
{
    [SessionAuthorize]
    public class DashboardController : Controller
    {
        private readonly IApiClient _api;

        public DashboardController(IApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            // =========================================================
            // 1. Get logged-in organizer's events
            // =========================================================

            var events = (await _api.GetMyEventsAsync()).ToList();

            // =========================================================
            // 2. Read JWT token
            // =========================================================

            var token = HttpContext.Session.GetString("JwtToken");

            string organizerName = "Organizer";
            string organizerEmail = "";
            string organizerRole = "Organizer";
            int userId = 0;

            // =========================================================
            // 3. Read organizer information from JWT
            // =========================================================

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var parts = token.Split('.');

                    if (parts.Length >= 2)
                    {
                        var payload = parts[1];

                        payload = payload
                            .Replace('-', '+')
                            .Replace('_', '/');

                        switch (payload.Length % 4)
                        {
                            case 2:
                                payload += "==";
                                break;

                            case 3:
                                payload += "=";
                                break;
                        }

                        var bytes = Convert.FromBase64String(payload);
                        var json = Encoding.UTF8.GetString(bytes);

                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        // User ID
                        if (root.TryGetProperty("userId", out var userIdElement))
                        {
                            if (userIdElement.ValueKind == JsonValueKind.Number)
                            {
                                userIdElement.TryGetInt32(out userId);
                            }
                            else if (userIdElement.ValueKind == JsonValueKind.String)
                            {
                                int.TryParse(
                                    userIdElement.GetString(),
                                    out userId
                                );
                            }
                        }

                        // Name
                        if (root.TryGetProperty("name", out var nameElement))
                        {
                            organizerName =
                                nameElement.GetString() ?? "Organizer";
                        }

                        // Email
                        if (root.TryGetProperty("email", out var emailElement))
                        {
                            organizerEmail =
                                emailElement.GetString() ?? "";
                        }

                        // Role
                        if (root.TryGetProperty("role", out var roleElement))
                        {
                            organizerRole =
                                roleElement.GetString() ?? "Organizer";
                        }

                        // Extra safety
                        if (userId != 0)
                        {
                            events = events
                                .Where(e => e.OrganizerId == userId)
                                .ToList();
                        }
                    }
                }
                catch
                {
                    // Keep default values if JWT parsing fails
                }
            }

            // =========================================================
            // 4. Get registrations
            // =========================================================

            var registrations =
                (await _api.GetRegistrationsAsync()).ToList();

            // =========================================================
            // 5. Get participants
            // =========================================================

            var participants =
                (await _api.GetParticipantsAsync()).ToList();

            // =========================================================
            // 6. Create participant list for each organizer event
            // =========================================================

            var eventParticipants =
                new Dictionary<int, List<KMCEventClient.Models.Participant>>();

            foreach (var ev in events)
            {
                var participantIds = registrations
                    .Where(r => r.EventId == ev.EventId)
                    .Select(r => r.ParticipantId)
                    .Distinct()
                    .ToList();

                var eventParticipantList = participants
                    .Where(p =>
                        participantIds.Contains(p.ParticipantId) ||
                        participantIds.Contains(p.Id))
                    .ToList();

                eventParticipants[ev.EventId] =
                    eventParticipantList;
            }

            // =========================================================
            // 7. Dashboard statistics
            // =========================================================

            ViewBag.OrganizerName = organizerName;
            ViewBag.OrganizerEmail = organizerEmail;
            ViewBag.OrganizerRole = organizerRole;
            ViewBag.UserId = userId;

            ViewBag.TotalEvents = events.Count;

            ViewBag.UpcomingEvents = events.Count(e =>
                e.Date.Date >= DateTime.Today);

            ViewBag.EventParticipants = eventParticipants;

            ViewBag.TotalParticipants = eventParticipants
                .Values
                .SelectMany(x => x)
                .Select(p => p.ParticipantId)
                .Distinct()
                .Count();

            return View(events);
        }
    }
}

