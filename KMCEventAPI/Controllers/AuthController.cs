
using KMCEventAPI.Data;
using KMCEventAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KMCEventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly KMCEventDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(
            KMCEventDbContext context,
            IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        // =====================================================
        // LOGIN
        // POST: /api/Auth/login
        // =====================================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest req)
        {
            if (req == null ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(
                    "Email and password are required."
                );
            }


            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == req.Email
                );


            if (user == null)
            {
                return Unauthorized(
                    "Invalid email or password."
                );
            }


            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return StatusCode(
                    500,
                    "User password hash is missing."
                );
            }


            bool passwordValid;

            try
            {
                passwordValid =
                    BCrypt.Net.BCrypt.Verify(
                        req.Password,
                        user.Password
                    );
            }
            catch
            {
                return StatusCode(
                    500,
                    "Password verification failed."
                );
            }


            if (!passwordValid)
            {
                return Unauthorized(
                    "Invalid email or password."
                );
            }


            string token;

            try
            {
                token = GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    $"Token generation failed: {ex.Message}"
                );
            }


            return Ok(new
            {
                token = token,
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                role = user.Role
            });
        }


        // =====================================================
        // REGISTER
        // POST: /api/Auth/register
        // =====================================================

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest req)
        {
            if (req == null)
            {
                return BadRequest(
                    "Registration data is required."
                );
            }


            // -----------------------------------------------
            // Validate required fields
            // -----------------------------------------------

            if (string.IsNullOrWhiteSpace(req.Name))
            {
                return BadRequest(
                    "Name is required."
                );
            }


            if (string.IsNullOrWhiteSpace(req.Email))
            {
                return BadRequest(
                    "Email is required."
                );
            }


            if (string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(
                    "Password is required."
                );
            }


            if (req.Password.Length < 6)
            {
                return BadRequest(
                    "Password must contain at least 6 characters."
                );
            }


            // -----------------------------------------------
            // Check email
            // -----------------------------------------------

            var emailExists =
                await _context.Users.AnyAsync(
                    u => u.Email == req.Email
                );


            if (emailExists)
            {
                return BadRequest(
                    "Email already registered."
                );
            }


            // -----------------------------------------------
            // Validate role
            // -----------------------------------------------

            string role =
                string.IsNullOrWhiteSpace(req.Role)
                    ? "User"
                    : req.Role;


            if (role != "User" &&
                role != "Organizer")
            {
                return BadRequest(
                    "Invalid role."
                );
            }


            // -----------------------------------------------
            // Create user
            // -----------------------------------------------

            var user = new User
            {
                Name = req.Name.Trim(),

                Email = req.Email.Trim(),

                Password =
                    BCrypt.Net.BCrypt.HashPassword(
                        req.Password
                    ),

                Role = role
            };


            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            // -----------------------------------------------
            // Response
            // -----------------------------------------------

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    userId = user.UserId,
                    name = user.Name,
                    email = user.Email,
                    role = user.Role,
                    message = "Registration successful."
                }
            );
        }


        // =====================================================
        // GENERATE JWT TOKEN
        // =====================================================

        private string GenerateJwtToken(User user)
        {
            var key = _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "JWT key is not configured."
                );
            }


            var issuer =
                _config["Jwt:Issuer"]
                ?? "KMCEventAPI";


            var audience =
                _config["Jwt:Audience"]
                ?? "KMCEventAPIUsers";


            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Email
                ),

                new Claim(
                    "userId",
                    user.UserId.ToString()
                ),

                new Claim(
                    ClaimTypes.Role,
                    user.Role ?? "User"
                )
            };


            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key)
                );


            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256
                );


            var token =
                new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(8),
                    signingCredentials: credentials
                );


            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }


    // =========================================================
    // REQUEST MODELS
    // =========================================================

    public record LoginRequest(
        string Email,
        string Password
    );


    public record RegisterRequest(
        string Name,
        string Email,
        string Password,
        string? Role
    );
}

