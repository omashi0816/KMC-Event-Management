using KMCEventClient.Models;
using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventClient.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiClient _api;

        public AuthController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginRequest("",""));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            if (!ModelState.IsValid) return View(req);

            var token = await _api.LoginAsync(req);
            if (token == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(req);
            }

            HttpContext.Session.SetString("JwtToken", token);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterRequest("","","", null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            if (!ModelState.IsValid) return View(req);

            var ok = await _api.RegisterAsync(req);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Could not register user (email may already exist).");
                return View(req);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
