using System.Collections.Generic;
using callbet.Domain.Entities;

namespace callbet.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwt(User user, IList<string> roles);
}
