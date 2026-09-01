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

namespace callbet.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/customer")]
    public class CustomerController(IMediator mediator, ICustomerService customerService) : ControllerBase
    {
        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
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
            var result = await mediator.Send(new GetUserAddressesQuery(userId));
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

        [HttpDelete("favorite-professional/{customerId}/{profileId}")]
        public async Task<IActionResult> RemoveFavoriteProfessional(Guid customerId, Guid profileId)
        {
            var result = await mediator.Send(new RemoveFavoriteProfessionalCommand(customerId, profileId));
            return Ok(new { Message = "Professional removed from favorites", Success = result });
        }

        [HttpGet("favorite-professionals/{customerId}")]
        public async Task<IActionResult> GetFavoriteProfessionals(Guid customerId)
        {
            var result = await mediator.Send(new GetFavoriteProfessionalsQuery(customerId));
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

        [HttpDelete("favorite-service/{customerId}/{serviceId}")]
        public async Task<IActionResult> RemoveFavoriteService(Guid customerId, Guid serviceId)
        {
            var result = await mediator.Send(new RemoveFavoriteServiceCommand(customerId, serviceId));
            return Ok(new { Message = "Service removed from favorites", Success = result });
        }

        [HttpGet("favorite-services/{customerId}")]
        public async Task<IActionResult> GetFavoriteServices(Guid customerId)
        {
            var result = await mediator.Send(new GetFavoriteServicesQuery(customerId));
            return Ok(result);
        }
    }
}
