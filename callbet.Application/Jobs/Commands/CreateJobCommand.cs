using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Jobs.Commands;

public record CreateJobCommand(JobDto Dto) : IRequest<Guid>;