using System;
using MediatR;

namespace callbet.Application.Admin.Commands;

public record DismissReviewFlagCommand(Guid ReviewId, Guid AdminUserId) : IRequest<bool>;
