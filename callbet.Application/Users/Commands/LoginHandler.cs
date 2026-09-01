using MediatR;
using callbet.Application.Interfaces;
using callbet.Application.DTOs;
using callbet.Application.Services; // ✅ instead of Infrastructure

namespace callbet.Application.Users.Commands;

public class LoginHandler(ICustomerService service) : IRequestHandler<LoginCommand, UserDto?>
{
    private readonly CryptoService _crypto = new();

    public async Task<UserDto?> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await service.GetByEmailAsync(request.Email, ct);
        if (user == null) return null;

        // Verify plain password against stored hash
        bool isValid = _crypto.VerifyPassword(request.Password, user.PasswordHash);
        return isValid ? user : null;
    }
}
