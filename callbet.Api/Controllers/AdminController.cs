using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Admin.Commands;
using callbet.Application.Admin.Queries;
using callbet.Application.Jobs.Commands;
using callbet.Application.Chat.Commands;
using callbet.Application.Interfaces;

using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace callbet.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin")]
public class AdminController(IMediator mediator, CallbetDbContext context) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new GetUsersQuery(request));
        return Ok(result);
    }

    [HttpGet("professional-profiles")]
    public async Task<IActionResult> GetProfessionalProfiles([FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new GetProfessionalProfilesQuery(request));
        return Ok(result);
    }

    [HttpPut("toggle-verify/{userId}")]
    public async Task<IActionResult> ToggleVerifyProfessional(Guid userId)
    {
        var isVerified = await mediator.Send(new ToggleVerifyProfessionalCommand(userId, GetCurrentUserId()));
        return Ok(new { Message = isVerified ? "Professional verified" : "Professional unverified", IsVerified = isVerified });
    }

    [HttpGet("pending-professionals")]
    public async Task<IActionResult> GetPendingProfessionals()
    {
        var result = await mediator.Send(new GetPendingProfessionalsQuery());
        return Ok(result);
    }

    [HttpPut("approve/{userId}")]
    public async Task<IActionResult> ApproveProfessional(Guid userId)
    {
        var id = await mediator.Send(new ApproveProfessionalCommand(userId, GetCurrentUserId()));
        return Ok(new { Message = "Professional approved", Id = id });
    }

    [HttpPut("suspend/{userId}")]
    public async Task<IActionResult> SuspendProfessional(Guid userId)
    {
        var id = await mediator.Send(new SuspendProfessionalCommand(userId, GetCurrentUserId()));
        return Ok(new { Message = "Professional suspended", Id = id });
    }

    [HttpPut("activate/{userId}")]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        var id = await mediator.Send(new ActivateUserCommand(userId, GetCurrentUserId()));
        return Ok(new { Message = "User account activated", Id = id });
    }

    [HttpDelete("delete/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var id = await mediator.Send(new DeleteUserCommand(userId, GetCurrentUserId()));
        return Ok(new { Message = "User deleted", Id = id });
    }

    [HttpDelete("review/{reviewId}")]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        var result = await mediator.Send(new DeleteReviewCommand(reviewId, GetCurrentUserId()));
        return Ok(new { Message = "Review removed and action logged", Success = result });
    }

    [HttpDelete("chat/message/{messageId}")]
    public async Task<IActionResult> DeleteChatMessage(Guid messageId)
    {
        var result = await mediator.Send(new DeleteChatMessageCommand(messageId, GetCurrentUserId()));
        return Ok(new { Message = "Chat message removed and action logged", Success = result });
    }

    [HttpPost("service-category")]
    public async Task<IActionResult> CreateServiceCategory([FromBody] ServiceCategoryDto dto)
    {
        var id = await mediator.Send(new CreateServiceCategoryCommand(dto, GetCurrentUserId()));
        return Ok(new { Message = "Service category created", Id = id });
    }

    [AllowAnonymous]
    [HttpGet("service-categories")]
    public async Task<IActionResult> GetServiceCategories()
    {
        var result = await mediator.Send(new GetServiceCategoriesQuery());
        return Ok(result);
    }

    [HttpPost("service")]
    public async Task<IActionResult> CreateService([FromBody] ServiceDto dto)
    {
        var id = await mediator.Send(new CreateServiceCommand(dto, GetCurrentUserId()));
        return Ok(new { Message = "Service created", Id = id });
    }

    [AllowAnonymous]
    [HttpGet("services")]
    [HttpGet("services/{categoryId}")]
    public async Task<IActionResult> GetServices([FromServices] IAdminService adminService, Guid? categoryId, CancellationToken ct)
    {
        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            var result = await mediator.Send(new GetServicesByCategoryQuery(categoryId.Value), ct);
            return Ok(result);
        }
        var allServices = await adminService.GetServicesAsync(ct);
        return Ok(allServices);
    }

    [HttpPost("assign-service")]
    public async Task<IActionResult> AssignService([FromBody] ProfessionalServiceDto dto)
    {
        var id = await mediator.Send(new AssignServiceCommand(dto, GetCurrentUserId()));
        return Ok(new { Message = "Service assigned to professional", Id = id });
    }

    [AllowAnonymous]
    [HttpGet("professional-services/{profileId}")]
    public async Task<IActionResult> GetProfessionalServices(Guid profileId)
    {
        var result = await mediator.Send(new GetProfessionalServicesQuery(profileId));
        return Ok(result);
    }

    [HttpPost("admin/register")]
    public async Task<IActionResult> RegisterAdmin([FromBody] UserDto dto)
    {
        var id = await mediator.Send(new RegisterAdminCommand(dto, GetCurrentUserId()));
        return Ok(new { Message = "Admin registered successfully", Id = id });
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetAdminLogs([FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new GetAdminLogsQuery(request));
        return Ok(result);
    }

    // Content Moderation
    [HttpGet("moderation/reviews")]
    public async Task<IActionResult> GetFlaggedReviews()
    {
        var result = await mediator.Send(new GetFlaggedReviewsQuery());
        return Ok(result);
    }

    [HttpPut("moderation/reviews/{reviewId}/dismiss")]
    public async Task<IActionResult> DismissReviewFlag(Guid reviewId)
    {
        var result = await mediator.Send(new DismissReviewFlagCommand(reviewId, GetCurrentUserId()));
        return Ok(new { Message = "Review flag dismissed and audit event logged", Success = result });
    }

    // Service Zones
    [HttpGet("service-zones")]
    public async Task<IActionResult> GetServiceZones()
    {
        var result = await mediator.Send(new GetServiceZonesQuery());
        return Ok(result);
    }

    [HttpPost("service-zones")]
    public async Task<IActionResult> CreateServiceZone([FromBody] CreateServiceZoneDto dto)
    {
        var result = await mediator.Send(new CreateServiceZoneCommand(dto, GetCurrentUserId()));
        return Ok(result);
    }

    [HttpDelete("service-zones/{id}")]
    public async Task<IActionResult> DeleteServiceZone(int id)
    {
        var result = await mediator.Send(new DeleteServiceZoneCommand(id, GetCurrentUserId()));
        return Ok(new { Message = "Service zone deleted successfully", Success = result, Id = id });
    }

    [HttpGet("financial-overview")]
    public async Task<IActionResult> GetFinancialOverview(CancellationToken ct)
    {
        var nonCancelledJobs = await context.Jobs
            .AsNoTracking()
            .Where(j => j.Status != JobStatus.Cancelled)
            .ToListAsync(ct);

        var totalEscrow = nonCancelledJobs.Sum(j => j.Price);
        var activeEscrow = nonCancelledJobs
            .Where(j => j.Status == JobStatus.Assigned || j.Status == JobStatus.InProgress || j.Status == JobStatus.CompletedPendingApproval)
            .Sum(j => j.Price);
        var closedJobsPrice = nonCancelledJobs
            .Where(j => j.Status == JobStatus.Closed)
            .Sum(j => j.Price);
        var releasedToPros = closedJobsPrice * 0.85m;
        var platformRevenue = closedJobsPrice * 0.15m;

        var openReportsCount = await context.Reports.CountAsync(r => !r.IsResolved, ct);

        return Ok(new
        {
            TotalEscrowVolume = totalEscrow,
            ActiveEscrow = activeEscrow,
            ReleasedToPros = releasedToPros,
            PlatformRevenue15 = platformRevenue,
            OpenDisputesCount = openReportsCount
        });
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(CancellationToken ct)
    {
        var reports = await (from r in context.Reports.AsNoTracking()
                             join reporter in context.Users.AsNoTracking() on r.ReporterUserId equals reporter.Id into repJoin
                             from reporterUser in repJoin.DefaultIfEmpty()
                             join reported in context.Users.AsNoTracking() on r.ReportedUserId equals reported.Id into repdJoin
                             from reportedUser in repdJoin.DefaultIfEmpty()
                             orderby r.CreatedAt descending
                             select new
                             {
                                 r.Id,
                                 r.ReporterUserId,
                                 ReporterName = reporterUser != null ? (reporterUser.FirstName + " " + reporterUser.LastName).Trim() : "System User",
                                 ReporterEmail = reporterUser != null ? reporterUser.Email : null,
                                 r.ReportedUserId,
                                 ReportedUserName = reportedUser != null ? (reportedUser.FirstName + " " + reportedUser.LastName).Trim() : null,
                                 r.Reason,
                                 r.Details,
                                 r.CreatedAt,
                                 r.IsResolved
                             }).ToListAsync(ct);
        return Ok(reports);
    }

    [HttpPut("reports/{id}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid id, CancellationToken ct)
    {
        var report = await context.Reports.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (report == null) return NotFound(new { Message = "Report not found." });
        report.IsResolved = true;
        await context.SaveChangesAsync(ct);
        return Ok(new { Message = "Report marked as resolved.", Success = true });
    }
}

