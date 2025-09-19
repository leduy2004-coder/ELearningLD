using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IToken
    {
        Task<string> GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
