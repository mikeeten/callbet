using MediatR;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using callbet.Domain.Entities;
using callbet.Application.Interfaces;
using callbet.Infrastructure.Services;
using callbet.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(typeof(ICustomerService).Assembly);
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProfessionalService, callbet.Infrastructure.Services.ProfessionalService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<CryptoDemoService>();
builder.Services.AddScoped<IAuthorizationHandler, VerifiedProfessionalHandler>();

builder.Services.AddProblemDetails();

// Identity Core Configuration with Enterprise Password and Lockout Policies
builder.Services.AddIdentityCore<User>(options =>
{
    // Password Policy (6+ characters)
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;

    // Brute-Force Lockout Protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<CallbetDbContext>();

// Configure JWT Bearer Authentication Pipeline
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

    // 📡 Extract JWT token from Query string for WebSocket / SignalR connections
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Configure Enterprise Policy Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ProfessionalOnly", policy => policy.RequireRole("Professional"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("VerifiedProfessional", policy =>
    {
        policy.Requirements.Add(new VerifiedProfessionalRequirement());
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddDbContext<CallbetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CALLBET")));


var app = builder.Build();

// Security Headers Middleware (Defense-in-Depth)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' http://localhost:5189 http://localhost:4200 ws://localhost:5189 wss://localhost:5189 ws://localhost:4200 wss://localhost:4200;");
    await next();
});

if (app.Environment.IsDevelopment())
{
    // Development: expose OpenAPI + Scalar explorer
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    // Production: hide explorers, use exception handler
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseStatusCodePages();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();   // establish identity
app.UseAuthorization();    // enforce policies
app.MapControllers();
app.MapHub<callbet.Infrastructure.Hubs.ChatHub>("/hubs/chat");

// Auto-seed Addis Ababa SubCities & Neighborhoods on startup
await callbet.Infrastructure.Persistence.DbInitializer.SeedLocationDataAsync(app.Services);

app.Run();