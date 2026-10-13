using KMCEventAPI.Data;
using KMCEventAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================================================
// Controllers
// ==================================================
builder.Services.AddControllers();


// ==================================================
// CORS
// PHP + HTML + CSS + JavaScript Frontend
// ==================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8000",
                "http://127.0.0.1:8000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


// ==================================================
// Swagger
// ==================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT token as: Bearer {token}"
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        }
    );
});


// ==================================================
// JWT Authentication
// ==================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });


// ==================================================
// Authorization
// ==================================================
builder.Services.AddAuthorization();


// ==================================================
// Entity Framework Core
// SQL Server
// ==================================================
builder.Services.AddDbContext<KMCEventDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    );
});


// ==================================================
// Build Application
// ==================================================
var app = builder.Build();


// ==================================================
// Swagger
// ==================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ==================================================
// HTTPS
// ==================================================
app.UseHttpsRedirection();


// ==================================================
// CORS
// IMPORTANT:
// CORS must be before Authentication
// and Authorization
// ==================================================
app.UseCors("FrontendPolicy");


// ==================================================
// Authentication
// ==================================================
app.UseAuthentication();


// ==================================================
// Authorization
// ==================================================
app.UseAuthorization();


// ==================================================
// Controllers
// ==================================================
app.MapControllers();


// ==================================================
// Database Migration + Seed Default Organizer
// ==================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db =
            services.GetRequiredService<KMCEventDbContext>();

        // ------------------------------------------
        // Apply pending migrations
        // ------------------------------------------
        db.Database.Migrate();


        // ------------------------------------------
        // Seed Default Organizer
        // ------------------------------------------

        var seededEmail = "organizer@example.com";

        var existingUser =
            db.Users.FirstOrDefault(
                u => u.Email == seededEmail
            );

        if (existingUser == null)
        {
            var organizer = new User
            {
                Name = "Organizer",

                Email = seededEmail,

                Password =
                    BCrypt.Net.BCrypt.HashPassword(
                        "password"
                    ),

                Role = "Organizer"
            };

            db.Users.Add(organizer);

            db.SaveChanges();

            Console.WriteLine(
                "Default organizer user created."
            );
        }
        else
        {
            Console.WriteLine(
                "Default organizer user already exists."
            );
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "Database migration/seeding failed:"
        );

        Console.WriteLine(ex.Message);
    }
}


// ==================================================
// Run Application
// ==================================================
app.Run();


// ==================================================
// Required for Integration Tests
// ==================================================
public partial class Program
{
}