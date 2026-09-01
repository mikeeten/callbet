using MediatR;
using callbet.Application.DTOs;

namespace callbet.Application.Professionals.Commands;

public record AddCertificateCommand(CertificateDto Dto) : IRequest<Guid>;

