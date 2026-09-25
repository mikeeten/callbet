using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using callbet.Application.Verification.Commands;
using callbet.Application.Verification.Queries;

namespace callbet.Api.Controllers
{
    [ApiController]
    [Route("api/verification")]
    public class VerificationController(IMediator mediator, IVerificationService verificationService) : ControllerBase
    {
        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("id")?.Value
                ?? User.FindFirst("userId")?.Value
                ?? User.FindFirst(ClaimTypes.Sid)?.Value;
            return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
        }

        // 1. Get verification profile for a single user (Public / Clients can see verified badge & details)
        [AllowAnonymous]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserVerificationProfile(Guid userId, CancellationToken ct)
        {
            var result = await verificationService.GetUserVerificationProfileAsync(userId, ct);
            if (result == null) return NotFound(new { message = "User verification profile not found." });

            return Ok(result);
        }

        // 2. Get logged-in user's own verification status (via JWT Token)
        [Authorize]
        [HttpGet("my-verification")]
        public async Task<IActionResult> GetMyVerificationProfile(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await verificationService.GetUserVerificationProfileAsync(userId, ct);
            if (result == null) return NotFound(new { message = "Verification profile not found." });

            return Ok(result);
        }

        // 3. Upload verification document file only (PDF / Image)
        [Authorize]
        [HttpPost("upload-document")]
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadVerificationDocument(
            [FromForm] IFormFile? file,
            [FromServices] IWebHostEnvironment environment,
            CancellationToken ct)
        {
            var actualFile = file ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);
            if (actualFile == null || actualFile.Length == 0)
                return BadRequest(new { Message = "No verification document or image provided." });

            if (actualFile.Length > 10 * 1024 * 1024)
                return BadRequest(new { Message = "File size exceeds 10MB limit." });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var ext = Path.GetExtension(actualFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
                return BadRequest(new { Message = "Invalid file type. Allowed formats: .jpg, .jpeg, .png, .webp, .pdf" });

            var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "verification");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await actualFile.CopyToAsync(stream, ct);
            }

            var documentUrl = $"{Request.Scheme}://{Request.Host}/uploads/verification/{fileName}";
            return Ok(new { Message = "Verification document uploaded successfully", DocumentUrl = documentUrl });
        }

        // 4. Submit verification document directly with attached file (PDF / Image)
        [Authorize]
        [HttpPost("with-document")]
        [HttpPost("upload-with-file")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadVerificationWithDocument(
            [FromForm] string? documentType,
            [FromForm] IFormFile? file,
            [FromForm] Guid? userId,
            [FromServices] IWebHostEnvironment environment,
            CancellationToken ct)
        {
            var actualType = !string.IsNullOrWhiteSpace(documentType) ? documentType :
                (Request.Form.ContainsKey("type") ? Request.Form["type"].ToString() :
                 Request.Form.ContainsKey("docType") ? Request.Form["docType"].ToString() : "National ID");

            var actualFile = file ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);
            if (actualFile == null || actualFile.Length == 0)
                return BadRequest(new { Message = "Verification document file (image or PDF) is required." });

            if (actualFile.Length > 10 * 1024 * 1024)
                return BadRequest(new { Message = "File size exceeds 10MB limit." });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var ext = Path.GetExtension(actualFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
                return BadRequest(new { Message = "Invalid file type. Allowed formats: .jpg, .jpeg, .png, .webp, .pdf" });

            var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "verification");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await actualFile.CopyToAsync(stream, ct);
            }

            var documentUrl = $"{Request.Scheme}://{Request.Host}/uploads/verification/{fileName}";
            var targetUserId = userId.HasValue && userId.Value != Guid.Empty ? userId.Value : GetCurrentUserId();

            if (targetUserId == Guid.Empty)
                return Unauthorized(new { Message = "Unable to determine user identity from token." });

            var dto = new VerificationRecordDto
            {
                UserId = targetUserId,
                DocumentType = actualType,
                DocumentUrl = documentUrl,
                IsVerified = false
            };

            var id = await mediator.Send(new UploadDocumentCommand(dto), ct);
            dto.Id = id;
            return Ok(new { Message = "Verification record submitted successfully", Id = id, VerificationRecord = dto });
        }

        // 5. Upload verification document metadata (JSON)
        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromBody] VerificationRecordDto dto)
        {
            if (dto.UserId == Guid.Empty)
            {
                dto.UserId = GetCurrentUserId();
            }
            var id = await mediator.Send(new UploadDocumentCommand(dto));
            dto.Id = id;
            return Ok(new { Message = "Verification record uploaded successfully", Id = id, VerificationRecord = dto });
        }

        // 4. Admin List Verification Records
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetVerificationRecords([FromQuery] PagedRequest request)
        {
            var result = await mediator.Send(new GetVerificationRecordsQuery(request));
            return Ok(result);
        }

        // 5. Admin Approve Verification Record
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("approve/{recordId}")]
        public async Task<IActionResult> ApproveVerification(Guid recordId)
        {
            var id = await mediator.Send(new ApproveVerificationCommand(recordId, GetCurrentUserId()));
            return Ok(new { Message = "Verification approved successfully", Id = id });
        }

        // 6. Delete Verification Record
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("delete/{recordId}")]
        public async Task<IActionResult> DeleteVerification(Guid recordId)
        {
            var id = await mediator.Send(new DeleteVerificationCommand(recordId, GetCurrentUserId()));
            return Ok(new { Message = "Verification record deleted", Id = id });
        }
    }
}
