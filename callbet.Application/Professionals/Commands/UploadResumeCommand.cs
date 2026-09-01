using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Commands;

public record UploadResumeCommand(ResumeDto Dto) : IRequest<Guid>;
