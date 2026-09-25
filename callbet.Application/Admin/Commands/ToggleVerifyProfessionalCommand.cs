using System;
using MediatR;

namespace callbet.Application.Admin.Commands;

public record ToggleVerifyProfessionalCommand(Guid UserId, Guid AdminUserId) : IRequest<bool>;
