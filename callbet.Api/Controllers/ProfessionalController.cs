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
using callbet.Application.Interfaces;
using System.Security.Claims;
namespace callbet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/professional")]
public class ProfessionalController(IMediator mediator, CallbetDbContext context) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var userId) ? userId : Guid.Empty;
    }

    private async Task<Guid> EnsureProfileIdAsync(Guid providedProfileId)
    {
        if (providedProfileId != Guid.Empty && await context.ProfessionalProfiles.AnyAsync(p => p.Id == providedProfileId))
        {
            return providedProfileId;
        }

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Guid.Empty;

        var profile = await context.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null) return profile.Id;

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

    [AllowAnonymous]
    [HttpGet("professional-profile-dashboard")]
    [HttpGet("professional-profile-dashboard/{id}")]
    public async Task<IActionResult> GetProfessionalProfileDashboard(Guid? id, CancellationToken ct)
    {
        if (id.HasValue && id.Value != Guid.Empty)
        {
            var targetResult = await mediator.Send(new GetProfessionalProfileDashboardQuery(id.Value), ct);
            if (targetResult == null)
            {
                var profile = await context.ProfessionalProfiles.FirstOrDefaultAsync(p => p.UserId == id.Value || p.Id == id.Value, ct);
                if (profile != null)
                {
                    targetResult = await mediator.Send(new GetProfessionalProfileDashboardQuery(profile.Id), ct);
                }
            }
            if (targetResult == null) return NotFound(new { Message = "Professional profile dashboard not found." });
            return Ok(targetResult);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            var fallbackProfile = await context.ProfessionalProfiles
                .OrderByDescending(p => p.OverallRating)
                .FirstOrDefaultAsync(ct);
            if (fallbackProfile != null)
            {
                var pubRes = await mediator.Send(new GetProfessionalProfileDashboardQuery(fallbackProfile.Id), ct);
                if (pubRes != null) return Ok(pubRes);
            }
            return Unauthorized(new { Message = "User identity could not be verified from token." });
        }

        var result = await mediator.Send(new GetProfessionalProfileDashboardQuery(currentUserId), ct);
        if (result == null)
        {
            var profileId = await EnsureProfileIdAsync(Guid.Empty);
            if (profileId != Guid.Empty)
            {
                result = await mediator.Send(new GetProfessionalProfileDashboardQuery(profileId), ct);
            }
        }

        if (result == null) return NotFound(new { Message = "Professional profile dashboard not found." });
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
        dto.Id = id;
        return Ok(new { Message = "Resume uploaded", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId, Resume = dto });
    }

    [HttpPost("resume/upload-document")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadResumeDocument(
        [FromForm] IFormFile? file,
        [FromForm] Guid? professionalProfileId,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "No resume PDF document provided." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { Message = "File size exceeds 10MB limit." });

        var allowed = new[] { ".pdf", ".docx", ".doc" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(new { Message = "Invalid file type. Allowed formats: .pdf, .docx, .doc" });

        var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "resumes");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var resumeFileUrl = $"{Request.Scheme}://{Request.Host}/uploads/resumes/{fileName}";
        var profileId = await EnsureProfileIdAsync(professionalProfileId ?? Guid.Empty);

        if (profileId != Guid.Empty)
        {
            var existingResume = await context.Resumes.FirstOrDefaultAsync(r => r.ProfessionalProfileId == profileId, ct);
            if (existingResume != null)
            {
                existingResume.ResumeFileUrl = resumeFileUrl;
                existingResume.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newResume = new Resume
                {
                    Id = Guid.NewGuid(),
                    ProfessionalProfileId = profileId,
                    ResumeFileUrl = resumeFileUrl,
                    Skills = new List<string>(),
                    UpdatedAt = DateTime.UtcNow
                };
                context.Resumes.Add(newResume);
            }
            await context.SaveChangesAsync(ct);
        }

        return Ok(new
        {
            Message = "Resume PDF uploaded successfully and attached to your professional profile.",
            ResumeFileUrl = resumeFileUrl,
            ProfessionalProfileId = profileId
        });
    }

    [HttpPost("resume/with-document")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadResumeWithDocument(
        [FromForm] string? summary,
        [FromForm] string? educationJson,
        [FromForm] string? experienceJson,
        [FromForm] List<string>? skills,
        [FromForm] List<string>? languages,
        [FromForm] IFormFile? file,
        [FromForm] Guid? professionalProfileId,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        string? resumeFileUrl = null;

        if (file != null && file.Length > 0)
        {
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { Message = "File size exceeds 10MB limit." });

            var allowed = new[] { ".pdf", ".docx", ".doc" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest(new { Message = "Invalid file type. Allowed formats: .pdf, .docx, .doc" });

            var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "resumes");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            resumeFileUrl = $"{Request.Scheme}://{Request.Host}/uploads/resumes/{fileName}";
        }

        var profileId = await EnsureProfileIdAsync(professionalProfileId ?? Guid.Empty);

        var dto = new ResumeDto
        {
            ProfessionalProfileId = profileId,
            Summary = summary,
            EducationJson = educationJson,
            ExperienceJson = experienceJson,
            Skills = skills ?? new List<string>(),
            Languages = languages,
            ResumeFileUrl = resumeFileUrl
        };

        var id = await mediator.Send(new UploadResumeCommand(dto), ct);
        dto.Id = id;
        return Ok(new { Message = "Resume saved successfully with uploaded PDF", Id = id, Resume = dto });
    }

    [HttpPost("certificate")]
    public async Task<IActionResult> AddCertificate([FromBody] CertificateDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new AddCertificateCommand(dto));
        dto.Id = id;
        return Ok(new { Message = "Certificate added", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId, Certificate = dto });
    }

    [HttpPost("certificate/upload-document")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCertificateDocument([FromForm] IFormFile? file, [FromServices] IWebHostEnvironment environment, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "No certificate document/image provided." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { Message = "File size exceeds 10MB limit." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(new { Message = "Invalid file type. Allowed formats: .jpg, .jpeg, .png, .webp, .pdf" });

        var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "certificates");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var documentImageUrl = $"{Request.Scheme}://{Request.Host}/uploads/certificates/{fileName}";
        return Ok(new { Message = "Certificate document uploaded successfully", DocumentImageUrl = documentImageUrl });
    }

    [HttpPost("certificate/with-document")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddCertificateWithDocument(
        [FromForm] string? title,
        [FromForm] string? organization,
        [FromForm] string? issueDate,
        [FromForm] string? expiryDate,
        [FromForm] IFormFile? file,
        [FromForm] Guid? professionalProfileId,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        var actualTitle = !string.IsNullOrWhiteSpace(title) ? title :
            (Request.Form.ContainsKey("name") ? Request.Form["name"].ToString() :
             Request.Form.ContainsKey("certificateTitle") ? Request.Form["certificateTitle"].ToString() :
             Request.Form.ContainsKey("title") ? Request.Form["title"].ToString() : string.Empty);

        var actualOrganization = !string.IsNullOrWhiteSpace(organization) ? organization :
            (Request.Form.ContainsKey("issuingOrganization") ? Request.Form["issuingOrganization"].ToString() :
             Request.Form.ContainsKey("issuer") ? Request.Form["issuer"].ToString() :
             Request.Form.ContainsKey("institution") ? Request.Form["institution"].ToString() :
             Request.Form.ContainsKey("organization") ? Request.Form["organization"].ToString() : string.Empty);

        if (string.IsNullOrWhiteSpace(actualTitle) || string.IsNullOrWhiteSpace(actualOrganization))
            return BadRequest(new { Message = "Title and Organization are required." });

        var actualFile = file ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);

        if (actualFile == null || actualFile.Length == 0)
            return BadRequest(new { Message = "Certificate document file is required." });

        if (actualFile.Length > 10 * 1024 * 1024)
            return BadRequest(new { Message = "File size exceeds 10MB limit." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
        var ext = Path.GetExtension(actualFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
            return BadRequest(new { Message = "Invalid file type. Allowed formats: .jpg, .jpeg, .png, .webp, .pdf" });

        var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "certificates");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await actualFile.CopyToAsync(stream, ct);
        }

        var documentImageUrl = $"{Request.Scheme}://{Request.Host}/uploads/certificates/{fileName}";
        var profileId = await EnsureProfileIdAsync(professionalProfileId ?? Guid.Empty);

        if (profileId == Guid.Empty)
        {
            return Unauthorized(new { Message = "Unable to determine professional profile for current user." });
        }

        DateTime parsedIssueDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(issueDate) && DateTime.TryParse(issueDate, out var dtIssue))
        {
            parsedIssueDate = dtIssue.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dtIssue, DateTimeKind.Utc)
                : dtIssue.ToUniversalTime();
        }

        DateTime? parsedExpiryDate = null;
        if (!string.IsNullOrWhiteSpace(expiryDate) && DateTime.TryParse(expiryDate, out var dtExpiry))
        {
            parsedExpiryDate = dtExpiry.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dtExpiry, DateTimeKind.Utc)
                : dtExpiry.ToUniversalTime();
        }

        var dto = new CertificateDto
        {
            ProfessionalProfileId = profileId,
            Title = actualTitle,
            Organization = actualOrganization,
            IssueDate = parsedIssueDate,
            ExpiryDate = parsedExpiryDate,
            DocumentImageUrl = documentImageUrl
        };

        var id = await mediator.Send(new AddCertificateCommand(dto), ct);
        dto.Id = id;
        return Ok(new { Message = "Certificate added successfully", Id = id, Certificate = dto });
    }

    [HttpDelete("certificate/{id}")]
    public async Task<IActionResult> DeleteCertificate(Guid id, [FromServices] IProfessionalService professionalService, CancellationToken ct)
    {
        var profileId = await EnsureProfileIdAsync(Guid.Empty);
        var success = await professionalService.DeleteCertificateAsync(profileId, id, ct);
        if (!success) return NotFound(new { Message = "Certificate not found." });
        return Ok(new { Message = "Certificate deleted successfully", Success = true });
    }

    [HttpPost("portfolio")]
    public async Task<IActionResult> AddPortfolioItem([FromBody] PortfolioItemDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new AddPortfolioItemCommand(dto));
        dto.Id = id;
        return Ok(new { Message = "Portfolio item added", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId, PortfolioItem = dto });
    }

    [HttpPost("portfolio/upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPortfolioImage([FromForm] IFormFile? file, [FromServices] IWebHostEnvironment environment, CancellationToken ct)
    {
        var actualFile = file ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);
        if (actualFile == null || actualFile.Length == 0)
            return BadRequest(new { Message = "No portfolio image provided." });

        if (actualFile.Length > 10 * 1024 * 1024)
            return BadRequest(new { Message = "File size exceeds 10MB limit." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(actualFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
            return BadRequest(new { Message = "Invalid image type. Allowed formats: .jpg, .jpeg, .png, .webp, .gif" });

        var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "portfolio");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await actualFile.CopyToAsync(stream, ct);
        }

        var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/portfolio/{fileName}";
        return Ok(new { Message = "Portfolio image uploaded successfully", ImageUrl = imageUrl });
    }

    [HttpPost("portfolio/with-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddPortfolioItemWithImage(
        [FromForm] string? title,
        [FromForm] string? description,
        [FromForm] string? dateCompleted,
        [FromForm] IFormFile? file,
        [FromForm] Guid? professionalProfileId,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken ct)
    {
        var actualTitle = !string.IsNullOrWhiteSpace(title) ? title :
            (Request.Form.ContainsKey("name") ? Request.Form["name"].ToString() :
             Request.Form.ContainsKey("title") ? Request.Form["title"].ToString() : string.Empty);

        if (string.IsNullOrWhiteSpace(actualTitle))
            return BadRequest(new { Message = "Title is required." });

        var actualFile = file ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);

        if (actualFile == null || actualFile.Length == 0)
            return BadRequest(new { Message = "Portfolio image file is required." });

        if (actualFile.Length > 10 * 1024 * 1024)
            return BadRequest(new { Message = "File size exceeds 10MB limit." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(actualFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
            return BadRequest(new { Message = "Invalid image type. Allowed formats: .jpg, .jpeg, .png, .webp, .gif" });

        var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "portfolio");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await actualFile.CopyToAsync(stream, ct);
        }

        var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/portfolio/{fileName}";
        var profileId = await EnsureProfileIdAsync(professionalProfileId ?? Guid.Empty);

        if (profileId == Guid.Empty)
        {
            return Unauthorized(new { Message = "Unable to determine professional profile for current user." });
        }

        DateTime parsedDateCompleted = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dateCompleted) && DateTime.TryParse(dateCompleted, out var dtComp))
        {
            parsedDateCompleted = dtComp.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dtComp, DateTimeKind.Utc)
                : dtComp.ToUniversalTime();
        }

        var dto = new PortfolioItemDto
        {
            ProfessionalProfileId = profileId,
            Title = actualTitle,
            Description = description,
            ImageUrl = imageUrl,
            DateCompleted = parsedDateCompleted
        };

        var id = await mediator.Send(new AddPortfolioItemCommand(dto), ct);
        dto.Id = id;
        return Ok(new { Message = "Portfolio item added successfully", Id = id, PortfolioItem = dto });
    }

    [HttpDelete("portfolio/{id}")]
    public async Task<IActionResult> DeletePortfolioItem(Guid id, [FromServices] IProfessionalService professionalService, CancellationToken ct)
    {
        var profileId = await EnsureProfileIdAsync(Guid.Empty);
        var success = await professionalService.DeletePortfolioItemAsync(profileId, id, ct);
        if (!success) return NotFound(new { Message = "Portfolio item not found." });
        return Ok(new { Message = "Portfolio item deleted successfully", Success = true });
    }

    [HttpPost("assign-service")]
    public async Task<IActionResult> AssignService([FromBody] ProfessionalServiceDto dto)
    {
        dto.ProfessionalProfileId = await EnsureProfileIdAsync(dto.ProfessionalProfileId);
        var id = await mediator.Send(new AssignServiceCommand(dto));
        return Ok(new { Message = "Service assigned to professional", Id = id, ProfessionalProfileId = dto.ProfessionalProfileId });
    }

    [HttpDelete("services/{serviceId}")]
    [HttpDelete("unassign-service/{serviceId}")]
    public async Task<IActionResult> UnassignService(Guid serviceId, [FromServices] IProfessionalService professionalService, CancellationToken ct)
    {
        var profileId = await EnsureProfileIdAsync(Guid.Empty);
        var success = await professionalService.UnassignServiceAsync(profileId, serviceId, ct);
        if (!success) return NotFound(new { Message = "Service offering not found on this profile." });
        return Ok(new { Message = "Service removed successfully from profile.", Success = true, ServiceId = serviceId });
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

    [HttpGet("availability")]
    [AllowAnonymous]
    [HttpGet("availability/{profileId}")]
    public async Task<IActionResult> GetAvailability(Guid? profileId, CancellationToken ct)
    {
        Guid targetId = (profileId.HasValue && profileId.Value != Guid.Empty) ? profileId.Value : Guid.Empty;
        if (targetId == Guid.Empty)
        {
            targetId = await EnsureProfileIdAsync(Guid.Empty);
        }
        if (targetId == Guid.Empty)
        {
            return Ok(new List<AvailabilityScheduleDto>());
        }

        var result = await mediator.Send(new GetAvailabilitySchedulesQuery(targetId), ct);
        return Ok(result ?? new List<AvailabilityScheduleDto>());
    }

    [HttpDelete("availability/{id}")]
    public async Task<IActionResult> DeleteAvailabilitySchedule(Guid id, [FromServices] IProfessionalService professionalService, CancellationToken ct)
    {
        var profileId = await EnsureProfileIdAsync(Guid.Empty);
        var success = await professionalService.DeleteAvailabilityScheduleAsync(profileId, id, ct);
        if (!success) return NotFound(new { Message = "Availability schedule not found." });
        return Ok(new { Message = "Availability schedule deleted successfully", Success = true });
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

    [HttpPost("report-abuse")]
    public async Task<IActionResult> ReportAbuse([FromBody] ReportAbuseDto dto, CancellationToken ct)
    {
        var reporterId = GetCurrentUserId();
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterUserId = reporterId != Guid.Empty ? reporterId : (dto.ReporterUserId ?? Guid.Empty),
            ReportedUserId = dto.ReportedUserId ?? Guid.Empty,
            Reason = string.IsNullOrWhiteSpace(dto.Category) ? dto.Reason : $"[{dto.Category}] {dto.Reason}",
            Details = dto.Details,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync(ct);

        return Ok(new { Message = "Abuse report submitted successfully. Our safety and trust team will review this shortly.", ReportId = report.Id, Success = true });
    }
}
