using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Jobs.Commands;

public record CreatePaymentCommand(PaymentDto Dto) : IRequest<Guid>;