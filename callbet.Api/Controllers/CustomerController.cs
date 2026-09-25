using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Interfaces;
using MediatR;
using callbet.Application.Customer.Commands;
using callbet.Application.Customer.Queries;
using callbet.Application.Jobs.Commands;
using callbet.Application.Jobs.Queries;

using callbet.Domain.Entities;
using callbet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace callbet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/customer")]
    public class CustomerController(IMediator mediator, ICustomerService customerService, CallbetDbContext context) : ControllerBase
    {
        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
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

        // -------------------------------------------------------------
        // CUSTOMER PERSONAL PROFILE MANAGEMENT
        // -------------------------------------------------------------

        // 1. Get logged-in customer's personal profile (via JWT Bearer Token)
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(new { message = "User identity not found in token." });

            var profile = await customerService.GetCustomerProfileAsync(userId, ct);
            if (profile == null) return NotFound(new { message = "Customer profile not found." });

            return Ok(profile);
        }

        // 2. Get customer's personal profile by userId
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfileById(Guid userId, CancellationToken ct)
        {
            var profile = await customerService.GetCustomerProfileAsync(userId, ct);
            if (profile == null) return NotFound(new { message = "Customer profile not found." });

            return Ok(profile);
        }

        // 3. Update logged-in customer's profile info (Name, Phone, Photo)
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileDto dto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(new { message = "User identity not found in token." });

            var updated = await customerService.UpdateCustomerProfileAsync(userId, dto, ct);
            if (!updated) return NotFound(new { message = "Customer profile not found." });

            var profile = await customerService.GetCustomerProfileAsync(userId, ct);
            return Ok(new { message = "Profile updated successfully.", profile });
        }

        // 4. Update customer's profile by userId
        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfileById(Guid userId, [FromBody] UpdateCustomerProfileDto dto, CancellationToken ct)
        {
            var updated = await customerService.UpdateCustomerProfileAsync(userId, dto, ct);
            if (!updated) return NotFound(new { message = "Customer profile not found." });

            var profile = await customerService.GetCustomerProfileAsync(userId, ct);
            return Ok(new { message = "Profile updated successfully.", profile });
        }

        // 5. Upload profile photo for logged-in customer (multipart/form-data)
        [HttpPost("profile/photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMyProfilePhoto([FromForm] IFormFile? file, [FromServices] IWebHostEnvironment environment, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(new { message = "User identity not found in token." });

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No image file provided." });
            }

            // Max 5MB file size limit
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "Image size exceeds the 5MB limit." });
            }

            // Validate image extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed formats: .jpg, .jpeg, .png, .webp, .gif" });
            }

            var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{userId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            var photoUrl = $"{Request.Scheme}://{Request.Host}/uploads/profiles/{uniqueFileName}";
            var updated = await customerService.UpdateProfilePhotoUrlAsync(userId, photoUrl, ct);
            if (!updated)
            {
                return NotFound(new { message = "Customer profile not found." });
            }

            var profile = await customerService.GetCustomerProfileAsync(userId, ct);
            return Ok(new
            {
                message = "Profile photo uploaded successfully.",
                profilePhotoUrl = photoUrl,
                profile
            });
        }

        // 6. Upload profile photo by userId
        [HttpPost("profile/{userId}/photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePhotoById(Guid userId, [FromForm] IFormFile? file, [FromServices] IWebHostEnvironment environment, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No image file provided." });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "Image size exceeds the 5MB limit." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed formats: .jpg, .jpeg, .png, .webp, .gif" });
            }

            var webRoot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{userId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            var photoUrl = $"{Request.Scheme}://{Request.Host}/uploads/profiles/{uniqueFileName}";
            var updated = await customerService.UpdateProfilePhotoUrlAsync(userId, photoUrl, ct);
            if (!updated)
            {
                return NotFound(new { message = "Customer profile not found." });
            }

            var profile = await customerService.GetCustomerProfileAsync(userId, ct);
            return Ok(new
            {
                message = "Profile photo uploaded successfully.",
                profilePhotoUrl = photoUrl,
                profile
            });
        }

        // -------------------------------------------------------------
        // PUBLIC SERVICE BROWSING
        // -------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet("services")]
        public async Task<IActionResult> GetProfessionalServices([FromQuery] PagedRequest request)
        {
            var result = await mediator.Send(new GetPagedProfessionalServicesQuery(request));
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("services/paged")]
        public async Task<IActionResult> GetProfessionalServicesPaged([FromQuery] PagedRequest request)
        {
            var result = await mediator.Send(new GetPagedProfessionalServicesQuery(request));
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("service-categories")]
        [HttpGet("service-categories/paged")]
        public async Task<IActionResult> GetPagedServiceCategories([FromQuery] PagedRequest request)
        {
            var result = await mediator.Send(new GetPagedServiceCategoriesQuery(request));
            return Ok(result);
        }

        [HttpPost("review")]
        public async Task<IActionResult> SubmitReview([FromBody] ReviewDto dto)
        {
            var id = await mediator.Send(new CreateReviewCommand(dto));
            return Ok(new { Message = "Review submitted successfully and professional reputation updated", Id = id });
        }

        [AllowAnonymous]
        [HttpGet("reviews/{professionalId}")]
        public async Task<IActionResult> GetReviewsForProfessional(Guid professionalId)
        {
            var result = await mediator.Send(new GetReviewsForProfessionalQuery(professionalId));
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("reviews/job/{jobId}")]
        public async Task<IActionResult> GetReviewByJob(Guid jobId)
        {
            var result = await mediator.Send(new GetReviewByJobIdQuery(jobId));
            return Ok(result);
        }

        [HttpPost("address")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDto dto)
        {
            if (dto.UserId == Guid.Empty)
            {
                dto.UserId = GetCurrentUserId();
            }
            var id = await mediator.Send(new AddAddressCommand(dto));
            return Ok(new { Message = "Address added successfully", Id = id });
        }

        [HttpGet("addresses/{userId}")]
        public async Task<IActionResult> GetUserAddresses(Guid userId)
        {
            var result = await mediator.Send(new GetUserAddressesQuery(userId));
            return Ok(result);
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await customerService.GetUserAddressesAsync(userId, default);
            return Ok(result);
        }

        [HttpPut("address/{addressId}")]
        public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] AddressDto dto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            dto.Id = addressId;
            var success = await customerService.UpdateAddressAsync(userId, dto, ct);
            if (!success) return NotFound(new { message = "Address not found." });
            return Ok(new { message = "Address updated successfully." });
        }

        [HttpDelete("address/{addressId}")]
        public async Task<IActionResult> DeleteAddress(Guid addressId, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var success = await customerService.DeleteAddressAsync(userId, addressId, ct);
            if (!success) return NotFound(new { message = "Address not found." });
            return Ok(new { message = "Address deleted successfully." });
        }

        // -------------------------------------------------------------
        // FAVORITE PROFESSIONALS & SERVICES
        // -------------------------------------------------------------
        [HttpGet("favorite-professionals")]
        [HttpGet("my-favorite-professionals")]
        public async Task<IActionResult> GetMyFavoriteProfessionals()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await mediator.Send(new GetFavoriteProfessionalsQuery(userId));
            return Ok(result);
        }

        [HttpGet("favorite-professionals/{customerId}")]
        public async Task<IActionResult> GetFavoriteProfessionals(Guid customerId)
        {
            var result = await mediator.Send(new GetFavoriteProfessionalsQuery(customerId));
            return Ok(result);
        }

        [HttpPost("favorite-professional")]
        public async Task<IActionResult> AddFavoriteProfessional([FromBody] FavoriteProfessionalDto dto)
        {
            if (dto.CustomerId == Guid.Empty)
            {
                dto.CustomerId = GetCurrentUserId();
            }
            var id = await mediator.Send(new AddFavoriteProfessionalCommand(dto));
            return Ok(new { Message = "Professional added to favorites", Id = id });
        }

        [HttpDelete("favorite-professional/{profileId}")]
        public async Task<IActionResult> RemoveMyFavoriteProfessional(Guid profileId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await mediator.Send(new RemoveFavoriteProfessionalCommand(userId, profileId));
            return Ok(new { Message = "Professional removed from favorites", Success = result });
        }

        [HttpDelete("favorite-professional/{customerId}/{profileId}")]
        public async Task<IActionResult> RemoveFavoriteProfessional(Guid customerId, Guid profileId)
        {
            var result = await mediator.Send(new RemoveFavoriteProfessionalCommand(customerId, profileId));
            return Ok(new { Message = "Professional removed from favorites", Success = result });
        }

        [HttpGet("favorite-services")]
        [HttpGet("my-favorite-services")]
        public async Task<IActionResult> GetMyFavoriteServices()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await mediator.Send(new GetFavoriteServicesQuery(userId));
            return Ok(result);
        }

        [HttpGet("favorite-services/{customerId}")]
        public async Task<IActionResult> GetFavoriteServices(Guid customerId)
        {
            var result = await mediator.Send(new GetFavoriteServicesQuery(customerId));
            return Ok(result);
        }

        [HttpPost("favorite-service")]
        public async Task<IActionResult> AddFavoriteService([FromBody] FavoriteServiceDto dto)
        {
            if (dto.CustomerId == Guid.Empty)
            {
                dto.CustomerId = GetCurrentUserId();
            }
            var id = await mediator.Send(new AddFavoriteServiceCommand(dto));
            return Ok(new { Message = "Service added to favorites", Id = id });
        }

        [HttpDelete("favorite-service/{serviceId}")]
        public async Task<IActionResult> RemoveMyFavoriteService(Guid serviceId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await mediator.Send(new RemoveFavoriteServiceCommand(userId, serviceId));
            return Ok(new { Message = "Service removed from favorites", Success = result });
        }

        [HttpDelete("favorite-service/{customerId}/{serviceId}")]
        public async Task<IActionResult> RemoveFavoriteService(Guid customerId, Guid serviceId)
        {
            var result = await mediator.Send(new RemoveFavoriteServiceCommand(customerId, serviceId));
            return Ok(new { Message = "Service removed from favorites", Success = result });
        }
    }
}
