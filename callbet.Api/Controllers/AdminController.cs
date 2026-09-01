using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.DTOs;
using callbet.Application.Admin.Commands;
using callbet.Application.Admin.Queries;
using callbet.Application.Jobs.Commands;
using callbet.Application.Chat.Commands;

namespace callbet.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin")]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new GetUsersQuery(request));
        return Ok(result);
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
        var id = await mediator.Send(new ApproveProfessionalCommand(userId));
        return Ok(new { Message = "Professional approved", Id = id });
    }

    [HttpPut("suspend/{userId}")]
    public async Task<IActionResult> SuspendProfessional(Guid userId)
    {
        var id = await mediator.Send(new SuspendProfessionalCommand(userId));
        return Ok(new { Message = "Professional suspended", Id = id });
    }

    [HttpDelete("delete/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var id = await mediator.Send(new DeleteUserCommand(userId));
        return Ok(new { Message = "User deleted", Id = id });
    }

    [HttpDelete("review/{reviewId}")]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        var result = await mediator.Send(new DeleteReviewCommand(reviewId));
        return Ok(new { Message = "Review removed and action logged", Success = result });
    }

    [HttpDelete("chat/message/{messageId}")]
    public async Task<IActionResult> DeleteChatMessage(Guid messageId)
    {
        var result = await mediator.Send(new DeleteChatMessageCommand(messageId));
        return Ok(new { Message = "Chat message removed and action logged", Success = result });
    }

    [HttpPost("service-category")]
    public async Task<IActionResult> CreateServiceCategory([FromBody] ServiceCategoryDto dto)
    {
        var id = await mediator.Send(new CreateServiceCategoryCommand(dto));
        return Ok(new { Message = "Service category created", Id = id });
    }

    [HttpGet("service-categories")]
    public async Task<IActionResult> GetServiceCategories()
    {
        var result = await mediator.Send(new GetServiceCategoriesQuery());
        return Ok(result);
    }

    [HttpPost("service")]
    public async Task<IActionResult> CreateService([FromBody] ServiceDto dto)
    {
        var id = await mediator.Send(new CreateServiceCommand(dto));
        return Ok(new { Message = "Service created", Id = id });
    }

    [HttpGet("services/{categoryId}")]
    public async Task<IActionResult> GetServicesByCategory(Guid categoryId)
    {
        var result = await mediator.Send(new GetServicesByCategoryQuery(categoryId));
        return Ok(result);
    }

    [HttpPost("assign-service")]
    public async Task<IActionResult> AssignService([FromBody] ProfessionalServiceDto dto)
    {
        var id = await mediator.Send(new AssignServiceCommand(dto));
        return Ok(new { Message = "Service assigned to professional", Id = id });
    }

    [HttpGet("professional-services/{profileId}")]
    public async Task<IActionResult> GetProfessionalServices(Guid profileId)
    {
        var result = await mediator.Send(new GetProfessionalServicesQuery(profileId));
        return Ok(result);
    }

    [HttpPost("admin/register")]
    public async Task<IActionResult> RegisterAdmin([FromBody] UserDto dto)
    {
        var id = await mediator.Send(new RegisterAdminCommand(dto));
        return Ok(new { Message = "Admin registered successfully", Id = id });
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetAdminLogs([FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new GetAdminLogsQuery(request));
        return Ok(result);
    }
}
