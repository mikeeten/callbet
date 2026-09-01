using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using callbet.Application.DTOs;
using callbet.Application.Professionals.Commands;
using callbet.Application.Professionals.Queries;
using callbet.Application.Jobs.Commands;
using callbet.Application.Jobs.Queries;
using System.Security.Claims;

namespace callbet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/professional")]
public class ProfessionalController(IMediator mediator, CallbetDbContext context) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    private async Task<Guid> EnsureProfileIdAsync(Guid providedProfileId)
    {
        if (providedProfileId != Guid.Empty)
        {
            return providedProfileId;
        }

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Guid.Empty;

        var profile = await context.ProfessionalProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new ProfessionalProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Headline = "Professional",
                Bio = string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ProfessionalProfiles.Add(profile);
            await context.SaveChangesAsync();
        }

        return profile.Id;
    }

    [HttpGet("my-profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await mediator.Send(new GetProfessionalProfileDetailsQuery(userId));
        if (result == null)
        {
            var profileId = await EnsureProfileIdAsync(Guid.Empty);
            result = await mediator.Send(new GetProfessionalProfileDetailsQuery(profileId));
        }

        return Ok(result);
    }

    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] ProfessionalProfileDto dto)
    {
        if (dto.UserId == Guid.Empty)
        {
            dto.UserId = GetCurrentUserId();
        }
        var id = await mediator.Send(new CreateProfessionalProfileCommand(dto));
        return Ok(new { Message = "Professional profile created or updated", Id = id });
    }

    [HttpPost("resume")]
    public async Task<IActionResult> UploadResume([FromBody] ResumeDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new UploadResumeCommand(dto));
        return Ok(new { Message = "Resume uploaded", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId });
    }

    [HttpPost("certificate")]
    public async Task<IActionResult> AddCertificate([FromBody] CertificateDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new AddCertificateCommand(dto));
        return Ok(new { Message = "Certificate added", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId });
    }

    [HttpPost("portfolio")]
    public async Task<IActionResult> AddPortfolioItem([FromBody] PortfolioItemDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new AddPortfolioItemCommand(dto));
        return Ok(new { Message = "Portfolio item added", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId });
    }

    [HttpPost("assign-service")]
    public async Task<IActionResult> AssignService([FromBody] ProfessionalServiceDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new AssignServiceCommand(dto));
        return Ok(new { Message = "Service assigned to professional", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId });
    }

    [AllowAnonymous]
    [HttpGet("services/{profileId}")]
    public async Task<IActionResult> GetProfessionalServices(Guid profileId)
    {
        var result = await mediator.Send(new GetProfessionalServicesQuery(profileId));
        return Ok(result);
    }

    [HttpPost("review/reply")]
    public async Task<IActionResult> ReplyToReview([FromBody] ReviewReplyDto dto)
    {
        var id = await mediator.Send(new ReplyToReviewCommand(dto));
        return Ok(new { Message = "Reply added to review", Id = id });
    }

    [AllowAnonymous]
    [HttpGet("reviews/{profileId}")]
    public async Task<IActionResult> GetReviews(Guid profileId)
    {
        var result = await mediator.Send(new GetReviewsForProfessionalQuery(profileId));
        return Ok(result);
    }

    [HttpPut("refresh-reputation/{userId}")]
    public async Task<IActionResult> RefreshReputation(Guid userId)
    {
        var result = await mediator.Send(new RefreshReputationCommand(userId));
        return Ok(new { Message = "Professional reputation refreshed", Success = result });
    }

    [HttpPost("availability")]
    public async Task<IActionResult> AddAvailability([FromBody] AvailabilityScheduleDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new AddAvailabilityScheduleCommand(dto));
        return Ok(new { Message = "Availability schedule added", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId });
    }

    [AllowAnonymous]
    [HttpGet("availability/{profileId}")]
    public async Task<IActionResult> GetAvailability(Guid profileId)
    {
        var result = await mediator.Send(new GetAvailabilitySchedulesQuery(profileId));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("profile-details/{id}")]
    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfileDetails(Guid id)
    {
        var result = await mediator.Send(new GetProfessionalProfileDetailsQuery(id));
        if (result == null) return NotFound(new { Message = "Professional profile not found" });
        return Ok(result);
    }
}
