using MediatR;
using Microsoft.AspNetCore.Mvc;
using callbet.Application.Users.Commands;
using callbet.Application.DTOs;


namespace callbet.Api.Controllers;

[ApiController]
[Route("api/User")]
public class UsersRegistration(IMediator mediator) : ControllerBase
{
    [HttpPost("customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] UserDto dto)
    {
        var id = await mediator.Send(new RegisterCustomerCommand(dto));
        return Ok(new { Message = "Customer registered successfully", Id = id });
    }

    [HttpPost("professional")]
    public async Task<IActionResult> RegisterProfessional([FromBody] UserDto dto)
    {
        var id = await mediator.Send(new RegisterProfessionalCommand(dto));
        return Ok(new { Message = "Professional registered successfully", Id = id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
    var user = await mediator.Send(new LoginCommand(request.Email, request.Password));
    if (user == null)
    {
        return Unauthorized(new { detail = "Invalid credentials." });
    }

    return Ok(new
    {
        userId = user.Id,
        email = user.Email,
        firstName = user.FirstName,
        lastName = user.LastName,
        role = user.Role
    });
}
}
