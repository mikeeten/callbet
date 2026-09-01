// using MediatR;
// using callbet.Application.Interfaces;

// namespace callbet.Application.Professionals.Commands;

// public class RegisterProfessionalHandler(IProfessionalService service)
//     : IRequestHandler<RegisterProfessionalCommand, Guid>
// {
//     public async Task<Guid> Handle(RegisterProfessionalCommand request, CancellationToken ct)
//     {
//         return await service.RegisterProfessionalAsync(request.Dto, ct);
//     }
// }
