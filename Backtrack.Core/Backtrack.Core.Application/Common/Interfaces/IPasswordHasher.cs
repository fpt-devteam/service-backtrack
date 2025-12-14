using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Common.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}