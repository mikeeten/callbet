using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;

namespace callbet.Infrastructure.Security;

public class VerifiedProfessionalHandler : AuthorizationHandler<VerifiedProfessionalRequirement>
{
    private readonly CallbetDbContext _context;

    public VerifiedProfessionalHandler(CallbetDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VerifiedProfessionalRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        // Admins satisfy this requirement
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // Must be in role Professional
        if (!context.User.IsInRole("Professional"))
        {
            return;
        }

        // Check if professional profile exists and is verified
        var isVerified = await _context.ProfessionalProfiles
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.IsVerified);

        if (isVerified)
        {
            context.Succeed(requirement);
        }
    }
}
